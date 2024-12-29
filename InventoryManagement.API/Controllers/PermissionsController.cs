using InventoryManagement.Core.DTOs.Permission;
using InventoryManagement.Core.Entities;
using InventoryManagement.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PermissionsController : ControllerBase
    {
        private readonly IPermissionRepository _permissionRepository;

        public PermissionsController(IPermissionRepository permissionRepository)
        {
            _permissionRepository = permissionRepository;
        }

        // GET: api/permissions/{userId}
        [HttpGet("{userId}")]
        public async Task<ActionResult<Permission>> GetPermission(int userId)
        {
            var permission = await _permissionRepository.GetByUserIdAsync(userId);
            if (permission == null)
                return NotFound();

            return Ok(permission);
        }

        // PUT: api/permissions/{userId}
        [HttpPut("{userId}")]
        public async Task<IActionResult> UpdatePermission(int userId, [FromBody] PermissionUpdateDto updatedPermissions)
        {
            var permission = await _permissionRepository.GetByUserIdAsync(userId);
            if (permission == null)
                return NotFound();

            permission.CanView = updatedPermissions.CanView;
            permission.CanCreate = updatedPermissions.CanCreate;
            permission.CanEdit = updatedPermissions.CanEdit;
            permission.CanDelete = updatedPermissions.CanDelete;

            await _permissionRepository.UpdateAsync(permission);
            return NoContent();
        }
    }
}