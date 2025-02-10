using AutoMapper;
using InventoryManagement.Core.DTOs.TicketSolution;
using InventoryManagement.Core.Entities;
using InventoryManagement.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using InventoryManagement.Core.DTOs;
using InventoryManagement.Core.DTOs.SolutionReview;
using InventoryManagement.Core.DTOs.User;
using InventoryManagement.Core.Helpers;
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
        private readonly IGenericRepository<SolutionReview> _solutionReviewRepository;

        public TicketSolutionsController(
            IGenericRepository<TicketSolution> ticketSolutionRepository,
            IMapper mapper,
            ApplicationDbContext context,
            IGenericRepository<SolutionReview> solutionReviewRepository)
        {
            _ticketSolutionRepository = ticketSolutionRepository;
            _mapper = mapper;
            _context = context;
            _solutionReviewRepository = solutionReviewRepository;
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
        public async Task<ActionResult<TicketSolutionDto>> CreateTicketSolution(
            TicketSolutionCreateUpdateDto createUpdateDto)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int currentUserId))
            {
                return BadRequest("Invalid user ID");
            }

            var ticket = await _context.Tickets
                .Include(t => t.Group)
                .FirstOrDefaultAsync(x => x.Id == createUpdateDto.TicketId);
            if (ticket == null)
            {
                return NotFound("Ticket not found");
            }

            if (ticket.UserId != currentUserId)
            {
                return BadRequest("You can only create solutions for tickets assigned to you");
            }

            var existingSolutions =
                await _ticketSolutionRepository.SearchAsync(s => s.TicketId == createUpdateDto.TicketId);
            if (existingSolutions.Any())
            {
                return BadRequest("Çağrı zaten çözümlendi.");
            }

            // Validate problem type and solution time before creating the solution
            var problemType = await _context.ProblemTypes
                .FirstOrDefaultAsync(pt => pt.Name == ticket.ProblemType && pt.GroupId == ticket.GroupId);
            if (problemType == null)
            {
                return BadRequest("Problem type not found");
            }

            var solutionTime = await _context.SolutionTimes
                .FirstOrDefaultAsync(st => st.ProblemTypeId == problemType.Id);
            if (solutionTime == null)
            {
                return BadRequest("Solution time not configured for the problem type");
            }

            if (!ticket.AssignedDate.HasValue)
            {
                return BadRequest("Ticket does not have an assigned date");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var solution = _mapper.Map<TicketSolution>(createUpdateDto);
                solution.UserId = currentUserId;
                solution.AssignedUserId = currentUserId;
                solution.SolutionDate = DateTime.Now;

                _context.TicketSolutions.Add(solution);
                await _context.SaveChangesAsync();

                var expectedSolutionTime = ticket.AssignedDate.Value.Add(solutionTime.TimeToSolve);
                bool isInTime = solution.SolutionDate <= expectedSolutionTime;
                TimeSpan? howMuchLate = isInTime ? null : solution.SolutionDate - expectedSolutionTime;

                string notes = isInTime
                    ? "Zamanında çözümlendi."
                    : $"Geç kalma süresi: {TimeSpanFormatter.Format(howMuchLate.Value)}";

                var solutionReview = new SolutionReview
                {
                    TicketSolutionId = solution.Id,
                    IsInTime = isInTime,
                    Notes = notes,
                    HowMuchLate = howMuchLate
                };

                _context.SolutionReviews.Add(solutionReview);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return CreatedAtAction(
                    nameof(GetTicketSolution),
                    new { id = solution.Id },
                    _mapper.Map<TicketSolutionDto>(solution));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
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