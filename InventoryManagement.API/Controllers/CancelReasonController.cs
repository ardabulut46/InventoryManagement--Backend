using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using InventoryManagement.Core.DTOs.CancelReason;
using InventoryManagement.Core.Entities;
using InventoryManagement.Core.Interfaces;
using InventoryManagement.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CancelReasonController : ControllerBase
    {
        private readonly IGenericRepository<CancelReason> _cancelReasonRepository;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;

        public CancelReasonController(
            IGenericRepository<CancelReason> cancelReasonRepository, 
            IMapper mapper, 
            ApplicationDbContext context)
        {
            _cancelReasonRepository = cancelReasonRepository;
            _mapper = mapper;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CancelReasonDto>>> GetCancelReasons()
        {
            var cancelReasons = await _cancelReasonRepository.GetAllAsync();
            return Ok(_mapper.Map<IEnumerable<CancelReasonDto>>(cancelReasons));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CancelReasonDto>> GetCancelReason(int id)
        {
            var cancelReason = await _cancelReasonRepository.GetByIdAsync(id);
            if (cancelReason == null)
            {
                return NotFound();
            }
            return Ok(_mapper.Map<CancelReasonDto>(cancelReason));
        }

        [HttpPost]
        public async Task<ActionResult<CancelReasonDto>> CreateCancelReason(CreateCancelReasonDto createDto)
        {
            // Check if a cancel reason with the same name already exists
            var existingCancelReason = await _context.CancelReasons.FirstOrDefaultAsync(c => c.Name == createDto.Name);
            if (existingCancelReason != null)
            {
                return BadRequest($"A cancel reason with the name '{createDto.Name}' already exists.");
            }

            var cancelReason = new CancelReason
            {
                Name = createDto.Name,
                IsActive = true
            };
            
            await _cancelReasonRepository.AddAsync(cancelReason);
            return CreatedAtAction(nameof(GetCancelReason), new { id = cancelReason.Id }, _mapper.Map<CancelReasonDto>(cancelReason));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCancelReason(int id, UpdateCancelReasonDto updateDto)
        {
            var existingCancelReason = await _cancelReasonRepository.GetByIdAsync(id);
            if (existingCancelReason == null)
            {
                return NotFound();
            }

            // Check if another cancel reason with the same name already exists
            var nameExists = await _context.CancelReasons.AnyAsync(c => c.Name == updateDto.Name && c.Id != id);
            if (nameExists)
            {
                return BadRequest($"Another cancel reason with the name '{updateDto.Name}' already exists.");
            }

            existingCancelReason.Name = updateDto.Name;
            existingCancelReason.IsActive = updateDto.IsActive;
            existingCancelReason.UpdatedDate = DateTime.Now;

            await _cancelReasonRepository.UpdateAsync(existingCancelReason);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCancelReason(int id)
        {
            var cancelReason = await _cancelReasonRepository.GetByIdAsync(id);
            if (cancelReason == null)
            {
                return NotFound();
            }

            // Check if there are any cancelled tickets using this cancel reason
            var hasReferences = await _context.CancelledTickets.AnyAsync(ct => ct.CancelReasonId == id);
            if (hasReferences)
            {
                // Instead of deleting, mark as inactive
                cancelReason.IsActive = false;
                cancelReason.UpdatedDate = DateTime.Now;
                await _cancelReasonRepository.UpdateAsync(cancelReason);
                return Ok(new { message = "Cancel reason is in use and cannot be deleted. It has been marked as inactive instead." });
            }

            await _cancelReasonRepository.DeleteAsync(id);
            return NoContent();
        }

        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<CancelReasonDto>>> GetActiveCancelReasons()
        {
            var cancelReasons = await _context.CancelReasons.Where(c => c.IsActive).ToListAsync();
            return Ok(_mapper.Map<IEnumerable<CancelReasonDto>>(cancelReasons));
        }
    }
}