using AutoMapper;
using InventoryManagement.Core.DTOs.Auth;
using InventoryManagement.Core.DTOs.User;
using InventoryManagement.Core.Entities;
using InventoryManagement.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;

    public AuthController(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        ITokenService tokenService,
        IMapper mapper)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _mapper = mapper;
    }

    /*
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto registerDto)
    {
        if (await _userManager.FindByEmailAsync(registerDto.Email) != null)
            return BadRequest("Email already exists");

        if (await _userManager.FindByNameAsync(registerDto.Username) != null)
            return BadRequest("Username already exists");

        var user = _mapper.Map<User>(registerDto);
        user.UserName = registerDto.Username;
        
        var result = await _userManager.CreateAsync(user, registerDto.Password);

        if (!result.Succeeded) return BadRequest(result.Errors);

        return new AuthResponseDto
        {
            Token = _tokenService.CreateToken(user),
            User = _mapper.Map<UserDto>(user)
        };
    }
    */

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto loginDto)
    {
        var user = await _userManager.FindByEmailAsync(loginDto.Email);
        if (user == null) return Unauthorized("Geçersiz email veya şifre");

        var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
        if (!result.Succeeded) return Unauthorized("Geçersiz email veya şifre");

        return new AuthResponseDto
        {
            Token = await _tokenService.CreateToken(user),  
            User = _mapper.Map<UserDto>(user)
        };
    }
}