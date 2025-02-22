using AutoMapper;
using InventoryManagement.Core.DTOs.TicketHistory;
using InventoryManagement.Core.Entities;
using InventoryManagement.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InventoryManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketHistoryController : ControllerBase
    {
        private readonly IGenericRepository<TicketHistory> _repository;
        private readonly IMapper _mapper;

        public TicketHistoryController(
            IGenericRepository<TicketHistory> repository,
            IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TicketHistoryDto>>> GetAll()
        {
            var histories = await _repository.GetAllWithIncludesAsync(
                "Ticket",
                "User",
                "FromAssignedUser",
                "ToUser",
                "Group");
            
            return Ok(_mapper.Map<IEnumerable<TicketHistoryDto>>(histories));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TicketHistoryDto>> GetById(int id)
        {
            var history = await _repository.GetByIdWithIncludesAsync(
                id,
                "Ticket",
                "User",
                "FromAssignedUser",
                "ToUser",
                "Group");

            if (history == null)
                return NotFound();

            return Ok(_mapper.Map<TicketHistoryDto>(history));
        }

        [HttpGet("by-ticket/{ticketId}")]
        public async Task<ActionResult<IEnumerable<TicketHistoryDto>>> GetByTicketId(int ticketId)
        {
            var histories = await _repository.SearchWithIncludesAsync(
                x => x.TicketId == ticketId,
                "Ticket",
                "User",
                "FromAssignedUser",
                "ToUser",
                "Group");

            return Ok(_mapper.Map<IEnumerable<TicketHistoryDto>>(histories));
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<TicketHistoryDto>> Create(TicketHistoryCreateUpdateDto createDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int currentUserId))
            {
                return BadRequest("Invalid user ID");
            }

            var entity = _mapper.Map<TicketHistory>(createDto);
            
            var created = await _repository.AddAsync(entity);

            return CreatedAtAction(
                nameof(GetById),
                new { id = created.Id },
                _mapper.Map<TicketHistoryDto>(created));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, TicketHistoryCreateUpdateDto updateDto)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                return NotFound();

            _mapper.Map(updateDto, entity);
            await _repository.UpdateAsync(entity);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                return NotFound();

            await _repository.DeleteAsync(id);
            return NoContent();
        }
    }
}