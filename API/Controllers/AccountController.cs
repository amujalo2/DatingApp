using API.DTOs;
using API.Entities;
using API.Interfaces;
using API.Services._Account;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace API.Controllers;

public class AccountController(IAccountService accountService, ILogger<AccountController> logger) : BaseApiController
{
    private readonly IAccountService _accountHelper = accountService;
    private readonly ILogger<AccountController> _logger = logger;

    /// <summary>
    /// POST: /api/account/register
    /// Registers a new user with the provided registration details.
    /// </summary>
    /// <param name="registerDto"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(typeof(ActionResult<UserDto?>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesErrorResponseType(typeof(void))]
    public async Task<ActionResult<UserDto?>> Register(RegisterDto registerDto)
    {
        try
        {
            _logger.LogDebug($"AccountController - {nameof(Register)} invoked. (registerDto: ${registerDto})");
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
    /// POST: /api/account/login
    /// Logs in a user with the provided login credentials and returns user details along with a token.
    /// </summary>
    /// <param name="loginDto"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(ActionResult<UserDto?>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesErrorResponseType(typeof(void))]
    public async Task<ActionResult<UserDto?>> Login(LoginDto loginDto)
    {
        try
        {
            _logger.LogDebug($"AccountController - {nameof(Login)} invoked. (loginDto: ${loginDto})");
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