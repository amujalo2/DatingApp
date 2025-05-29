
using API.DTOs;
using API.Helpers;

namespace API.Interfaces
{
    public interface IUserService
    {
        Task<PagedList<MemberDto>> GetUsers(UserParams userParams, string currentUsername);
        Task<MemberDto> GetUser(string username, string currentUsername);
        Task UpdateUser(MemberUpdateDto memberUpdateDto, string username);
        Task<PhotoDto> AddPhoto(AddPhotoDto addPhotoDto, string username);
        Task SetMainPhoto(int photoId, string username);
        Task DeletePhoto(int photoId, string username);
        Task AssignTags(int photoId, string username, List<string> tagNames);
        Task<IEnumerable<object>> GetTags();
        Task<IEnumerable<TagDto>> GetTags(int photoId);
        Task<List<PhotoDto>> GetPhotoWithTagsByUsernameAsync(string username);
    }
}