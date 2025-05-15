using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using API.Errors;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using API.Services._Likes;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class LikesController(IUnitOfWork unitOfWork, ILogger<LikesController> logger) : BaseApiController
{
    private readonly LikesService _likesService = new LikesService(unitOfWork);
    private readonly ILogger<LikesController> _logger = logger;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="targetUserId"></param>
    /// <returns></returns>
    [HttpPost("{targetUserId:int}")]
    public async Task<ActionResult> ToggleLike(int targetUserId)
    {
        try
        {
            var sourceId = User.GetUserId();
            await _likesService.ToggleLike(sourceId, targetUserId);
            return Ok();
        }
        catch (BadRequestException ex)
        {
            _logger.LogError(ex, "Exception in LikesController.ToggleLike");
            throw;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [HttpGet("list")]
    public async Task<ActionResult<IEnumerable<int>>> GetCurrentUserLikeIds()
    {
        try
        {
            var userId = User.GetUserId();
            var ids = await _likesService.GetCurrentUserLikeIds(userId);
            return Ok(ids);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in LikesController.GetCurrentUserLikeIds");
            throw;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="likesParams"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUserLikes([FromQuery] LikesParams likesParams)
    {
        try
        {
            likesParams.UserID = User.GetUserId();
            var users = await _likesService.GetUserLikes(likesParams);
            Response.AddPaginationHeader(users);
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in LikesController.GetUserLikes");
            throw;
        }
        
    }
}