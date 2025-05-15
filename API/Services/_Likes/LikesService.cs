using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Errors;
using API.Helpers;
using API.Interfaces;

namespace API.Services._Likes;

public class LikesService(IUnitOfWork unitOfWork)
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task ToggleLike(int sourceId, int targetUserId)
    {
        if (sourceId == targetUserId)
            throw new BadRequestException("You cannot like your self!");

        var existingLike = await _unitOfWork.LikesRepository.GetUserLike(sourceId, targetUserId);

        if (existingLike == null)
        {
            var like = new UserLike
            {
                SourceUserId = sourceId,
                LikedUserId = targetUserId
            };
            _unitOfWork.LikesRepository.AddLike(like);
        }
        else
        {
            _unitOfWork.LikesRepository.DeleteLike(existingLike);
        }

        if (!await _unitOfWork.Complete())
            throw new BadRequestException("Failed to update like");
    }

    public async Task<IEnumerable<int>> GetCurrentUserLikeIds(int userId)
    {
        return await _unitOfWork.LikesRepository.GetCurrentUserLikeIds(userId);
    }

    public async Task<PagedList<MemberDto>> GetUserLikes(LikesParams likesParams)
    {
        return await _unitOfWork.LikesRepository.GetUserLikes(likesParams);
    }
}