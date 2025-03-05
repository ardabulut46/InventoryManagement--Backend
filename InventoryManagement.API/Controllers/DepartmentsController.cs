using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InventoryManagement.Core.DTOs;
using InventoryManagement.Core.DTOs.Department;
using InventoryManagement.Core.Entities;
using InventoryManagement.Core.Interfaces;

namespace InventoryManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
  //  [Authorize(Roles = "Admin")]
    public class DepartmentController : ControllerBase
    {
        private readonly IGenericRepository<Department> _departmentRepository;
        private readonly IMapper _mapper;

        public DepartmentController(
            IGenericRepository<Department> departmentRepository,
            IMapper mapper)
        {
            _departmentRepository = departmentRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DepartmentDto>>> GetDepartments()
        {
            var departments = await _departmentRepository.GetAllAsync();
            return Ok(_mapper.Map<IEnumerable<DepartmentDto>>(departments));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DepartmentDto>> GetDepartment(int id)
        {
            var department = await _departmentRepository.GetByIdAsync(id);
            
            if (department == null)
                return NotFound($"Department with ID {id} not found");

            return Ok(_mapper.Map<DepartmentDto>(department));
        }

        [HttpPost]
        //[Authorize(Policy = "CanCreate")]
        public async Task<ActionResult<DepartmentDto>> CreateDepartment(DepartmentDto departmentDto)
        {
            if (string.IsNullOrWhiteSpace(departmentDto.Name))
                return BadRequest("Department name cannot be empty");

            var department = _mapper.Map<Department>(departmentDto);
            var createdDepartment = await _departmentRepository.AddAsync(department);

            return CreatedAtAction(
                nameof(GetDepartment),
                new { id = createdDepartment.Id },
                _mapper.Map<DepartmentDto>(createdDepartment));
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "CanEdit")]
        public async Task<IActionResult> UpdateDepartment(int id, DepartmentDto departmentDto)
        {
            if (id != departmentDto.Id)
                return BadRequest("ID mismatch");

            var department = await _departmentRepository.GetByIdAsync(id);
            if (department == null)
                return NotFound($"Department with ID {id} not found");

            if (string.IsNullOrWhiteSpace(departmentDto.Name))
                return BadRequest("Department name cannot be empty");

            _mapper.Map(departmentDto, department);
            await _departmentRepository.UpdateAsync(department);

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "CanDelete")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            var department = await _departmentRepository.GetByIdAsync(id);
            if (department == null)
                return NotFound($"Department with ID {id} not found");

            await _departmentRepository.DeleteAsync(id);
            return NoContent();
        }
    }
}