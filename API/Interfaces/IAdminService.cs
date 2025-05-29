using System;
using API.DTOs;

namespace API.Interfaces;

public interface IAdminService
{
    Task<List<object>> GetUsersWithRoles();
    Task<IList<string>> EditRoles(string username, string roles);
    Task<IEnumerable<object>> GetPhotosForModeration();
    Task ApprovePhoto(int photoId);
    Task RejectPhoto(int photoId);
    Task<object> CreateTagAsync(string tagName);
    Task<IEnumerable<object>> GetTagsAsync();
    Task RemoveTagByNameAsync(string name);
    Task<List<string>> GetUsersWithoutMainPhoto(int currentUserId);
    Task<List<PhotoApprovalStatisticsDto>> GetPhotoApprovalStatisticsAsync(int currentUserId);
}
