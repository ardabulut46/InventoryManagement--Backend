using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using InventoryManagement.Core.DTOs.Inventory;
using InventoryManagement.Core.Entities;
using InventoryManagement.Core.Interfaces;
using InventoryManagement.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryTypeController : ControllerBase
    {
        private readonly IGenericRepository<InventoryType> _typeRepository;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;

        public InventoryTypeController(IGenericRepository<InventoryType> typeRepository, IMapper mapper, ApplicationDbContext context)
        {
            _typeRepository = typeRepository;
            _mapper = mapper;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<InventoryTypeDto>>> GetTypes()
        {
            var types = await _typeRepository.GetAllAsync();
            return Ok(_mapper.Map<IEnumerable<InventoryTypeDto>>(types));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<InventoryTypeDto>> GetType(int id)
        {
            var type = await _typeRepository.GetByIdAsync(id);
            if (type == null)
            {
                return NotFound();
            }
            return Ok(_mapper.Map<InventoryTypeDto>(type));
        }

        [HttpPost]
        //[Authorize(Policy = "CanCreate")]
        public async Task<ActionResult<InventoryTypeDto>> CreateType(InventoryTypeDto typeDto)
        {
            // Check if a type with the same name already exists
            var existingType = await _context.InventoryTypes.FirstOrDefaultAsync(t => t.Name == typeDto.Name);
            if (existingType != null)
            {
                return BadRequest($"A type with the name '{typeDto.Name}' already exists.");
            }

            var type = new InventoryType
            {
                Name = typeDto.Name,
                IsActive = true
            };

            await _typeRepository.AddAsync(type);
            return CreatedAtAction(nameof(GetType), new { id = type.Id }, _mapper.Map<InventoryTypeDto>(type));
        }

        [HttpPut("{id}")]
       // [Authorize(Policy = "CanEdit")]
        public async Task<IActionResult> UpdateType(int id, InventoryTypeDto typeDto)
        {
            var type = await _typeRepository.GetByIdAsync(id);
            if (type == null)
            {
                return NotFound();
            }

            // Check if another type with the same name already exists
            var existingType = await _context.InventoryTypes.FirstOrDefaultAsync(t => t.Name == typeDto.Name && t.Id != id);
            if (existingType != null)
            {
                return BadRequest($"Another type with the name '{typeDto.Name}' already exists.");
            }

            type.Name = typeDto.Name;
            type.IsActive = typeDto.IsActive;

            await _typeRepository.UpdateAsync(type);
            return NoContent();
        }

        [HttpDelete("{id}")]
        //[Authorize(Policy = "CanDelete")]
        public async Task<IActionResult> DeleteType(int id)
        {
            var type = await _typeRepository.GetByIdAsync(id);
            if (type == null)
            {
                return NotFound();
            }

            // Check if there are any inventories using this type
            var hasInventories = await _context.Inventories.AnyAsync(i => i.TypeId == id);
            if (hasInventories)
            {
                // Instead of deleting, mark as inactive
                type.IsActive = false;
                await _typeRepository.UpdateAsync(type);
                return Ok(new { message = "Type has associated inventories and cannot be deleted. It has been marked as inactive instead." });
            }

            await _typeRepository.DeleteAsync(id);
            return NoContent();
        }

        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<InventoryTypeDto>>> GetActiveTypes()
        {
            var types = await _context.InventoryTypes.Where(t => t.IsActive).ToListAsync();
            return Ok(_mapper.Map<IEnumerable<InventoryTypeDto>>(types));
        }
    }
}