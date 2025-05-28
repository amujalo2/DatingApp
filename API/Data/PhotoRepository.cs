using System;
using System.Data;
using Api.DTOs;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class PhotoRepository(DataContext context, IMapper mapper) : IPhotoRepository
{
    public async Task<Photo?> GetPhotoById(int id)
    {
        return await context.Photos
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(x => x.Id == id);
    }
    public async Task<Photo?> GetPhotoWithTagsById(int id)
    {
        return await context.Photos
            .Include(p => p.PhotoTags)
            .ThenInclude(pt => pt.Tag)
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id);
    }
    public async Task<List<Photo>> GetPhotosByUsername(string username)
    {
        return await context.Photos
            .Where(p => p.AppUser.UserName == username)
            .Include(p => p.PhotoTags)
            .ThenInclude(pt => pt.Tag)
            .IgnoreQueryFilters()
            .ToListAsync();
    }
    public async Task<IEnumerable<PhotoForApprovalDto>> GetUnapprovedPhotos()
    {
        var query = context.Photos
            .IgnoreQueryFilters()
            .Where(p => p.IsApproved == false)
            .AsQueryable();
        return await query.ProjectTo<PhotoForApprovalDto>(mapper.ConfigurationProvider)
            .ToListAsync();
    }
    public async Task<AppUser?> GetUserByPhotoId(int photoId)
    {
        return await context.Users
            .Include(p => p.Photos)
            .IgnoreQueryFilters()
            .Where(p => p.Photos.Any(p => p.Id == photoId))
            .FirstOrDefaultAsync();
    }
    public void RemovePhoto(Photo photo)
    {
        context.Photos.Remove(photo);
    }
    public void AddTag(Tag tag)
    {
        context.Tags.Add(tag);
    }
    public async Task<IEnumerable<string>> GetTagsAsStrings()
    {
        return await context.Tags.Select(t => t.Name).ToListAsync();
    }
    public async Task<IEnumerable<TagDto>> GetTags()
    {
        var query = context.Tags.AsQueryable();
        return await query.ProjectTo<TagDto>(mapper.ConfigurationProvider).ToListAsync();
    }
    public async Task<List<string>> GetUsersWithoutMainPhotoAsync(int currentUserId)
    {
        var result = new List<string>();

        using var command = context.Database.GetDbConnection().CreateCommand();
        command.CommandText = "GetUsersWithoutMainPhoto";
        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add(new SqlParameter("@CurrentUserId", currentUserId));

        await context.Database.OpenConnectionAsync();

        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            result.Add(reader.GetString(0));
        }
        return result;
    }
    public async Task<List<PhotoApprovalStatisticsDto>> GetPhotoApprovalStatisticsAsync(int currentUserId)
    {
        var result = new List<PhotoApprovalStatisticsDto>();

        using var command = context.Database.GetDbConnection().CreateCommand();

        command.CommandText = "GetPhotoApprovalStatistics";
        command.CommandType = CommandType.StoredProcedure;

        var userIdParam = new SqlParameter("@CurrentUserId", currentUserId);
        command.Parameters.Add(userIdParam);

        await context.Database.OpenConnectionAsync();

        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            result.Add(new PhotoApprovalStatisticsDto
            {
                Username = reader.GetString(0),
                ApprovedPhotos = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                UnapprovedPhotos = reader.IsDBNull(2) ? 0 : reader.GetInt32(2)
            });
        }
        return result;
    }
}

