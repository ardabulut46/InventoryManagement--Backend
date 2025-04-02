using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using InventoryManagement.Core.DTOs.DelayReason;
using InventoryManagement.Core.Entities;
using InventoryManagement.Core.Interfaces;
using InventoryManagement.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DelayReasonController : ControllerBase
    {
        private readonly IGenericRepository<DelayReason> _delayReasonRepository;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;

        public DelayReasonController(
            IGenericRepository<DelayReason> delayReasonRepository, 
            IMapper mapper, 
            ApplicationDbContext context)
        {
            _delayReasonRepository = delayReasonRepository;
            _mapper = mapper;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DelayReasonDto>>> GetDelayReasons()
        {
            var delayReasons = await _delayReasonRepository.GetAllAsync();
            return Ok(_mapper.Map<IEnumerable<DelayReasonDto>>(delayReasons));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DelayReasonDto>> GetDelayReason(int id)
        {
            var delayReason = await _delayReasonRepository.GetByIdAsync(id);
            if (delayReason == null)
            {
                return NotFound();
            }
            return Ok(_mapper.Map<DelayReasonDto>(delayReason));
        }

        [HttpPost]
        public async Task<ActionResult<DelayReasonDto>> CreateDelayReason(CreateDelayReasonDto createDto)
        {
            // Check if a delay reason with the same name already exists
            var existingDelayReason = await _context.DelayReasons.FirstOrDefaultAsync(d => d.Name == createDto.Name);
            if (existingDelayReason != null)
            {
                return BadRequest($"A delay reason with the name '{createDto.Name}' already exists.");
            }

            var delayReason = new DelayReason
            {
                Name = createDto.Name,
                IsActive = true
            };
            
            await _delayReasonRepository.AddAsync(delayReason);
            return CreatedAtAction(nameof(GetDelayReason), new { id = delayReason.Id }, _mapper.Map<DelayReasonDto>(delayReason));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDelayReason(int id, UpdateDelayReasonDto updateDto)
        {
            var existingDelayReason = await _delayReasonRepository.GetByIdAsync(id);
            if (existingDelayReason == null)
            {
                return NotFound();
            }

            // Check if another delay reason with the same name already exists
            var nameExists = await _context.DelayReasons.AnyAsync(d => d.Name == updateDto.Name && d.Id != id);
            if (nameExists)
            {
                return BadRequest($"Another delay reason with the name '{updateDto.Name}' already exists.");
            }

            existingDelayReason.Name = updateDto.Name;
            existingDelayReason.IsActive = updateDto.IsActive;
            existingDelayReason.UpdatedDate = DateTime.Now;

            await _delayReasonRepository.UpdateAsync(existingDelayReason);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDelayReason(int id)
        {
            var delayReason = await _delayReasonRepository.GetByIdAsync(id);
            if (delayReason == null)
            {
                return NotFound();
            }

            // Check if there are any solution reviews using this delay reason
            var hasReferences = await _context.SolutionReviews.AnyAsync(sr => sr.DelayReasonId == id);
            if (hasReferences)
            {
                // Instead of deleting, mark as inactive
                delayReason.IsActive = false;
                delayReason.UpdatedDate = DateTime.Now;
                await _delayReasonRepository.UpdateAsync(delayReason);
                return Ok(new { message = "Delay reason is in use and cannot be deleted. It has been marked as inactive instead." });
            }

            await _delayReasonRepository.DeleteAsync(id);
            return NoContent();
        }

        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<DelayReasonDto>>> GetActiveDelayReasons()
        {
            var delayReasons = await _context.DelayReasons.Where(d => d.IsActive).ToListAsync();
            return Ok(_mapper.Map<IEnumerable<DelayReasonDto>>(delayReasons));
        }
    }
}