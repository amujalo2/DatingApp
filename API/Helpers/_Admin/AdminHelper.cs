using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using API.SignalR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace API.Helpers._Admin;

public class AdminHelper
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPhotoService _photoService;
    private readonly IHubContext<PresenceHub> _hubContext;

    public AdminHelper(UserManager<AppUser> userManager, IUnitOfWork unitOfWork, IPhotoService photoService, IHubContext<PresenceHub> hubContext)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _photoService = photoService;
        _hubContext = hubContext;
    }

    public async Task<List<object>> GetUsersWithRoles()
    {
        var users = await _userManager.Users
            .OrderBy(x => x.UserName)
            .Select(x => new
            {
                x.Id,
                Username = x.UserName,
                Roles = x.UserRoles.Select(r => r.Role.Name).ToList()
            }).ToListAsync();
        
        return users.Cast<object>().ToList();
    }

    public async Task<(bool Success, string? ErrorMessage, IList<string>? Roles)> EditRoles(string username, string roles)
    {
        if (string.IsNullOrEmpty(roles))
            return (false, "You must select at least one role", null);

        var selectedRoles = roles.Split(',').ToArray();
        var user = await _userManager.FindByNameAsync(username);
        
        if (user == null)
            return (false, "User not found", null);

        var userRoles = await _userManager.GetRolesAsync(user);
        var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));
        
        if (!result.Succeeded)
            return (false, "Failed to add roles", null);

        result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));
        
        if (!result.Succeeded)
            return (false, "Failed to remove from roles", null);

        return (true, null, await _userManager.GetRolesAsync(user));
    }

    public async Task<IEnumerable<object>> GetPhotosForModeration()
    {
        return await _unitOfWork.PhotoRepository.GetUnapprovedPhotos();
    }

    public async Task<(bool Success, string? ErrorMessage)> ApprovePhoto(int photoId)
    {
        var photo = await _unitOfWork.PhotoRepository.GetPhotoById(photoId);
        
        if (photo == null)
            return (false, "Could not get photo from photo id");

        photo.IsApproved = true;
        var user = await _unitOfWork.PhotoRepository.GetUserByPhotoId(photoId);
        
        if (user == null)
            return (false, "Could not get user from photoID");

        if (!user.Photos.Any(x => x.IsMain))
            photo.IsMain = true;

        if (await _unitOfWork.Complete())
        {
            // SignalR: Obavesti korisnika
#pragma warning disable CS8604 // Possible null reference argument.
            var connections = await PresenceTracker.GetConnectionsForUser(user.UserName);
#pragma warning restore CS8604 // Possible null reference argument.
            if (connections != null && connections.Count > 0)
            {
                await _hubContext.Clients.Clients(connections).SendAsync("PhotoApproved", new
                {
                    message = "Your photo has been approved!"
                });
            }

            return (true, null);
        }
        
        return (false, "Failed to approve photo");
    }

    public async Task<(bool Success, string? ErrorMessage)> RejectPhoto(int photoId)
    {
        var photo = await _unitOfWork.PhotoRepository.GetPhotoById(photoId);
        
        if (photo == null)
            return (false, "Could not get photo from photo id");

        if (photo.PublicId != null)
        {
            var result = await _photoService.DeletePhotoAsync(photo.PublicId);
            
            if (result.Result == "ok")
            {
                _unitOfWork.PhotoRepository.RemovePhoto(photo);
            }
            else
            {
                return (false, "Failed to delete photo from cloud storage");
            }
        }
        else
        {
            _unitOfWork.PhotoRepository.RemovePhoto(photo);
        }

        var user = await _unitOfWork.PhotoRepository.GetUserByPhotoId(photoId);
        if (user == null)
            return (true, "Could not get user from photoID");

        if (await _unitOfWork.Complete())
        {
            // SignalR: Obavesti korisnika
#pragma warning disable CS8604 // Possible null reference argument.
            var connections = await PresenceTracker.GetConnectionsForUser(user.UserName);
#pragma warning restore CS8604 // Possible null reference argument.
            if (connections != null && connections.Count > 0)
            {
                await _hubContext.Clients.Clients(connections).SendAsync("PhotoRejected", new
                {
                    message = "Your photo has been rejected!"
                });
            }

            return (true, null);
        }
        return (true, "Failed to reject photo");
    }
}