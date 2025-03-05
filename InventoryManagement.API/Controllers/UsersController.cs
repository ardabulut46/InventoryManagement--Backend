using System.Security.Claims;
using AutoMapper;
using InventoryManagement.Core.DTOs.Auth;
using InventoryManagement.Core.DTOs.Permission;
using InventoryManagement.Core.DTOs.User;
using InventoryManagement.Core.Entities;
using InventoryManagement.Core.Interfaces;
using InventoryManagement.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using Microsoft.EntityFrameworkCore;


//kayıt olunmicak admin izin vericek 

namespace InventoryManagement.API.Controllers
{
    // API/Controllers/UsersController.cs
    //[Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly IPermissionRepository _permissionRepository;
        private readonly IGenericRepository<Group> _groupRepository;
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<Role> _roleManager;

        public UsersController(
            IUserRepository userRepository,
            IMapper mapper,
            UserManager<User> userManager,
            IPermissionRepository permissionRepository,
            IGenericRepository<Group> groupRepository,
            ApplicationDbContext context,
            RoleManager<Role> roleManager)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _userManager = userManager;
            _permissionRepository = permissionRepository;
            _groupRepository = groupRepository;
            _context = context;
            _roleManager = roleManager;
        }
        
        
        //[Authorize(Policy = "CanView")]
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


        //[Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<UserDto>> CreateUser(CreateUserDto createUserDto)
        {
            if (await _userManager.FindByEmailAsync(createUserDto.Email) != null)
                return BadRequest("Email zaten kullanımda");

            // Get the group first
            var group = await _groupRepository.GetByIdAsync(createUserDto.GroupId ?? 0);
            if (group == null)
                return BadRequest("Geçersiz grup");

            // Verify the role exists
            if (!string.IsNullOrEmpty(createUserDto.Role))
            {
                var roleExists = await _roleManager.RoleExistsAsync(createUserDto.Role);
                if (!roleExists)
                    return BadRequest($"Rol '{createUserDto.Role}' bulunamadı");
            }

            var user = _mapper.Map<User>(createUserDto);
            user.UserName = createUserDto.Email;
            user.GroupId = group.Id;
            user.DepartmentId = group.DepartmentId;

            var result = await _userManager.CreateAsync(user, createUserDto.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            // Assign the user to the specified role
            if (!string.IsNullOrEmpty(createUserDto.Role))
            {
                var roleResult = await _userManager.AddToRoleAsync(user, createUserDto.Role);
                if (!roleResult.Succeeded)
                    return BadRequest(roleResult.Errors);
            }

            // Create basic permission for backward compatibility
            var permission = new Permission
            {
                UserId = user.Id,
                CanView = createUserDto.CanView,
                CanCreate = createUserDto.CanCreate,
                CanEdit = createUserDto.CanEdit,
                CanDelete = createUserDto.CanDelete
            };

            await _permissionRepository.AddAsync(permission);

            // Get user with permissions for response
            var userWithPermissions = await _userManager.FindByIdAsync(user.Id.ToString());
            var userDto = _mapper.Map<UserDto>(userWithPermissions);

            // Add basic permissions to response
            userDto.Permissions = new PermissionDto
            {
                CanView = permission.CanView,
                CanCreate = permission.CanCreate,
                CanEdit = permission.CanEdit,
                CanDelete = permission.CanDelete
            };

            // Include role information in the response
            if (!string.IsNullOrEmpty(createUserDto.Role))
            {
                // Get role details to include permissions in the response
                var role = await _roleManager.FindByNameAsync(createUserDto.Role);
                if (role != null)
                {
                    var rolePermissions = await _context.RolePermissions
                        .Where(rp => rp.RoleId == role.Id)
                        .Select(rp => rp.Permission)
                        .ToListAsync();

                    userDto.Role = createUserDto.Role;
                    userDto.RolePermissions = rolePermissions;
                }
            }

            return CreatedAtAction(nameof(GetUser),
                new { id = user.Id },
                userDto);
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

        [Authorize(Policy = "Users:Delete")]
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

