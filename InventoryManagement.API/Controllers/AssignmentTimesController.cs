using AutoMapper;
using InventoryManagement.Core.DTOs.AssignmentTime;
using InventoryManagement.Core.Entities;
using InventoryManagement.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryManagement.Infrastructure.Data;

namespace InventoryManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] 
    public class AssignmentTimesController : ControllerBase
    {
        private readonly IGenericRepository<AssignmentTime> _assignmentTimeRepository;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;

        public AssignmentTimesController(
            IGenericRepository<AssignmentTime> assignmentTimeRepository,
            IMapper mapper,
            ApplicationDbContext context)
        {
            _assignmentTimeRepository = assignmentTimeRepository;
            _mapper = mapper;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AssignmentTimeDto>>> GetAssignmentTimes()
        {
            var assignmentTimes = await _assignmentTimeRepository.GetAllWithIncludesAsync("ProblemType");
            return Ok(_mapper.Map<IEnumerable<AssignmentTimeDto>>(assignmentTimes));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AssignmentTimeDto>> GetAssignmentTime(int id)
        {
            var assignmentTime = await _assignmentTimeRepository.GetByIdWithIncludesAsync(id, "ProblemType");
            
            if (assignmentTime == null)
                return NotFound();

            return Ok(_mapper.Map<AssignmentTimeDto>(assignmentTime));
        }

        [HttpPost]
        public async Task<ActionResult<AssignmentTimeDto>> CreateAssignmentTime(CreateUpdateAssignmentTimeDto createDto)
        {
            // Validate if problem type exists
            var problemType = await _context.ProblemTypes
                .FirstOrDefaultAsync(p => p.Id == createDto.ProblemTypeId && p.IsActive);
            if (problemType == null)
            {
                return BadRequest("Invalid or inactive problem type");
            }

            // Check if assignment time already exists for this problem type
            var existingAssignment = await _assignmentTimeRepository
                .SearchAsync(at => at.ProblemTypeId == createDto.ProblemTypeId);
            if (existingAssignment.Any())
            {
                return BadRequest("An assignment time already exists for this problem type");
            }

            var assignmentTime = new AssignmentTime
            {
                ProblemTypeId = createDto.ProblemTypeId,
                TimeToAssign = createDto.TimeToAssign
            };

            var createdAssignmentTime = await _assignmentTimeRepository.AddAsync(assignmentTime);
            
            return CreatedAtAction(
                nameof(GetAssignmentTime),
                new { id = createdAssignmentTime.Id },
                _mapper.Map<AssignmentTimeDto>(createdAssignmentTime));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAssignmentTime(int id, CreateUpdateAssignmentTimeDto updateDto)
        {
            var assignmentTime = await _assignmentTimeRepository.GetByIdAsync(id);
            if (assignmentTime == null)
                return NotFound();

            // Validate if problem type exists
            var problemType = await _context.ProblemTypes
                .FirstOrDefaultAsync(p => p.Id == updateDto.ProblemTypeId && p.IsActive);
            if (problemType == null)
            {
                return BadRequest("Invalid or inactive problem type");
            }

            // Check if assignment time already exists for this problem type (excluding current record)
            var existingAssignment = await _assignmentTimeRepository
                .SearchAsync(at => at.ProblemTypeId == updateDto.ProblemTypeId && at.Id != id);
            if (existingAssignment.Any())
            {
                return BadRequest("An assignment time already exists for this problem type");
            }

            assignmentTime.ProblemTypeId = updateDto.ProblemTypeId;
            assignmentTime.TimeToAssign = updateDto.TimeToAssign;

            await _assignmentTimeRepository.UpdateAsync(assignmentTime);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAssignmentTime(int id)
        {
            var assignmentTime = await _assignmentTimeRepository.GetByIdAsync(id);
            if (assignmentTime == null)
                return NotFound();

            await _assignmentTimeRepository.DeleteAsync(id);
            return NoContent();
        }

        [HttpGet("problem-type/{problemTypeId}")]
        public async Task<ActionResult<AssignmentTimeDto>> GetByProblemType(int problemTypeId)
        {
            var assignmentTime = await _assignmentTimeRepository
                .SearchWithIncludesAsync(
                    at => at.ProblemTypeId == problemTypeId,
                    "ProblemType");

            var result = assignmentTime.FirstOrDefault();
            if (result == null)
                return NotFound();

            return Ok(_mapper.Map<AssignmentTimeDto>(result));
        }
    }
}