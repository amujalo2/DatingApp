using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using API.Interfaces;
using API.Entities;
using API.Services;
using Xunit;
using API.Services._Admin;
using Microsoft.AspNetCore.SignalR;
using API.SignalR;
using API.Errors;

namespace API.Tests
{
    public class PhotoApprovalTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IPhotoRepository> _photoRepoMock;
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IPhotoService> _photoServiceMock;
        private readonly Mock<UserManager<AppUser>> _userManagerMock;
        private readonly Mock<IHubContext<PresenceHub>> _hubContext;
        private readonly AdminService _adminService;

        public PhotoApprovalTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _photoRepoMock = new Mock<IPhotoRepository>();
            _userRepoMock = new Mock<IUserRepository>();
            _photoServiceMock = new Mock<IPhotoService>();
            _userManagerMock = new Mock<UserManager<AppUser>>(
                Mock.Of<IUserStore<AppUser>>(), null, null, null, null, null, null, null, null
            );
            _hubContext = new Mock<IHubContext<PresenceHub>>();

            _unitOfWorkMock.Setup(u => u.PhotoRepository).Returns(_photoRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.UserRepository).Returns(_userRepoMock.Object);

            _adminService = new AdminService(
                _userManagerMock.Object,
                _unitOfWorkMock.Object,
                _photoServiceMock.Object,
                _hubContext.Object
            );
        }

        [Fact]
        public async Task ApprovePhotoAsync_ApprovesPhoto_WhenPhotoExists()
        {
            // Arrange
            var appUserId = 2;
            var photo = CreateTestPhoto(appUserId: appUserId, isMain: false, isApproved: false, id: 1);
            var user = CreateTestUser(appUserId, photo);

            SetupMocksForSuccessfulApproval(photo, user);

            // Act
            await _adminService.ApprovePhoto(photo.Id);

            // Assert
            Assert.True(photo.IsApproved);
            _unitOfWorkMock.Verify(u => u.Complete(), Times.Once);
        }

        [Fact]
        public async Task ApprovePhotoAsync_SetAsMain_WhenNoMainPhotoExists()
        {
            // Arrange
            var appUserId = 2;
            var photo = CreateTestPhoto(appUserId: appUserId);
            var user = CreateTestUser(appUserId, photo);

            SetupMocksForSuccessfulApproval(photo, user);

            // Act
            await _adminService.ApprovePhoto(photo.Id);

            // Assert
            Assert.True(photo.IsMain);
            Assert.True(photo.IsApproved);
            _unitOfWorkMock.Verify(u => u.Complete(), Times.Once);
        }

        [Fact]
        public async Task ApprovePhotoAsync_DoesNotSetAsMain_WhenMainPhotoAlreadyExists()
        {
            // Arrange
            var appUserId = 2;
            var mainPhoto = CreateTestPhoto(id: 1, isMain: true, isApproved: true, appUserId: appUserId);
            var photoToApprove = CreateTestPhoto(id: 2, isMain: false, isApproved: false, appUserId: appUserId);
            var user = CreateTestUser(appUserId, mainPhoto, photoToApprove);

            SetupMocksForSuccessfulApproval(photoToApprove, user);

            // Act
            await _adminService.ApprovePhoto(photoToApprove.Id);

            // Assert
            Assert.False(photoToApprove.IsMain);
            Assert.True(photoToApprove.IsApproved);
            Assert.True(mainPhoto.IsMain); // Main photo should remain unchanged
        }

        [Fact]
        public async Task ApprovePhotoAsync_ThrowsNotFoundException_WhenPhotoNotFound()
        {
            // Arrange
            const int nonExistentPhotoId = 999;
            _photoRepoMock.Setup(r => r.GetPhotoById(nonExistentPhotoId))
                          .ReturnsAsync((Photo?)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(
                () => _adminService.ApprovePhoto(nonExistentPhotoId));
            _unitOfWorkMock.Verify(u => u.Complete(), Times.Never);
        }

        [Fact]
        public async Task ApprovePhotoAsync_ThrowsNotFoundException_WhenProblemApproving()
        {
            // Arrange
            var appUserId = 2;
            var photo = CreateTestPhoto(appUserId: appUserId);
            var user = CreateTestUser(appUserId, photo);

            _photoRepoMock.Setup(r => r.GetPhotoById(photo.Id)).ReturnsAsync(photo);
            _photoRepoMock.Setup(r => r.GetUserByPhotoId(photo.Id)).ReturnsAsync(user);
            _unitOfWorkMock.Setup(u => u.Complete()).ReturnsAsync(false); // Simulate failure

            // Act & Assert
            var ex = await Assert.ThrowsAsync<NotFoundException>(
                () => _adminService.ApprovePhoto(photo.Id));
            Assert.Equal("Failed to approve photo", ex.Message);
            _unitOfWorkMock.Verify(u => u.Complete(), Times.Once); // Promena: Times.Once umesto Times.Never
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-999)]
        public async Task ApprovePhotoAsync_ThrowsNotFoundException_WhenPhotoIdIsInvalid(int invalidId)
        {
            // Arrange
            _photoRepoMock.Setup(r => r.GetPhotoById(invalidId))
                          .ReturnsAsync((Photo?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => _adminService.ApprovePhoto(invalidId));

            Assert.Equal("Photo not found", exception.Message);
        }

        [Fact]
        public async Task ApprovePhotoAsync_ThrowsNotFoundException_WhenUserNotFound()
        {
            // Arrange
            var appUserId = 2;
            var photo = CreateTestPhoto(appUserId: appUserId);

            _photoRepoMock.Setup(r => r.GetPhotoById(photo.Id)).ReturnsAsync(photo);
            _photoRepoMock.Setup(r => r.GetUserByPhotoId(photo.Id))
                         .ReturnsAsync((AppUser?)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<NotFoundException>(
                () => _adminService.ApprovePhoto(photo.Id));
            Assert.Equal("User not found", ex.Message);
        }

        #region Helper Methods

        private static Photo CreateTestPhoto(int id = 1, bool isMain = false, bool isApproved = false, int appUserId = 2)
        {
            return new Photo
            {
                Id = id,
                AppUserId = appUserId,
                IsApproved = isApproved,
                IsMain = isMain,
                Url = $"https://test.com/photo{id}.jpg"
            };
        }

        private static AppUser CreateTestUser(int appUserId, params Photo[] photos)
        {
            return new AppUser
            {
                Id = appUserId,
                UserName = "testuser",
                KnownAs = "Test User",
                Gender = "male",
                City = "TestCity",
                Country = "TestCountry",
                Photos = photos.ToList()
            };
        }

        private void SetupMocksForSuccessfulApproval(Photo photo, AppUser user)
        {
            _photoRepoMock.Setup(r => r.GetPhotoById(photo.Id)).ReturnsAsync(photo);
            _photoRepoMock.Setup(r => r.GetUserByPhotoId(photo.Id)).ReturnsAsync(user); // OVO JE KLJUČNO
            _unitOfWorkMock.Setup(u => u.Complete()).ReturnsAsync(true);
        }

        #endregion
    }
}