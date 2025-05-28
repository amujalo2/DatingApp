using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Errors;
using API.Interfaces;
using API.SignalR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace API.Services._Admin;

public class AdminService(UserManager<AppUser> userManager, IUnitOfWork unitOfWork, IPhotoService photoService, IHubContext<PresenceHub> hubContext)
{
    private readonly UserManager<AppUser> _userManager = userManager;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IPhotoService _photoService = photoService;
    private readonly IHubContext<PresenceHub> _hubContext = hubContext;

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

    public async Task<IList<string>> EditRoles(string username, string roles)
    {
        if (string.IsNullOrEmpty(roles))
            throw new BadRequestException("You must select at least one role");

        var selectedRoles = roles.Split(',').ToArray();
        var user = await _userManager.FindByNameAsync(username) ?? throw new NotFoundException("User not found");

        var userRoles = await _userManager.GetRolesAsync(user);
        var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));

        if (!result.Succeeded)
            throw new BadRequestException("Failed to add role");

        result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));

        if (!result.Succeeded)
            throw new BadRequestException("Failed to remove role");

        return await _userManager.GetRolesAsync(user);
    }

    public async Task<IEnumerable<object>> GetPhotosForModeration()
    {
        return await _unitOfWork.PhotoRepository.GetUnapprovedPhotos();
    }

    public async Task ApprovePhoto(int photoId)
    {
        var photo = await _unitOfWork.PhotoRepository.GetPhotoById(photoId) ?? throw new NotFoundException("Photo not found");

        photo.IsApproved = true;
        var user = await _unitOfWork.PhotoRepository.GetUserByPhotoId(photoId) ?? throw new NotFoundException("User not found");

        if (!user.Photos.Any(x => x.IsMain))
            photo.IsMain = true;

        if (!await _unitOfWork.Complete())
            throw new Exception("Failed to approve photo");

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
    }

    public async Task RejectPhoto(int photoId)
    {
        var photo = await _unitOfWork.PhotoRepository.GetPhotoById(photoId) ?? throw new NotFoundException("Photo not found");

        if (photo.PublicId != null)
        {
            var result = await _photoService.DeletePhotoAsync(photo.PublicId);

            if (result.Result != "ok")
                throw new Exception("Failed to delete photo from cloud storage");
        }

        _unitOfWork.PhotoRepository.RemovePhoto(photo);

        var user = await _unitOfWork.PhotoRepository.GetUserByPhotoId(photoId) ?? throw new NotFoundException("User not found");

        if (!await _unitOfWork.Complete())
            throw new Exception("Failed to reject photo");

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
    }
    public async Task<object> CreateTagAsync(string tagName)
    {

        if (string.IsNullOrWhiteSpace(tagName))
        {
            throw new ArgumentException("Tag name cannot be null or empty.");
        }

        var tag = new Tag { Name = tagName };

        var existingTags = await _unitOfWork.TagRepository.GetAllTagsAsync();
        if (existingTags.Any(t => t.Name.Equals(tag.Name, StringComparison.OrdinalIgnoreCase)))
        {
            return "duplicate";
        }

        _unitOfWork.PhotoRepository.AddTag(tag);

        if (!await _unitOfWork.Complete())
        {
            throw new Exception("Problem creating tag.");
        }

        return tag;
    }
    public async Task<IEnumerable<object>> GetTagsAsync()
    {
        return await _unitOfWork.PhotoRepository.GetTags();
    }
    public async Task RemoveTagByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tag name is required.");
        await _unitOfWork.TagRepository.RemoveTagByName(name);
        if (!await _unitOfWork.Complete())
            throw new Exception("Failed to remove tag.");
    }

    public async Task<List<string>> GetUsersWithoutMainPhoto(int currentUserId)
    {
        var usersWithoutMainPhoto = await _unitOfWork.PhotoRepository.GetUsersWithoutMainPhotoAsync(currentUserId);
        if (usersWithoutMainPhoto == null || !usersWithoutMainPhoto.Any())
            throw new KeyNotFoundException("No users without main photo found.");
        return usersWithoutMainPhoto;
    }
    public async Task<List<PhotoApprovalStatisticsDto>> GetPhotoApprovalStatisticsAsync(int currentUserId)
    {
        var stats = await _unitOfWork.PhotoRepository.GetPhotoStatsApprovalAsync(currentUserId);
        if (stats == null || !stats.Any())
            throw new KeyNotFoundException("No photo approval stats found for the current user.");
        return stats.ToList();
    }
}