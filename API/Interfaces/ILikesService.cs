using System;
using API.DTOs;
using API.Helpers;

namespace API.Interfaces;

public interface ILikesService
{
    Task<PagedList<MemberDto>> GetUserLikes(LikesParams likesParams);
    Task<IEnumerable<int>> GetCurrentUserLikeIds(int userId);
    Task ToggleLike(int sourceId, int targetUserId);
}
