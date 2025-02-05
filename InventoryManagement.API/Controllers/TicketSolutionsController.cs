using AutoMapper;
using InventoryManagement.Core.DTOs.TicketSolution;
using InventoryManagement.Core.Entities;
using InventoryManagement.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using InventoryManagement.Core.DTOs;
using InventoryManagement.Core.DTOs.User;
using InventoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketSolutionsController : ControllerBase
    {
        private readonly IGenericRepository<TicketSolution> _ticketSolutionRepository;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;

        public TicketSolutionsController(
            IGenericRepository<TicketSolution> ticketSolutionRepository,
            IMapper mapper,
            ApplicationDbContext context)
        {
            _ticketSolutionRepository = ticketSolutionRepository;
            _mapper = mapper;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TicketSolutionDto>>> GetTicketSolutions()
        {
            var solutions = await _ticketSolutionRepository.GetAllWithIncludesAsync(
                "Ticket",
                "User",
                "AssignedUser",
                "SolutionType");
            
            return Ok(_mapper.Map<IEnumerable<TicketSolutionDto>>(solutions));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TicketSolutionDto>> GetTicketSolution(int id)
        {
            var solution = await _ticketSolutionRepository.GetByIdWithIncludesAsync(
                id,
                "Ticket",
                "User",
                "AssignedUser",
                "SolutionType");

            if (solution == null)
                return NotFound();

            return Ok(_mapper.Map<TicketSolutionDto>(solution));
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<TicketSolutionDto>> CreateTicketSolution(TicketSolutionCreateUpdateDto createUpdateDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int currentUserId))
            {
                return BadRequest("Invalid user ID");
            }
    
            // Check if the ticket exists and is assigned to the current user
            var ticket = await _context.Tickets
                .FirstOrDefaultAsync(x => x.Id == createUpdateDto.TicketId);

            if (ticket == null)
            {
                return NotFound("Ticket not found");
            }

            // Check if the current user is assigned to this ticket
            if (ticket.UserId != currentUserId)
            {
                return BadRequest("You can only create solutions for tickets assigned to you");
            }

            var ticketSolution = await _context.TicketSolutions.FirstOrDefaultAsync(x => x.TicketId == createUpdateDto.TicketId);
            if (ticketSolution != null)
            {
                return BadRequest("Çağrı zaten çözümlendi.");
            }

            // Check if ticket is already solved
            /*
            if (ticket.Status == TicketStatus.Solved)
            {
                return BadRequest("This ticket is already solved");
            }*/

            var solution = _mapper.Map<TicketSolution>(createUpdateDto);
            solution.UserId = currentUserId;
            solution.AssignedUserId = currentUserId; // The solver is the current assigned user
            solution.SolutionDate = DateTime.Now;

            var createdSolution = await _ticketSolutionRepository.AddAsync(solution);

            // Update ticket status to Solved
           // ticket.Status = TicketStatus.Solved;
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetTicketSolution),
                new { id = createdSolution.Id },
                _mapper.Map<TicketSolutionDto>(createdSolution));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTicketSolution(int id, TicketSolutionCreateUpdateDto createUpdateDto)
        {
            var solution = await _ticketSolutionRepository.GetByIdAsync(id);
            if (solution == null)
                return NotFound();

            _mapper.Map(createUpdateDto, solution);
            await _ticketSolutionRepository.UpdateAsync(solution);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTicketSolution(int id)
        {
            var solution = await _ticketSolutionRepository.GetByIdAsync(id);
            if (solution == null)
                return NotFound();

            await _ticketSolutionRepository.DeleteAsync(id);
            return NoContent();
        }

        [HttpGet("by-ticket/{ticketId}")]
        public async Task<ActionResult<IEnumerable<TicketSolutionDto>>> GetSolutionsByTicket(int ticketId)
        {
            var solutions = await _ticketSolutionRepository.SearchWithIncludesAsync(
                s => s.TicketId == ticketId,
                "Ticket",
                "User",
                "AssignedUser",
                "SolutionType");

            return Ok(_mapper.Map<IEnumerable<TicketSolutionDto>>(solutions));
        }

        [HttpGet("by-assigned-user")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<TicketSolutionDto>>> GetSolutionsByAssignedUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int currentUserId))
            {
                return BadRequest("Invalid user ID");
            }

            var solutions = await _ticketSolutionRepository.SearchWithIncludesAsync(
                s => s.AssignedUserId == currentUserId,
                "Ticket",
                "User",
                "AssignedUser",
                "SolutionType");

            return Ok(_mapper.Map<IEnumerable<TicketSolutionDto>>(solutions));
        }
    }
}