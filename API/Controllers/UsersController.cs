using API.DTOs;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using API.Services._User;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace API.Controllers;

[Authorize]
public class UsersController(IUnitOfWork unitOfWork, IMapper mapper, IPhotoService photoService, ILogger<UsersController> logger) : BaseApiController
{
    private readonly UserService _userHelper = new UserService(unitOfWork, mapper, photoService);
    private readonly ILogger<UsersController> _logger = logger;

    /// <summary>
    /// GET /api/users?predicate={userParams}
    /// </summary>
    /// <param name="userParams"></param>
    /// <returns></returns>
    /// [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(typeof(ActionResult<IEnumerable<MemberDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesErrorResponseType(typeof(void))]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery] UserParams userParams)
    {
        try
        {
            _logger.LogDebug($"UsersController - {nameof(GetUsers)} invoked. (userParams: {userParams})");
            var users = await _userHelper.GetUsers(userParams, User.GetUsername());
            Response.AddPaginationHeader(users);
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in UsersController.GetUsers");
            throw;
        }
    }

    /// <summary>
    /// GET /api/users/{username}
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
    /// [AllowAnonymous]
    [HttpGet("{username}")]
    [ProducesResponseType(typeof(ActionResult<MemberDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesErrorResponseType(typeof(void))]
    public async Task<ActionResult<MemberDto>> GetUser(string username)
    {
        try
        {
            _logger.LogDebug($"UsersController - {nameof(GetUser)} invoked. (username: {username})");
            var user = await _userHelper.GetUser(username, User.GetUsername());
            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in UsersController.GetUser");
            throw;
        }
    }

    /// <summary>
    /// PUT /api/users
    /// </summary>
    /// <param name="memberUpdateDto"></param>
    /// <returns></returns>
    /// [AllowAnonymous]
    [HttpPut]
    [ProducesResponseType(typeof(ActionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesErrorResponseType(typeof(void))]
    public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
    {
        try
        {
            _logger.LogDebug($"UsersController - {nameof(UpdateUser)} invoked. (memberUpdateDto: {memberUpdateDto})");
            await _userHelper.UpdateUser(memberUpdateDto, User.GetUsername());
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in UsersController.UpdateUser");
            throw;
        }
    }

    /// <summary>
    /// POST /api/users/add-photo
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    /// [AllowAnonymous]
    [HttpPost("add-photo")]
    [ProducesResponseType(typeof(ActionResult<PhotoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesErrorResponseType(typeof(void))]
    public async Task<ActionResult<PhotoDto>> AddPhoto([FromForm] AddPhotoDto addPhotoDto)
    {
        try
        {
            _logger.LogDebug($"UsersController - {nameof(AddPhoto)} invoked. (file: {addPhotoDto})");
            var photo = await _userHelper.AddPhoto(addPhotoDto, User.GetUsername());
            return CreatedAtAction(nameof(GetUser), new { username = User.GetUsername() }, photo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in UsersController.AddPhoto");
            throw;
        }
    }

    /// <summary>
    /// PUT /api/users/set-main-photo/{photoId:int}
    /// </summary>
    /// <param name="photoId"></param>
    /// <returns></returns>
    /// [AllowAnonymous]
    [HttpPut("set-main-photo/{photoId:int}")]
    [ProducesResponseType(typeof(ActionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesErrorResponseType(typeof(void))]
    public async Task<ActionResult> SetMainPhoto(int photoId)
    {
        try
        {
            _logger.LogDebug($"UsersController - {nameof(SetMainPhoto)} invoked. (photoId: {photoId})");
            await _userHelper.SetMainPhoto(photoId, User.GetUsername());
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in UsersController.SetMainPhoto");
            throw;
        }
    }

    /// <summary>
    /// DELETE /api/users/delete-photo/{photoId:int}
    /// </summary>
    /// <param name="photoId"></param>
    /// <returns></returns>
    /// [AllowAnonymous]
    [HttpDelete("delete-photo/{photoId:int}")]
    [ProducesResponseType(typeof(ActionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesErrorResponseType(typeof(void))]
    public async Task<ActionResult> DeletePhoto(int photoId)
    {
        try
        {
            _logger.LogDebug($"UsersController - {nameof(DeletePhoto)} invoked. (photoId: {photoId})");
            await _userHelper.DeletePhoto(photoId, User.GetUsername());
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in UsersController.DeletePhoto");
            throw;
        }
    }
}