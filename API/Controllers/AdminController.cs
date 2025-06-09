using API.Entities;
using API.Interfaces;
using API.Services._Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using API.SignalR;
using API.DTOs;
using Serilog;
using System.Security.Claims;

namespace API.Controllers;

public class AdminController(UserManager<AppUser> userManager, IUnitOfWork unitOfWork, IPhotoService photoService, IHubContext<PresenceHub> hubContext, ILogger<AdminController> logger) : BaseApiController
{
    private readonly IAdminService _adminHelper = new AdminService(userManager, unitOfWork, photoService, hubContext);
    private readonly ILogger<AdminController> _logger = logger;

    /// <summary>
    /// GET /api/admin/users-with-roles
    /// Retrieves a list of users along with their roles.
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
    /// Edits the roles of a user specified by username.
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
    /// Retrieves a list of photos that need moderation.
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
    /// Approves a photo for publication by its ID.
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
    /// Rejects a photo by its ID, removing it from moderation queue and deleting the photo.
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

    /// <summary>
    /// POST /api/admin/create-tag
    /// Creates a new tag with the specified name.
    /// </summary>
    /// <param name="tagDto"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// [Authorize(Policy = "ModeratePhotoRole")]
    [HttpPost("create-tag")]
    [ProducesResponseType(typeof(ActionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesErrorResponseType(typeof(void))]
    public async Task<ActionResult> CreateTag([FromBody] TagCreateDto tagDto)
    {
        try
        {
            _logger.LogDebug($"AdminController - {nameof(CreateTag)} invoked. (tagDto: {tagDto})");
            var createdTag = await _adminHelper.CreateTagAsync(tagDto?.Name?.Trim() ?? throw new ArgumentException("Tag name cannot be null or empty."));
            return Ok(createdTag);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in AdminController.CreateTag");
            throw;
        }
    }

    /// <summary>
    /// GET /api/admin/get-tags
    /// Retrieves a list of all tags.
    /// </summary>
    /// <returns></returns>
    /// [Authorize(Policy = "ModeratePhotoRole")]
    [HttpGet("get-tags")]
    [ProducesResponseType(typeof(ActionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesErrorResponseType(typeof(void))]
    public async Task<ActionResult> GetTags()
    {
        try
        {
            _logger.LogDebug($"AdminController - {nameof(GetTags)} invoked.");
            var tags = await _adminHelper.GetTagsAsync();
            return Ok(tags);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in AdminController.GetTags");
            throw;
        }
    }

    /// <summary>
    /// DELETE /api/admin/delete-tag/{name}
    /// Deletes a tag by its name.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// [Authorize(Policy = "ModeratePhotoRole")]
    [HttpDelete("delete-tag/{name}")]
    [ProducesResponseType(typeof(ActionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesErrorResponseType(typeof(void))]
    public async Task<ActionResult> DeleteTag(string name)
    {
        try
        {
            _logger.LogDebug($"AdminController - {nameof(DeleteTag)} invoked. (name: {name})");
            await _adminHelper.RemoveTagByNameAsync(name?.Trim() ?? throw new ArgumentException("Tag name cannot be null or empty."));
            return Ok(new { message = "Tag successfully removed." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in AdminController.DeleteTag");
            throw;
        }
    }

    /// <summary>
    /// GET /api/admin/users-without-main-photo
    /// Retrieves a list of usernames of users who do not have a main photo.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="UnauthorizedAccessException"></exception>
    /// <exception cref="KeyNotFoundException"></exception>
    [Authorize(Policy = "RequireAdminRole")]
    [HttpGet("users-without-main-photo")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesErrorResponseType(typeof(void))]
    public async Task<ActionResult<IEnumerable<string>>> GetUsersWithoutMainPhoto()
    {
        try
        {
            _logger.LogDebug($"AdminController - {nameof(GetUsersWithoutMainPhoto)} invoked.");
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("nameid")?.Value
                ?? throw new Exception("Cannot get user id from token!"));
            var users = await _adminHelper.GetUsersWithoutMainPhoto(userId) ?? throw new KeyNotFoundException("No users without main photo found.");
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in AdminController.GetUsersWithoutMainPhoto");
            throw;
        }
    }

    /// <summary>
    /// GET /api/admin/photo-stats
    /// Retrieves statistics about photos, including approved and unapproved counts for each user.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="UnauthorizedAccessException"></exception>
    /// <exception cref="KeyNotFoundException"></exception>
    [Authorize(Policy = "RequireAdminRole")]
    [HttpGet("photo-stats")]
    [ProducesResponseType(typeof(IEnumerable<PhotoApprovalStatisticsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesErrorResponseType(typeof(void))]
    public async Task<ActionResult<IEnumerable<PhotoApprovalStatisticsDto>>> GetPhotoStats()
    {
        try
        {
            _logger.LogDebug($"AdminController - {nameof(GetPhotoStats)} invoked.");
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("nameId")?.Value
                ?? throw new Exception("Cannot get user id from token!"));
            var photoStats = await _adminHelper.GetPhotoApprovalStatisticsAsync(userId) ?? throw new KeyNotFoundException("No photo stats found.");
            return Ok(photoStats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in AdminController.GetPhotoStats");
            throw;
        }
    }

}

