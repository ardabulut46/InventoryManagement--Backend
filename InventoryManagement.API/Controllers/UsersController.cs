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
            var userDtos = new List<UserDto>();

            foreach (var user in users)
            {
                var userDto = _mapper.Map<UserDto>(user);

                // Ensure Group properties are set correctly
                if (user.Group != null)
                {
                    userDto.GroupId = user.GroupId;
                    userDto.GroupName = user.Group.Name; // Assuming Group has a Name property
                }

                // Get user roles
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Any())
                {
                    userDto.Role = roles.First(); // Assuming a user has one primary role

                    // Get role details
                    var role = await _roleManager.FindByNameAsync(userDto.Role);
                    if (role != null)
                    {
                        userDto.RoleId = role.Id;

                        // Get role permissions
                        var rolePermissions = await _context.RolePermissions
                            .Where(rp => rp.RoleId == role.Id)
                            .ToListAsync();

                        userDto.RolePermissions = rolePermissions.Select(rp => rp.Permission).ToList();
                    }
                }

                userDtos.Add(userDto);
            }

            return Ok(userDtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return NotFound();

            var userDto = _mapper.Map<UserDto>(user);

            // Ensure Group properties are set correctly
            if (user.Group != null)
            {
                userDto.GroupId = user.GroupId;
                userDto.GroupName = user.Group.Name; // Assuming Group has a Name property
            }

            // Get user roles
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Any())
            {
                userDto.Role = roles.First(); // Assuming a user has one primary role

                // Get role details
                var role = await _roleManager.FindByNameAsync(userDto.Role);
                if (role != null)
                {
                    userDto.RoleId = role.Id;

                    // Get role permissions
                    var rolePermissions = await _context.RolePermissions
                        .Where(rp => rp.RoleId == role.Id)
                        .ToListAsync();

                    userDto.RolePermissions = rolePermissions.Select(rp => rp.Permission).ToList();
                }
            }

            return Ok(userDto);
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
            var role = await _roleManager.FindByIdAsync(createUserDto.RoleId.ToString());
            if (role == null)
                return BadRequest($"Rol ID '{createUserDto.RoleId}' bulunamadı");

            var user = _mapper.Map<User>(createUserDto);
            user.UserName = createUserDto.Email;
            user.GroupId = group.Id;
            user.DepartmentId = group.DepartmentId;

            var result = await _userManager.CreateAsync(user, createUserDto.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            // Assign the user to the role
            var roleResult = await _userManager.AddToRoleAsync(user, role.Name);
            if (!roleResult.Succeeded)
                return BadRequest(roleResult.Errors);

            // Get user with permissions for response
            var userWithPermissions = await _userManager.FindByIdAsync(user.Id.ToString());
            var userDto = _mapper.Map<UserDto>(userWithPermissions);

            // Include role information in the response
            var rolePermissions = await _context.RolePermissions
                .Where(rp => rp.RoleId == role.Id)
                .Select(rp => rp.Permission)
                .ToListAsync();

            userDto.Role = role.Name;
            userDto.RoleId = role.Id;
            userDto.RolePermissions = rolePermissions;

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

            // Check if email is already in use by another user
            var existingUser = await _userManager.FindByEmailAsync(updateUserDto.Email);
            if (existingUser != null && existingUser.Id != id)
                return BadRequest("Email zaten kullanımda");

            // Get the group if provided
            if (updateUserDto.GroupId.HasValue)
            {
                var group = await _groupRepository.GetByIdAsync(updateUserDto.GroupId.Value);
                if (group == null)
                    return BadRequest("Geçersiz grup");

                user.GroupId = group.Id;
                user.DepartmentId = group.DepartmentId;
            }

            // Verify the role exists
            var role = await _roleManager.FindByIdAsync(updateUserDto.RoleId.ToString());
            if (role == null)
                return BadRequest($"Rol ID '{updateUserDto.RoleId}' bulunamadı");

            // Get current roles
            var currentRoles = await _userManager.GetRolesAsync(user);

            // Remove from current roles
            if (currentRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
            }

            // Add to new role
            var roleResult = await _userManager.AddToRoleAsync(user, role.Name);
            if (!roleResult.Succeeded)
                return BadRequest(roleResult.Errors);

            // Update basic user properties
            user.Name = updateUserDto.Name;
            user.Surname = updateUserDto.Surname;
            user.Email = updateUserDto.Email;
            user.UserName = updateUserDto.Email; // Keep username in sync with email
            user.Location = updateUserDto.Location;
            user.Floor = updateUserDto.Floor;
            user.Room = updateUserDto.Room;
            user.City = updateUserDto.City;
            user.District = updateUserDto.District;
            user.Address = updateUserDto.Address;
            user.IsActive = updateUserDto.IsActive;

            // Handle password update if provided
            if (!string.IsNullOrEmpty(updateUserDto.Password))
            {
                // Remove current password
                var removePasswordResult = await _userManager.RemovePasswordAsync(user);
                if (!removePasswordResult.Succeeded)
                    return BadRequest(removePasswordResult.Errors);

                // Add new password
                var addPasswordResult = await _userManager.AddPasswordAsync(user, updateUserDto.Password);
                if (!addPasswordResult.Succeeded)
                    return BadRequest(addPasswordResult.Errors);
            }

            // Update the user
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return BadRequest(updateResult.Errors);

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

