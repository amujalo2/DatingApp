using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Http;

namespace API.Helpers._User;

public class UserHelper
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IPhotoService _photoService;

    public UserHelper(IUnitOfWork unitOfWork, IMapper mapper, IPhotoService photoService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _photoService = photoService;
    }

    public async Task<PagedList<MemberDto>> GetUsers(UserParams userParams, string currentUsername)
    {
        userParams.CurrentUsername = currentUsername;
        return await _unitOfWork.UserRepository.GetMembersAsync(userParams);
    }

    public async Task<MemberDto> GetUser(string username, string currentUsername)
    {
        return await _unitOfWork.UserRepository.GetMemberAsync(username, isCurrentUser: currentUsername == username);
    }

    public async Task<(bool Success, string? ErrorMessage)> UpdateUser(MemberUpdateDto memberUpdateDto, string username)
    {
        var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);
        
        if (user == null) 
            return (false, "User not found");
        
        _mapper.Map(memberUpdateDto, user);
        
        if (await _unitOfWork.Complete()) 
            return (true, null);
        
        return (false, "Failed to update user");
    }

    public async Task<(bool Success, string? ErrorMessage, PhotoDto? Photo)> AddPhoto(IFormFile file, string username)
    {
        var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);
        
        if (user == null) 
            return (false, "Cannot update user", null);
        
        var result = await _photoService.AddPhotoAsync(file);
        
        if (result.Error != null) 
            return (false, result.Error.Message, null);
        
        var photo = new Photo
        {
            Url = result.SecureUrl.AbsoluteUri,
            PublicId = result.PublicId
        };
        
        user.Photos.Add(photo);
        
        if (await _unitOfWork.Complete())
        {
            return (true, null, _mapper.Map<PhotoDto>(photo));
        }
        
        return (false, "Problem adding photo", null);
    }

    public async Task<(bool Success, string? ErrorMessage)> SetMainPhoto(int photoId, string username)
    {
        var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);
        
        if (user == null) 
            return (false, "User not found");
        
        var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);
        
        if (photo == null) 
            return (false, "Photo not found");
            
        if (photo.IsMain) 
            return (false, "This is already your main photo");
        
        var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);
        
        if (currentMain != null) 
            currentMain.IsMain = false;
            
        photo.IsMain = true;

        if (await _unitOfWork.Complete()) 
            return (true, null);
            
        return (false, "Failed to set main photo");
    }

    public async Task<(bool Success, string? ErrorMessage)> DeletePhoto(int photoId, string username)
    {
        var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);
        
        if (user == null) 
            return (false, "User not found");
        
        var photo = await _unitOfWork.PhotoRepository.GetPhotoById(photoId);
        
        if (photo == null) 
            return (false, "Photo not found");
        
        if (photo.IsMain) 
            return (false, "You cannot delete your main photo");
        
        if (photo.PublicId != null)
        {
            var result = await _photoService.DeletePhotoAsync(photo.PublicId);
            
            if (result.Error != null) 
                return (false, result.Error.Message);
        }
        
        user.Photos.Remove(photo);
        
        if (await _unitOfWork.Complete()) 
            return (true, null);
            
        return (false, "Failed to delete the photo");
    }
}