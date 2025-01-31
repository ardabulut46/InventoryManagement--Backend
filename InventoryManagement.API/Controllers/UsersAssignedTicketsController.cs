using AutoMapper;
using InventoryManagement.Core.DTOs.UsersAssignedTickets;
using InventoryManagement.Core.Entities;
using InventoryManagement.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersAssignedTicketsController : ControllerBase
    {
        private readonly IGenericRepository<UsersAssignedTickets> _repository;
        private readonly IMapper _mapper;

        public UsersAssignedTicketsController(
            IGenericRepository<UsersAssignedTickets> repository,
            IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UsersAssignedTicketsDto>>> GetAll()
        {
            var records = await _repository.GetAllWithIncludesAsync("User");
            return Ok(_mapper.Map<IEnumerable<UsersAssignedTicketsDto>>(records));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UsersAssignedTicketsDto>> GetById(int id)
        {
            var record = await _repository.GetByIdWithIncludesAsync(id, "User");
            if (record == null)
                return NotFound();

            return Ok(_mapper.Map<UsersAssignedTicketsDto>(record));
        }

        [HttpGet("by-user/{userId}")]
        public async Task<ActionResult<UsersAssignedTicketsDto>> GetByUserId(int userId)
        {
            var record = await _repository.SearchWithIncludesAsync(
                x => x.UserId == userId,
                "User");

            return Ok(_mapper.Map<UsersAssignedTicketsDto>(record));
        }

        [HttpPost]
        public async Task<ActionResult<UsersAssignedTicketsDto>> Create(UsersAssignedTicketsCreateUpdateDto createDto)
        {
            var entity = _mapper.Map<UsersAssignedTickets>(createDto);
            var created = await _repository.AddAsync(entity);

            return CreatedAtAction(
                nameof(GetById),
                new { id = created.Id },
                _mapper.Map<UsersAssignedTicketsDto>(created));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UsersAssignedTicketsCreateUpdateDto updateDto)
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