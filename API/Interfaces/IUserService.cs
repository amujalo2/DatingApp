using Api.DTOs;
using API.DTOs;
using API.Helpers;

namespace API.Interfaces
{
    public interface IUserService
    {
        Task<List<TagDto>> GetTags(int photoId);
        Task<IEnumerable<object>> GetTags();
        Task AssignTags(int photoId, string v, List<string> tags);
        Task<PagedList<MemberDto>> GetUsers(UserParams userParams);
        Task<MemberDto> GetUserAsync(string username);
        Task<List<PhotoDto>> GetPhotoWithTagsByUsernameAsync(string username);
        Task UpdateUserAsync(string username, MemberUpdateDto memberUpdateDto);
        Task<PhotoDto> AddPhotoAsync(string username, IFormFile file);
        Task SetMainPhotoAsync(string username, int photoId);
        Task DeletePhotoAsync(string username, int photoId);
    }
}