using System;
using System.Threading.Tasks;
using API.Entities;
using API.Interfaces;
using API.Helpers._Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using API.SignalR;

namespace API.Controllers;

public class AdminController : BaseApiController
{
    private readonly AdminHelper _adminHelper;

    public AdminController(UserManager<AppUser> userManager, IUnitOfWork unitOfWork, IPhotoService photoService, IHubContext<PresenceHub> hubContext)
    {
        _adminHelper = new AdminHelper(userManager, unitOfWork, photoService, hubContext);
    }

    [Authorize(Policy = "RequireAdminRole")]
    [HttpGet("users-with-roles")]
    public async Task<ActionResult> GetUsersWithRoles()
    {
        var users = await _adminHelper.GetUsersWithRoles();
        return Ok(users);
    }

    [Authorize(Policy = "RequireAdminRole")]
    [HttpPost("edit-roles/{username}")]
    public async Task<ActionResult> EditRoles(string username, string roles)
    {
        var updatedRoles = await _adminHelper.EditRoles(username, roles);
        return Ok(updatedRoles);
    }

    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpGet("photos-to-moderate")]
    public async Task<ActionResult> GetPhotosForModeration()
    {
        var photos = await _adminHelper.GetPhotosForModeration();
        return Ok(photos);
    }

    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpPost("approve-photo/{photoId}")]
    public async Task<ActionResult> ApprovePhoto(int photoId)
    {
        await _adminHelper.ApprovePhoto(photoId);
        return Ok();
    }

    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpPost("reject-photo/{photoId}")]
    public async Task<ActionResult> RejectPhoto(int photoId)
    {
        await _adminHelper.RejectPhoto(photoId);
        return Ok();
    }
}