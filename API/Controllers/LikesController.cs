using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using API.Errors;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using API.Services._Likes;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace API.Controllers;

public class LikesController(ILikesService likesService, ILogger<LikesController> logger) : BaseApiController
{
    private readonly ILikesService _likesService = likesService;
    private readonly ILogger<LikesController> _logger = logger;

    /// <summary>
    /// POST /api/likes/{targetUserId:int}
    /// Toggles a like for the specified user.
    /// </summary>
    /// <param name="targetUserId"></param>
    /// <returns></returns>
    /// [AllowAnonymous]
    [HttpPost("{targetUserId:int}")]
    [ProducesResponseType(typeof(ActionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesErrorResponseType(typeof(void))]
    public async Task<ActionResult> ToggleLike(int targetUserId)
    {
        try
        {
            _logger.LogDebug($"LikesController - {nameof(ToggleLike)} invoked. (targetUserId: {targetUserId})");
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
    /// GET /api/likes/list
    /// Retrieves a list of IDs of users that the current user has liked.
    /// </summary>
    /// <returns></returns>
    /// [AllowAnonymous]
    [HttpGet("list")]
    [ProducesResponseType(typeof(ActionResult<IEnumerable<int>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesErrorResponseType(typeof(void))]
    public async Task<ActionResult<IEnumerable<int>>> GetCurrentUserLikeIds()
    {
        try
        {
            _logger.LogDebug($"LikesController - {nameof(GetCurrentUserLikeIds)} invoked.");
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
    /// GET /api/likes?predicate={likesParams}
    /// Retrieves a list of users that the current user has liked or who have liked the current user based on the provided LikesParams.
    /// </summary>
    /// <param name="likesParams"></param>
    /// <returns></returns>
    /// [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(typeof(ActionResult<IEnumerable<MemberDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesErrorResponseType(typeof(void))]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUserLikes([FromQuery] LikesParams likesParams)
    {
        try
        {
            _logger.LogDebug($"LikesController - {nameof(GetUserLikes)} invoked. (likesParams: {likesParams})");
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