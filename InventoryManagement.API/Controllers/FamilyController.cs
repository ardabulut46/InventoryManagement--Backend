using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using InventoryManagement.Core.DTOs.Family;
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
    public class FamilyController : ControllerBase
    {
        private readonly IGenericRepository<Family> _familyRepository;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;

        public FamilyController(IGenericRepository<Family> familyRepository, IMapper mapper, ApplicationDbContext context)
        {
            _familyRepository = familyRepository;
            _mapper = mapper;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FamilyDto>>> GetFamilies()
        {
            var families = await _familyRepository.GetAllAsync();
            return Ok(_mapper.Map<IEnumerable<FamilyDto>>(families));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FamilyDto>> GetFamily(int id)
        {
            var family = await _familyRepository.GetByIdAsync(id);
            if (family == null)
            {
                return NotFound();
            }
            return Ok(_mapper.Map<FamilyDto>(family));
        }

        [HttpPost]
        //[Authorize(Policy = "CanCreate")]
        public async Task<ActionResult<FamilyDto>> CreateFamily(FamilyDto familyDto)
        {
            // Check if a family with the same name already exists
            var existingFamily = await _context.Families.FirstOrDefaultAsync(f => f.Name == familyDto.Name);
            if (existingFamily != null)
            {
                return BadRequest($"A family with the name '{familyDto.Name}' already exists.");
            }

            var family = new Family
            {
                Name = familyDto.Name,
                IsActive = true
            };

            await _familyRepository.AddAsync(family);
            return CreatedAtAction(nameof(GetFamily), new { id = family.Id }, _mapper.Map<FamilyDto>(family));
        }

        [HttpPut("{id}")]
       // [Authorize(Policy = "CanEdit")]
        public async Task<IActionResult> UpdateFamily(int id, FamilyDto familyDto)
        {
            var family = await _familyRepository.GetByIdAsync(id);
            if (family == null)
            {
                return NotFound();
            }

            // Check if another family with the same name already exists
            var existingFamily = await _context.Families.FirstOrDefaultAsync(f => f.Name == familyDto.Name && f.Id != id);
            if (existingFamily != null)
            {
                return BadRequest($"Another family with the name '{familyDto.Name}' already exists.");
            }

            family.Name = familyDto.Name;
            family.IsActive = familyDto.IsActive;

            await _familyRepository.UpdateAsync(family);
            return NoContent();
        }

        [HttpDelete("{id}")]
     //   [Authorize(Policy = "CanDelete")]
        public async Task<IActionResult> DeleteFamily(int id)
        {
            var family = await _familyRepository.GetByIdAsync(id);
            if (family == null)
            {
                return NotFound();
            }

            // Check if there are any inventories using this family
            var hasInventories = await _context.Inventories.AnyAsync(i => i.FamilyId == id);
            if (hasInventories)
            {
                // Instead of deleting, mark as inactive
                family.IsActive = false;
                await _familyRepository.UpdateAsync(family);
                return Ok(new { message = "Family has associated inventories and cannot be deleted. It has been marked as inactive instead." });
            }

            await _familyRepository.DeleteAsync(id);
            return NoContent();
        }

        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<FamilyDto>>> GetActiveFamilies()
        {
            var families = await _context.Families.Where(f => f.IsActive).ToListAsync();
            return Ok(_mapper.Map<IEnumerable<FamilyDto>>(families));
        }
    }
}