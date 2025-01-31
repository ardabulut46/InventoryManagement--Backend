// InventoryManagement.API/Controllers/DepartmentPermissionsController.cs

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryManagement.Core.Entities;
using InventoryManagement.Infrastructure.Data; // ya da eğer repository ile gidersen IPermissionRepository gibi
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using InventoryManagement.Core.DTOs.Department;

namespace InventoryManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DepartmentPermissionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        
        public DepartmentPermissionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/departmentpermissions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DepartmentPermission>>> GetAll()
        {
            var list = await _context.DepartmentPermissions
                .Include(dp => dp.Department)
                .ToListAsync();
            return Ok(list);
        }

        // GET: api/departmentpermissions/5  (5 => departmentId)
        [HttpGet("{departmentId}")]
        public async Task<ActionResult<DepartmentPermission>> GetByDepartmentId(int departmentId)
        {
            var dp = await _context.DepartmentPermissions
                .Include(x => x.Department)
                .FirstOrDefaultAsync(x => x.DepartmentId == departmentId);

            if (dp == null)
                return NotFound("Departmanın permission kaydı yok.");

            return Ok(dp);
        }

        // POST: api/departmentpermissions
        [HttpPost]
        public async Task<ActionResult<DepartmentPermission>> Create([FromBody] DepartmentPermissionDto dto)
        {
            var department = await _context.Departments.FindAsync(dto.DepartmentId);
            if (department == null)
                return NotFound("Department not found");

            var alreadyExists = await _context.DepartmentPermissions
                .AnyAsync(x => x.DepartmentId == dto.DepartmentId);
            if (alreadyExists)
                return BadRequest("Bu departmanın zaten permission kaydı var.");

            var permission = new DepartmentPermission
            {
                DepartmentId = dto.DepartmentId,
                CanView = dto.CanView,
                CanCreate = dto.CanCreate,
                CanEdit = dto.CanEdit,
                CanDelete = dto.CanDelete
            };

            _context.DepartmentPermissions.Add(permission);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetByDepartmentId), 
                new { departmentId = permission.DepartmentId }, permission);
        }

        // PUT: api/departmentpermissions/5
        // 5 => departmentId. Body'de updated permission gelecek.
        [HttpPut("{departmentId}")]
        public async Task<IActionResult> Update(int departmentId, [FromBody] DepartmentPermission updated)
        {
            var existing = await _context.DepartmentPermissions
                .FirstOrDefaultAsync(x => x.DepartmentId == departmentId);

            if (existing == null)
                return NotFound("Bu departmanın permission kaydı bulunamadı.");
            
            existing.CanView = updated.CanView;
            existing.CanCreate = updated.CanCreate;
            existing.CanEdit = updated.CanEdit;
            existing.CanDelete = updated.CanDelete;
            
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
