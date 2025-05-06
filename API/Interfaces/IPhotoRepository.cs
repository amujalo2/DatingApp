using System;
using API.DTOs;
using API.Entities;

namespace API.Interfaces;

public interface IPhotoRepository
{
    Task<IEnumerable<PhotoForApprovalDto>> GetUnapprovedPhotos();
    Task<Photo?> GetPhotoById(int id);
    Task<AppUser?> GetUserByPhotoId(int photoId); 
    void RemovePhoto(Photo photo);
}
