using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InventoryManagement.Core.Entities;
using InventoryManagement.Core.Interfaces;
using InventoryManagement.Core.DTOs.IdleDurationLimit;
using InventoryManagement.Core.DTOs.Ticket;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class IdleDurationLimitController : ControllerBase
    {
        private readonly IGenericRepository<IdleDurationLimit> _idleDurationLimitRepository;
        private readonly IGenericRepository<Ticket> _ticketRepository;
        private readonly IMapper _mapper;

        public IdleDurationLimitController(
            IGenericRepository<IdleDurationLimit> idleDurationLimitRepository,
            IGenericRepository<Ticket> ticketRepository,
            IMapper mapper)
        {
            _idleDurationLimitRepository = idleDurationLimitRepository;
            _ticketRepository = ticketRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<IdleDurationLimitDto>>> GetIdleDurationLimits()
        {
            var limits = await _idleDurationLimitRepository.GetAllAsync(
                include: query => query.Include(x => x.ProblemType));
            return Ok(_mapper.Map<IEnumerable<IdleDurationLimitDto>>(limits));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IdleDurationLimitDto>> GetIdleDurationLimit(int id)
        {
            var limit = await _idleDurationLimitRepository.GetByIdWithIncludesAsync(id, "ProblemType");
            if (limit == null)
                return NotFound();
            return Ok(_mapper.Map<IdleDurationLimitDto>(limit));
        }

        [HttpGet("by-problem-type/{problemTypeId}")]
        public async Task<ActionResult<IdleDurationLimitDto>> GetByProblemType(int problemTypeId)
        {
            var limits = await _idleDurationLimitRepository.SearchWithIncludesAsync(
                l => l.ProblemTypeId == problemTypeId, "ProblemType");
            var limit = limits.FirstOrDefault();
            if (limit == null)
                return NotFound();
            return Ok(_mapper.Map<IdleDurationLimitDto>(limit));
        }

        [HttpPost]
        public async Task<ActionResult<IdleDurationLimitDto>> CreateIdleDurationLimit(CreateUpdateIdleDurationLimitDto createDto)
        {
            // Optionally ensure a limit does not exist yet for the problem type.
            var existing = await _idleDurationLimitRepository.SearchAsync(l => l.ProblemTypeId == createDto.ProblemTypeId);
            if (existing.Any())
                return BadRequest("An idle duration limit for this problem type already exists.");

            var idleLimit = _mapper.Map<IdleDurationLimit>(createDto);
            var createdLimit = await _idleDurationLimitRepository.AddAsync(idleLimit);

            // Fetch full data with ProblemType included.
            var fullCreatedLimit = await _idleDurationLimitRepository.GetByIdWithIncludesAsync(createdLimit.Id, "ProblemType");

            return CreatedAtAction(nameof(GetIdleDurationLimit), new { id = createdLimit.Id }, _mapper.Map<IdleDurationLimitDto>(fullCreatedLimit));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateIdleDurationLimit(int id, CreateUpdateIdleDurationLimitDto updateDto)
        {
            var idleLimit = await _idleDurationLimitRepository.GetByIdAsync(id);
            if (idleLimit == null)
                return NotFound();

            // Optionally validate if another limit already exists for the same problem type.
            var existing = await _idleDurationLimitRepository.SearchAsync(l => l.ProblemTypeId == updateDto.ProblemTypeId && l.Id != id);
            if (existing.Any())
                return BadRequest("An idle duration limit for this problem type already exists.");

            _mapper.Map(updateDto, idleLimit);
            await _idleDurationLimitRepository.UpdateAsync(idleLimit);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteIdleDurationLimit(int id)
        {
            var idleLimit = await _idleDurationLimitRepository.GetByIdAsync(id);
            if (idleLimit == null)
                return NotFound();

            await _idleDurationLimitRepository.DeleteAsync(id);
            return NoContent();
        }

        // Extra endpoint: return tickets whose idle duration has breached the allowed limit.
        [HttpGet("idle-breach-tickets")]
        public async Task<ActionResult<IEnumerable<TicketDto>>> GetIdleBreachTickets()
        {
            // Get all idle duration limits and organize them by ProblemTypeId.
            var idleLimits = await _idleDurationLimitRepository.GetAllAsync();
            var idleLimitDict = idleLimits.ToDictionary(l => l.ProblemTypeId, l => l.TimeToAssign);

            // Get tickets including their ProblemType.
            var tickets = await _ticketRepository.GetAllWithIncludesAsync("ProblemType");

            var breachedTickets = tickets.Where(ticket =>
            {
                if (ticket.ProblemTypeId == null)
                    return false;
                if (!idleLimitDict.ContainsKey(ticket.ProblemTypeId.Value))
                    return false;

                var allowedIdleTime = idleLimitDict[ticket.ProblemTypeId.Value];
                TimeSpan currentIdle;
                if (ticket.AssignedDate.HasValue)
                {
                    // For assigned tickets, we assume IdleDuration was calculated at assignment.
                    currentIdle = ticket.AssignedDate.Value - ticket.CreatedDate;
                }
                else
                {
                    // For unassigned tickets, calculate the waiting duration.
                    currentIdle = DateTime.Now - ticket.CreatedDate;
                }
                return currentIdle > allowedIdleTime;
            }).ToList();

            return Ok(_mapper.Map<IEnumerable<TicketDto>>(breachedTickets));
        }
    }
}