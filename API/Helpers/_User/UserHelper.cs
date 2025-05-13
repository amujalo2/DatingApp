using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Errors;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Http;

namespace API.Helpers._User;

public class UserHelper(IUnitOfWork unitOfWork, IMapper mapper, IPhotoService photoService)
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly IPhotoService _photoService = photoService;

    public async Task<PagedList<MemberDto>> GetUsers(UserParams userParams, string currentUsername)
    {
        userParams.CurrentUsername = currentUsername;
        return await _unitOfWork.UserRepository.GetMembersAsync(userParams);
    }

    public async Task<MemberDto> GetUser(string username, string currentUsername)
    {
        var user = await _unitOfWork.UserRepository.GetMemberAsync(username, isCurrentUser: currentUsername == username) ?? throw new NotFoundException("User not found");
        return user;
    }

    public async Task UpdateUser(MemberUpdateDto memberUpdateDto, string username)
    {
        var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username) ?? throw new NotFoundException("User not found");
        
        _mapper.Map(memberUpdateDto, user);

        if (!await _unitOfWork.Complete())
            throw new Exception("Failed to update user");
    }

    public async Task<PhotoDto> AddPhoto(IFormFile file, string username)
    {
        var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username) ?? throw new NotFoundException("User not found");
        
        var result = await _photoService.AddPhotoAsync(file);
        if (result.Error != null)
            throw new BadRequestException(result.Error.Message);

        var photo = new Photo
        {
            Url = result.SecureUrl.AbsoluteUri,
            PublicId = result.PublicId
        };

        user.Photos.Add(photo);

        if (!await _unitOfWork.Complete())
            throw new Exception("Problem adding photo");

        return _mapper.Map<PhotoDto>(photo);
    }

    public async Task SetMainPhoto(int photoId, string username)
    {
        var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username) ?? throw new NotFoundException("User not found");
        
        var photo = user.Photos.FirstOrDefault(x => x.Id == photoId) ?? throw new NotFoundException("Photo not found");
        
        if (photo.IsMain)
            throw new BadRequestException("This is already your main photo");

        var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);
        if (currentMain != null)
            currentMain.IsMain = false;

        photo.IsMain = true;

        if (!await _unitOfWork.Complete())
            throw new Exception("Failed to set main photo");
    }

    public async Task DeletePhoto(int photoId, string username)
    {
        var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username) ?? throw new NotFoundException("User not found");
        
        var photo = await _unitOfWork.PhotoRepository.GetPhotoById(photoId) ?? throw new NotFoundException("Photo not found");
        
        if (photo.IsMain)
            throw new BadRequestException("You cannot delete your main photo");

        if (photo.PublicId != null)
        {
            var result = await _photoService.DeletePhotoAsync(photo.PublicId);
            if (result.Error != null)
                throw new BadRequestException(result.Error.Message);
        }

        user.Photos.Remove(photo);

        if (!await _unitOfWork.Complete())
            throw new Exception("Failed to delete the photo");
    }
}