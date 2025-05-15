using System;
using System.Threading.Tasks;
using API.Entities;
using API.Interfaces;
using API.Services._Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using API.SignalR;

namespace API.Controllers;

public class AdminController(UserManager<AppUser> userManager, IUnitOfWork unitOfWork, IPhotoService photoService, IHubContext<PresenceHub> hubContext, ILogger<AdminController> logger) : BaseApiController
{
    private readonly AdminService _adminHelper = new AdminService(userManager, unitOfWork, photoService, hubContext);
    private readonly ILogger<AdminController> _logger = logger;

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "RequireAdminRole")]
    [HttpGet("users-with-roles")]
    public async Task<ActionResult> GetUsersWithRoles()
    {
        try
        {
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
    /// 
    /// </summary>
    /// <param name="username"></param>
    /// <param name="roles"></param>
    /// <returns></returns>
    [Authorize(Policy = "RequireAdminRole")]
    [HttpPost("edit-roles/{username}")]
    public async Task<ActionResult> EditRoles(string username, string roles)
    {
        try
        {
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
    /// 
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpGet("photos-to-moderate")]
    public async Task<ActionResult> GetPhotosForModeration()
    {
        try
        {
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
    /// 
    /// </summary>
    /// <param name="photoId"></param>
    /// <returns></returns>
    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpPost("approve-photo/{photoId}")]
    public async Task<ActionResult> ApprovePhoto(int photoId)
    {
        try
        {
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
    /// 
    /// </summary>
    /// <param name="photoId"></param>
    /// <returns></returns>
    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpPost("reject-photo/{photoId}")]
    public async Task<ActionResult> RejectPhoto(int photoId)
    {
        try
        {
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