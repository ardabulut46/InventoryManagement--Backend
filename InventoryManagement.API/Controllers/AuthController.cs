using AutoMapper;
using InventoryManagement.Core.DTOs.Auth;
using InventoryManagement.Core.DTOs.User;
using InventoryManagement.Core.Entities;
using InventoryManagement.Core.Services;
using InventoryManagement.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;
    private readonly ApplicationDbContext _context;
    private readonly RoleManager<Role> _roleManager;

    public AuthController(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        ITokenService tokenService,
        IMapper mapper,
        ApplicationDbContext context,
        RoleManager<Role> roleManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _mapper = mapper;
        _context = context;
        _roleManager = roleManager;
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

        // Enrich UserDto with role, permissions, and group info
        var userDto = _mapper.Map<UserDto>(user);

        // Set group info
        if (user.Group != null)
        {
            userDto.GroupId = user.GroupId;
            userDto.GroupName = user.Group.Name;
        }

        // Get user roles
        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Any())
        {
            userDto.Role = roles.First(); // Assuming a user has one primary role

            // Get role details using RoleManager
            var role = await _roleManager.FindByNameAsync(userDto.Role);
            if (role != null)
            {
                userDto.RoleId = role.Id;

                // Get role permissions
                var rolePermissions = await _context.RolePermissions
                    .Where(rp => rp.RoleId == role.Id)
                    .Select(rp => rp.Permission)
                    .ToListAsync();

                userDto.RolePermissions = rolePermissions;
            }
            else
            {
                userDto.RolePermissions = new List<string>();
            }
        }
        else
        {
            userDto.RolePermissions = new List<string>();
        }

        return new AuthResponseDto
        {
            Token = await _tokenService.CreateToken(user),
            User = userDto
        };
    }

    
}