using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using API.Helpers._User;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace API.Controllers;
[Authorize]
public class UsersController : BaseApiController
{
    private readonly UserHelper _userHelper;
    private readonly IUnitOfWork _unitOfWork;

    public UsersController(IUnitOfWork unitOfWork, IMapper mapper, IPhotoService photoService)
    {
        _userHelper = new UserHelper(unitOfWork, mapper, photoService);
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery]UserParams userParams)
    {
        var users = await _userHelper.GetUsers(userParams, User.GetUsername());
        Response.AddPaginationHeader(users);
        return Ok(users);
    }
    
    [HttpGet("{username}")] 
    public async Task<ActionResult<MemberDto>> GetUser(string username)
    {
        var user = await _userHelper.GetUser(username, User.GetUsername());
        
        if (user == null) return NotFound();
        
        return user;
    }

    [HttpPut]
    public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
    {
        var (success, errorMessage) = await _userHelper.UpdateUser(memberUpdateDto, User.GetUsername());
        
        if (!success) return BadRequest(errorMessage);
        
        return NoContent();
    }

    [HttpPost("add-photo")]
    public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
    {
        var (success, errorMessage, photo) = await _userHelper.AddPhoto(file, User.GetUsername());
        
        if (!success) return BadRequest(errorMessage);
        
        return CreatedAtAction(nameof(GetUser), 
            new { username = User.GetUsername() }, 
            photo);
    }
    
    [HttpPut("set-main-photo/{photoId:int}")]
    public async Task<ActionResult> SetMainPhoto(int photoId)
    {
        var (success, errorMessage) = await _userHelper.SetMainPhoto(photoId, User.GetUsername());
        
        if (!success) return BadRequest(errorMessage);
        
        return NoContent();
    }

    [HttpDelete("delete-photo/{photoId:int}")]
    public async Task<ActionResult> DeletePhoto(int photoId)
    {
        var (success, errorMessage) = await _userHelper.DeletePhoto(photoId, User.GetUsername());
        
        if (!success) return BadRequest(errorMessage);
        
        return Ok();
    }
}