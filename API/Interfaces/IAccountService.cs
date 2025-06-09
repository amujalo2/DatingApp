using System;
using API.DTOs;

namespace API.Interfaces;

public interface IAccountService
{
    Task<UserDto> Register(RegisterDto registerDto);
    Task<UserDto> Login(LoginDto loginDto);
    Task<bool> UserExists(string username);
}
