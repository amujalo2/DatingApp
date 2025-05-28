using System;

using API.DTOs;
using API.Entities;

namespace API.Interfaces;

public interface IPhotoRepository
{
    Task<IEnumerable<PhotoForApprovalDto>> GetUnapprovedPhotos();
    Task<Photo?> GetPhotoById(int id);
    Task<AppUser?> GetUserByPhotoId(int photoId);
    Task<IEnumerable<string>> GetTagsAsStrings();
    Task<Photo?> GetPhotoWithTagsById(int id);
    Task<List<Photo>> GetPhotosByUsername(string username);
    Task<List<string>> GetUsersWithoutMainPhotoAsync(int currentUserId);
    Task<List<PhotoApprovalStatisticsDto>> GetPhotoApprovalStatisticsAsync(int currentUserId);
    Task<IEnumerable<TagDto>> GetTags();
    void AddTag(Tag tag);
    void RemovePhoto(Photo photo);
}
