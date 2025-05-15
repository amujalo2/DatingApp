using API.DTOs;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using API.Services._User;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;
[Authorize]
public class UsersController(IUnitOfWork unitOfWork, IMapper mapper, IPhotoService photoService, ILogger<UsersController> logger) : BaseApiController
{
    private readonly UserService _userHelper = new UserService(unitOfWork, mapper, photoService);
    private readonly ILogger<UsersController> _logger = logger;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="userParams"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery]UserParams userParams)
    {
        try 
        {
            var users = await _userHelper.GetUsers(userParams, User.GetUsername());
            Response.AddPaginationHeader(users);
            return Ok(users);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Exception in UsersController.GetUsers");
            throw;
        }
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
    [HttpGet("{username}")] 
    public async Task<ActionResult<MemberDto>> GetUser(string username)
    {
        try
        {
            var user = await _userHelper.GetUser(username, User.GetUsername());
            return Ok(user);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Exception in UsersController.GetUser");
            throw;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="memberUpdateDto"></param>
    /// <returns></returns>
    [HttpPut]
    public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
    {
        try
        {
            await _userHelper.UpdateUser(memberUpdateDto, User.GetUsername());
            return NoContent();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Exception in UsersController.UpdateUser");
            throw;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    [HttpPost("add-photo")]
    public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
    {
        try 
        {
            var photo = await _userHelper.AddPhoto(file, User.GetUsername());
            return CreatedAtAction(nameof(GetUser), new { username = User.GetUsername() }, photo);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Exception in UsersController.AddPhoto");
            throw;
        }
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="photoId"></param>
    /// <returns></returns>
    [HttpPut("set-main-photo/{photoId:int}")]
    public async Task<ActionResult> SetMainPhoto(int photoId)
    {
        try
        {
            await _userHelper.SetMainPhoto(photoId, User.GetUsername());
            return NoContent();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Exception in UsersController.SetMainPhoto");
            throw;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="photoId"></param>
    /// <returns></returns>
    [HttpDelete("delete-photo/{photoId:int}")]
    public async Task<ActionResult> DeletePhoto(int photoId)
    {
        try
        {
            await _userHelper.DeletePhoto(photoId, User.GetUsername());
            return Ok();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Exception in UsersController.DeletePhoto");
            throw;
        }
    }
}