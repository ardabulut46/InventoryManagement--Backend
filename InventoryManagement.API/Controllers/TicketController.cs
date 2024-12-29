using AutoMapper;
using InventoryManagement.Core.DTOs.Ticket;
using InventoryManagement.Core.Entities;
using InventoryManagement.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketController : ControllerBase
    {
        private readonly IGenericRepository<Ticket> _ticketRepository;
        private readonly IMapper _mapper;

        public TicketController(IGenericRepository<Ticket> ticketRepository, IMapper mapper)
        {
            _ticketRepository = ticketRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TicketDto>>> GetTickets()
        {
            var tickets = await _ticketRepository.GetAllWithIncludesAsync(
                "User",
                "Inventory");
            return Ok(_mapper.Map<IEnumerable<TicketDto>>(tickets));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TicketDto>> GetTicket(int id)
        {
            var ticket = await _ticketRepository.GetByIdWithIncludesAsync(
                id,
                "User",
                "Inventory");
        
            if (ticket == null)
                return NotFound();

            return Ok(_mapper.Map<TicketDto>(ticket));
        }

        [HttpPost]
        public async Task<ActionResult<TicketDto>> CreateTicket(CreateTicketDto createTicketDto)
        {
            var ticket = _mapper.Map<Ticket>(createTicketDto);
            var createdTicket = await _ticketRepository.AddAsync(ticket);
            return CreatedAtAction(nameof(GetTicket), 
                new { id = createdTicket.Id }, 
                _mapper.Map<TicketDto>(createdTicket));
        }  

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTicket(int id, UpdateTicketDto updateTicketDto)
        {
            var ticket = await _ticketRepository.GetByIdAsync(id);
            if (ticket == null)
                return NotFound();

            _mapper.Map(updateTicketDto, ticket);
            await _ticketRepository.UpdateAsync(ticket);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTicket(int id)
        {
            var ticket = await _ticketRepository.GetByIdAsync(id);
            if (ticket == null)
                return NotFound();

            await _ticketRepository.DeleteAsync(id);
            return NoContent();
        }
    }
}
