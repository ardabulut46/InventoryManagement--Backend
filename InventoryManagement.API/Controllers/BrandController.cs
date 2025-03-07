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
    public class BrandController : ControllerBase
    {
        private readonly IGenericRepository<Brand> _brandRepository;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;

        public BrandController(IGenericRepository<Brand> brandRepository, IMapper mapper, ApplicationDbContext context)
        {
            _brandRepository = brandRepository;
            _mapper = mapper;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BrandDto>>> GetBrands()
        {
            var brands = await _brandRepository.GetAllAsync();
            return Ok(_mapper.Map<IEnumerable<BrandDto>>(brands));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BrandDto>> GetBrand(int id)
        {
            var brand = await _brandRepository.GetByIdAsync(id);
            if (brand == null)
            {
                return NotFound();
            }
            return Ok(_mapper.Map<BrandDto>(brand));
        }

        [HttpPost]
       // [Authorize(Policy = "CanCreate")]
        public async Task<ActionResult<BrandDto>> CreateBrand(BrandDto brandDto)
        {
            // Check if a brand with the same name already exists
            var existingBrand = await _context.Brands.FirstOrDefaultAsync(b => b.Name == brandDto.Name);
            if (existingBrand != null)
            {
                return BadRequest($"A brand with the name '{brandDto.Name}' already exists.");
            }

            var brand = new Brand
            {
                Name = brandDto.Name,
                IsActive = true
            };

            await _brandRepository.AddAsync(brand);
            return CreatedAtAction(nameof(GetBrand), new { id = brand.Id }, _mapper.Map<BrandDto>(brand));
        }

        [HttpPut("{id}")]
        //[Authorize(Policy = "CanEdit")]
        public async Task<IActionResult> UpdateBrand(int id, BrandDto brandDto)
        {
            var brand = await _brandRepository.GetByIdAsync(id);
            if (brand == null)
            {
                return NotFound();
            }

            // Check if another brand with the same name already exists
            var existingBrand = await _context.Brands.FirstOrDefaultAsync(b => b.Name == brandDto.Name && b.Id != id);
            if (existingBrand != null)
            {
                return BadRequest($"Another brand with the name '{brandDto.Name}' already exists.");
            }

            brand.Name = brandDto.Name;
            brand.IsActive = brandDto.IsActive;

            await _brandRepository.UpdateAsync(brand);
            return NoContent();
        }

        [HttpDelete("{id}")]
       // [Authorize(Policy = "CanDelete")]
        public async Task<IActionResult> DeleteBrand(int id)
        {
            var brand = await _brandRepository.GetByIdAsync(id);
            if (brand == null)
            {
                return NotFound();
            }

            // Check if there are any inventories or models using this brand
            var hasInventories = await _context.Inventories.AnyAsync(i => i.BrandId == id);
            var hasModels = await _context.Models.AnyAsync(m => m.BrandId == id);
            
            if (hasInventories || hasModels)
            {
                // Instead of deleting, mark as inactive
                brand.IsActive = false;
                await _brandRepository.UpdateAsync(brand);
                return Ok(new { message = "Brand has associated inventories or models and cannot be deleted. It has been marked as inactive instead." });
            }

            await _brandRepository.DeleteAsync(id);
            return NoContent();
        }

        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<BrandDto>>> GetActiveBrands()
        {
            var brands = await _context.Brands.Where(b => b.IsActive).ToListAsync();
            return Ok(_mapper.Map<IEnumerable<BrandDto>>(brands));
        }

        [HttpGet("{id}/models")]
        public async Task<ActionResult<IEnumerable<object>>> GetBrandModels(int id)
        {
            var brand = await _brandRepository.GetByIdAsync(id);
            if (brand == null)
            {
                return NotFound();
            }

            var models = await _context.Models
                .Where(m => m.BrandId == id && m.IsActive)
                .Select(m => new { m.Id, m.Name })
                .ToListAsync();

            return Ok(models);
        }
    }
}