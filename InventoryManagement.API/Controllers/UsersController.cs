using System.Security.Claims;
using AutoMapper;
using InventoryManagement.Core.DTOs.Auth;
using InventoryManagement.Core.DTOs.User;
using InventoryManagement.Core.Entities;
using InventoryManagement.Core.Interfaces;
using InventoryManagement.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


//kayıt olunmicak admin izin vericek 

namespace InventoryManagement.API.Controllers
{
    // API/Controllers/UsersController.cs
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly IPermissionRepository _permissionRepository;

        public UsersController(
            IUserRepository userRepository,
            IMapper mapper,
            UserManager<User> userManager,
            IPermissionRepository permissionRepository)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _userManager = userManager;
            _permissionRepository = permissionRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            var users = await _userRepository.GetAllAsync();
            return Ok(_mapper.Map<IEnumerable<UserDto>>(users));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return NotFound();

            return Ok(_mapper.Map<UserDto>(user));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<UserDto>> CreateUser(CreateUserDto createUserDto)
        {
            if (await _userManager.FindByEmailAsync(createUserDto.Email) != null)
                return BadRequest("Email zaten kullanımda");

            var user = _mapper.Map<User>(createUserDto);
            user.UserName = createUserDto.Email; // emaili, username yerine kullanıyoruz 
    
            var result = await _userManager.CreateAsync(user, createUserDto.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            await _userManager.AddToRoleAsync(user, createUserDto.Role);
            
            var permission = new Permission
            {
                UserId = user.Id,
                CanView = false,
                CanCreate = false,
                CanEdit = false,
                CanDelete = false
            };

            await _permissionRepository.AddAsync(permission);

            return CreatedAtAction(nameof(GetUser), 
                new { id = user.Id }, 
                _mapper.Map<UserDto>(user));
        }
        [HttpPost("change-password")]
        [Authorize]  // sadece giriş yapmış kullanıcılar şifre değiştirebilir
        public async Task<IActionResult> ChangePassword(ChangePasswordDto changePasswordDto)
        {
            // giriş yapmış kullanıcının ID'sini al
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound();

            // mevcut şifreyi kontrol et ve yeni şifreyi belirle
            var result = await _userManager.ChangePasswordAsync(user,
                changePasswordDto.CurrentPassword,
                changePasswordDto.NewPassword);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UpdateUserDto updateUserDto)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return NotFound();

            _mapper.Map(updateUserDto, user);
            await _userRepository.UpdateAsync(user);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return NotFound();

            await _userRepository.DeleteAsync(id);
            return NoContent();
        }
    }
    }

