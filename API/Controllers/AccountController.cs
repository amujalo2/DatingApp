using System;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using API.Helpers._Account;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class AccountController : BaseApiController
{
    private readonly AccountHelper _accountHelper;

    public AccountController(UserManager<AppUser> userManager, ITokenService tokenService, IMapper mapper)
    {
        _accountHelper = new AccountHelper(userManager, tokenService, mapper);
    }
    
    [HttpPost("register")] //account/register
    public async Task<ActionResult<UserDto?>> Register(RegisterDto registerDto)
    {
        var userDto = await _accountHelper.Register(registerDto);
        return Ok(userDto);
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto?>> Login(LoginDto loginDto)
    {
        var userDto = await _accountHelper.Login(loginDto);
        return Ok(userDto);
    }
}