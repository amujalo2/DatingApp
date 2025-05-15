using System;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using API.Services._Account;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace API.Controllers;

public class AccountController(UserManager<AppUser> userManager, ITokenService tokenService, IMapper mapper, ILogger<AccountController> logger) : BaseApiController
{
    private readonly AccountService _accountHelper = new AccountService(userManager, tokenService, mapper);
    private readonly ILogger<AccountController> _logger = logger;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="registerDto"></param>
    /// <returns></returns>
    [HttpPost("register")]
    public async Task<ActionResult<UserDto?>> Register(RegisterDto registerDto)
    {
        try
        {
            var userDto = await _accountHelper.Register(registerDto);
            return Ok(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in AccountController.Register");
            throw;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="loginDto"></param>
    /// <returns></returns>
    [HttpPost("login")]
    public async Task<ActionResult<UserDto?>> Login(LoginDto loginDto)
    {
        try
        {
            var userDto = await _accountHelper.Login(loginDto);
            return Ok(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in AccountController.Login");
            throw;
        }
    }
}