using System.Diagnostics.Contracts;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class LikesController(ILikesRepository likesRepository) : BaseApiController
{
    [HttpPost("{targetUserId:int}")]
    public async Task<ActionResult> ToggleLike(int targetUserId)
    {
        var sourceId = User.GetUserId();
        if (sourceId == targetUserId) return BadRequest("You cannot like your self!");
        var extensingLike = await likesRepository.GetUserLike(sourceId, targetUserId);
        if(extensingLike == null) 
        {
            var like = new UserLike 
            {
                SourceUserId = sourceId,
                TargetUserId = targetUserId
            };
            likesRepository.AddLike(like);
        }
        else
        {
            likesRepository.DeleteLike(extensingLike);
        }
        if (await likesRepository.SaveChanges()) return Ok();
        return BadRequest("Faild to update like");
    }
    [HttpGet("list")]
    public async Task<ActionResult<IEnumerable<int>>> GetCurrentUserLikeIds()
    {
        return Ok(await likesRepository.GetCurrentUserLikeIds(User.GetUserId()));
    }
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUserLikes([FromQuery]LikesParams likesParams)
    {
        likesParams.UserID = User.GetUserId();
        var users = await likesRepository.GetUserLikes(likesParams);
        Response.AddPaginationHeader(users);
        return Ok(users);
    }
}
