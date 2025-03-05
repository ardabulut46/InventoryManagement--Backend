// InventoryManagement.API/Controllers/RolesController.cs
using InventoryManagement.Core.Constants;
using InventoryManagement.Core.DTOs.Role;
using InventoryManagement.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InventoryManagement.Infrastructure.Data;

namespace InventoryManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
   // [Authorize(Policy = Policies.SuperAdmin)]
    public class RolesController : ControllerBase
    {
        private readonly RoleManager<Role> _roleManager;
        private readonly ApplicationDbContext _context;

        public RolesController(RoleManager<Role> roleManager, ApplicationDbContext context)
        {
            _roleManager = roleManager;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoleWithPermissionsDto>>> GetRoles()
        {
            try
            {
                // Get only roles that have permissions
                var rolesWithPermissions = await _context.RolePermissions
                    .Select(rp => rp.RoleId)
                    .Distinct()
                    .ToListAsync();

                var roles = await _roleManager.Roles
                    .Where(r => rolesWithPermissions.Contains(r.Id))
                    .ToListAsync();

                var roleWithPermissionsDtos = new List<RoleWithPermissionsDto>();

                foreach (var role in roles)
                {
                    // Get permissions for this role
                    var permissions = await _context.RolePermissions
                        .Where(rp => rp.RoleId == role.Id)
                        .Select(rp => new RolePermissionDto
                        {
                            Id = rp.Id,
                            Name = rp.Permission ?? string.Empty,
                            CreatedDate = rp.CreatedDate,
                            UpdatedDate = rp.UpdatedDate,
                            IsActive = rp.IsActive
                        })
                        .ToListAsync();

                    // Create DTO with NULL checks
                    var roleDto = new RoleWithPermissionsDto
                    {
                        Id = role.Id,
                        Name = role.Name ?? string.Empty,
                        Description = role.Description ?? string.Empty,
                        Permissions = permissions
                    };

                    roleWithPermissionsDtos.Add(roleDto);
                }

                return Ok(roleWithPermissionsDtos);
            }
            catch (Exception ex)
            {
                // Log the exception details
                Console.WriteLine($"Error in GetRoles: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    Console.WriteLine($"Inner stack trace: {ex.InnerException.StackTrace}");
                }

                return StatusCode(500, "An error occurred while retrieving roles. See server logs for details.");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RoleDto>> GetRole(int id)
        {
            var role = await _roleManager.Roles
                .Include(r => r.RolePermissions)
                .FirstOrDefaultAsync(r => r.Id == id);
                
            if (role == null)
                return NotFound();
                
            var roleDto = new RoleDto
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                Permissions = role.RolePermissions.Select(rp => rp.Permission).ToList()
            };
            
            return Ok(roleDto);
        }

        [HttpPost]
        public async Task<ActionResult<RoleDto>> CreateRole(CreateRoleDto createRoleDto)
        {
            var role = new Role
            {
                Name = createRoleDto.Name,
                Description = createRoleDto.Description,
                NormalizedName = createRoleDto.Name.ToUpper()
            };

            var result = await _roleManager.CreateAsync(role);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            // Add permissions
            foreach (var permission in createRoleDto.Permissions)
            {
                await _context.RolePermissions.AddAsync(new RolePermission
                {
                    RoleId = role.Id,
                    Permission = permission
                });
            }
            
            await _context.SaveChangesAsync();
            
            return CreatedAtAction(nameof(GetRole), new { id = role.Id }, 
                new RoleDto
                {
                    Id = role.Id,
                    Name = role.Name,
                    Description = role.Description,
                    Permissions = createRoleDto.Permissions
                });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(int id, UpdateRoleDto updateRoleDto)
        {
            var role = await _roleManager.Roles
                .Include(r => r.RolePermissions)
                .FirstOrDefaultAsync(r => r.Id == id);
                
            if (role == null)
                return NotFound();
                
            role.Name = updateRoleDto.Name;
            role.Description = updateRoleDto.Description;
            role.NormalizedName = updateRoleDto.Name.ToUpper();
            
            var result = await _roleManager.UpdateAsync(role);
            if (!result.Succeeded)
                return BadRequest(result.Errors);
                
            // Remove existing permissions
            _context.RolePermissions.RemoveRange(role.RolePermissions);
            
            // Add new permissions
            foreach (var permission in updateRoleDto.Permissions)
            {
                await _context.RolePermissions.AddAsync(new RolePermission
                {
                    RoleId = role.Id,
                    Permission = permission
                });
            }
            
            await _context.SaveChangesAsync();
            
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            var role = await _roleManager.FindByIdAsync(id.ToString());
            if (role == null)
                return NotFound();
                
            var result = await _roleManager.DeleteAsync(role);
            if (!result.Succeeded)
                return BadRequest(result.Errors);
                
            return NoContent();
        }
    }
}