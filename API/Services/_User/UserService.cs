using System;
using System.Collections.Generic;
using System.Formats.Tar;
using System.Threading.Tasks;

using API.DTOs;
using API.Entities;
using API.Errors;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Http;

namespace API.Services._User;

public class UserService(IUnitOfWork unitOfWork, IMapper mapper, IPhotoService photoService) : IUserService
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

    public async Task<PhotoDto> AddPhoto(AddPhotoDto addPhotoDto, string username)
    {
        var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username) ?? throw new NotFoundException("User not found");

        var result = await _photoService.AddPhotoAsync(addPhotoDto.File);
        if (result.Error != null)
            throw new BadRequestException(result.Error.Message);

        var photo = new Photo
        {
            Url = result.SecureUrl.AbsoluteUri,
            PublicId = result.PublicId
        };

        foreach (var tagId in addPhotoDto.TagIds)
        {
            photo.PhotoTags.Add(new PhotoTag
            {
                TagId = tagId
            });
        }

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
            if (photo.AppUserId != user.Id)
                throw new UnauthorizedAccessException("You cannot delete this photo.");
            // Tek sada pozovi _photoService.DeletePhotoAsync
            var result = await _photoService.DeletePhotoAsync(photo.PublicId);
            if (result.Error != null)
                throw new BadRequestException(result.Error.Message);
        }

        user.Photos.Remove(photo);

        if (!await _unitOfWork.Complete())
            throw new Exception("Failed to delete the photo");
    }

    public async Task AssignTags(int photoId, string username, List<string> tagNames)
    {
        var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username) ?? throw new KeyNotFoundException("User not found.");
        var photo = await _unitOfWork.PhotoRepository.GetPhotoWithTagsById(photoId) ?? throw new KeyNotFoundException("Photo not found.");

        // Ako je lista prazna, samo sačuvaj promene i izađi
        var distinctTagNames = tagNames
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Select(n => n.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        // Dohvati sve tagove iz baze koji su u listi
        var tagsFromDb = distinctTagNames.Count > 0
            ? await _unitOfWork.TagRepository.GetTagsByNamesAsync(distinctTagNames)
            : new List<Tag>();

        var tagsToRemove = photo.PhotoTags
            .Where(pt => pt.Tag != null && !distinctTagNames.Contains(pt.Tag.Name, StringComparer.OrdinalIgnoreCase))
            .ToList();
        
        foreach (var pt in tagsToRemove)
            photo.PhotoTags.Remove(pt);

        foreach (var tag in tagsFromDb)
        {
            if (!photo.PhotoTags.Any(pt => pt.TagId == tag.Id))
            {
                photo.PhotoTags.Add(new PhotoTag
                {
                    Photo = photo,
                    PhotoId = photo.Id,
                    Tag = tag,
                    TagId = tag.Id
                });
            }
        }

        if (!await _unitOfWork.Complete())
            throw new Exception("Failed to assign tags to the photo");
    }
    public async Task<IEnumerable<object>> GetTags()
    {
        try
        {
            return await _unitOfWork.PhotoRepository.GetTagsAsStrings();
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to retrievke tags", ex);
        }
    }
    public async Task<IEnumerable<TagDto>> GetTags(int photoId)
    {
        try
        {
            var photos = await _unitOfWork.PhotoRepository.GetPhotoWithTagsById(photoId)
                 ?? throw new NotFoundException("Photo not found");

            var tags = photos.PhotoTags.Select(pt => pt.Tag).ToList();

            return _mapper.Map<List<TagDto>>(tags);
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to retrieve tags", ex);
        }
    }
    public async Task<List<PhotoDto>> GetPhotoWithTagsByUsernameAsync(string username)
    {
        var photos = await _unitOfWork.PhotoRepository.GetPhotosByUsername(username) ?? throw new NotFoundException("Photos not found");
        return _mapper.Map<List<PhotoDto>>(photos);
    }
}