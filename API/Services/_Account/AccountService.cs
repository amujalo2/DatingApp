using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Errors;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Services._Account;

public class AccountService(UserManager<AppUser> userManager, ITokenService tokenService, IMapper mapper) : IAccountService
{
    private readonly UserManager<AppUser> _userManager = userManager;
    private readonly ITokenService _tokenService = tokenService;
    private readonly IMapper _mapper = mapper;

    public async Task<UserDto> Register(RegisterDto registerDto)
    {
        if (await UserExists(registerDto.Username))
            throw new BadRequestException("Username is already taken!");

        var user = _mapper.Map<AppUser>(registerDto);
        user.UserName = registerDto.Username.ToLower();

        var result = await _userManager.CreateAsync(user, registerDto.Password);
        if (!result.Succeeded) 
            throw new BadRequestException(string.Join(", ", result.Errors.Select(e => e.Description)));

        return new UserDto
        {
            Username = user.UserName,
            Token = await _tokenService.CreateToken(user),
            KnownAs = user.KnownAs,
            Gender = user.Gender,
        };
    }

    public async Task<UserDto> Login(LoginDto loginDto)
    {
        var user = await _userManager.Users
            .Include(p => p.Photos)
            .FirstOrDefaultAsync(x => x.NormalizedUserName!.Equals(loginDto.Username));

        if (user == null || user.UserName == null) 
            throw new UnauthorizedException("Invalid username!");

        var result = await _userManager.CheckPasswordAsync(user, loginDto.Password);
        if (!result) 
            throw new UnauthorizedException("Invalid password!");

        return new UserDto
        {
            Username = user.UserName,
            Token = await _tokenService.CreateToken(user),
            PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
            KnownAs = user.KnownAs,
            Gender = user.Gender
        };
    }

    public async Task<bool> UserExists(string username)
    {
        return await _userManager.Users.AnyAsync(x => x.NormalizedUserName == username.ToUpper());
    }
}