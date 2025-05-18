using API.Entities;
using API.Interfaces;
using API.Services._Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using API.SignalR;
using Serilog;

namespace API.Controllers;

public class AdminController(UserManager<AppUser> userManager, IUnitOfWork unitOfWork, IPhotoService photoService, IHubContext<PresenceHub> hubContext, ILogger<AdminController> logger) : BaseApiController
{
    private readonly AdminService _adminHelper = new AdminService(userManager, unitOfWork, photoService, hubContext);
    private readonly ILogger<AdminController> _logger = logger;

    /// <summary>
    /// GET /api/admin/users-with-roles
    /// </summary>
    /// <returns></returns>
    ///[AllowAnonymous]
    [Authorize(Policy = "RequireAdminRole")]
    [HttpGet("users-with-roles")]
    [ProducesResponseType(typeof(ActionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesErrorResponseType(typeof(void))]
    public async Task<ActionResult> GetUsersWithRoles()
    {
        try
        {
            _logger.LogDebug($"AdminController - {nameof(GetUsersWithRoles)} invoked.");
            var users = await _adminHelper.GetUsersWithRoles();
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in AdminController.GetUsersWithRoles");
            throw;
        }
    }

    /// <summary>
    /// POST /api/admin/edit-roles/{username}
    /// </summary>
    /// <param name="username"></param>
    /// <param name="roles"></param>
    /// <returns></returns>
    ///[AllowAnonymous]
    [HttpPost("edit-roles/{username}")]
    [Authorize(Policy = "RequireAdminRole")]
    [ProducesResponseType(typeof(ActionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesErrorResponseType(typeof(void))]
    public async Task<ActionResult> EditRoles(string username, string roles)
    {
        try
        {
            _logger.LogDebug($"AdminController - {nameof(EditRoles)} invoked. (username: {username}, roles: {roles})");
            var updatedRoles = await _adminHelper.EditRoles(username, roles);
            return Ok(updatedRoles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in AdminController.EditRoles");
            throw;
        }
    }

    /// <summary>
    /// GET /api/admin/photos-to-moderate
    /// </summary>
    /// <returns></returns>
    [HttpGet("photos-to-moderate")]
    [Authorize(Policy = "ModeratePhotoRole")]
    [ProducesResponseType(typeof(ActionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesErrorResponseType(typeof(void))]

    public async Task<ActionResult> GetPhotosForModeration()
    {
        try
        {
            _logger.LogDebug($"AdminController - {nameof(GetPhotosForModeration)} invoked.");
            var photos = await _adminHelper.GetPhotosForModeration();
            return Ok(photos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in AdminController.GetPhotosForModeration");
            throw;
        }
    }

    /// <summary>
    /// POST /api/admin/approve-photo/{photoId}
    /// </summary>
    /// <param name="photoId">
    /// lorem ipsum
    /// </param>
    /// <returns></returns>
    ///[AllowAnonymous]
    [HttpPost("approve-photo/{photoId}")]
    [Authorize(Policy = "ModeratePhotoRole")]
    [ProducesResponseType(typeof(ActionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesErrorResponseType(typeof(void))]
    public async Task<ActionResult> ApprovePhoto(int photoId)
    {
        try
        {
            _logger.LogDebug($"AdminController - {nameof(ApprovePhoto)} invoked. (photoId: {photoId}");
            await _adminHelper.ApprovePhoto(photoId);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in AdminController.ApprovePhoto");
            throw;
        }
    }

    /// <summary>
    /// POST /api/admin/reject-photo/{photoId}
    /// </summary>
    /// <param name="photoId"></param>
    /// <returns></returns>
    ///[AllowAnonymous]
    [HttpPost("reject-photo/{photoId}")]
    [Authorize(Policy = "ModeratePhotoRole")]
    [ProducesResponseType(typeof(ActionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesErrorResponseType(typeof(void))]
    public async Task<ActionResult> RejectPhoto(int photoId)
    {
        try
        {
            _logger.LogDebug($"AdminController - {nameof(RejectPhoto)} invoked. (photoId: {photoId}");
            await _adminHelper.RejectPhoto(photoId);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in AdminController.RejectPhoto");
            throw;
        }
    }
}