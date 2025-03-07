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
    public class ModelController : ControllerBase
    {
        private readonly IGenericRepository<Model> _modelRepository;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;

        public ModelController(IGenericRepository<Model> modelRepository, IMapper mapper, ApplicationDbContext context)
        {
            _modelRepository = modelRepository;
            _mapper = mapper;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ModelDto>>> GetModels()
        {
            var models = await _modelRepository.GetAllWithIncludesAsync("Brand");
            return Ok(_mapper.Map<IEnumerable<ModelDto>>(models));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ModelDto>> GetModel(int id)
        {
            var model = await _modelRepository.GetByIdWithIncludesAsync(id, "Brand");
            if (model == null)
            {
                return NotFound();
            }
            return Ok(_mapper.Map<ModelDto>(model));
        }

        [HttpPost]
        //[Authorize(Policy = "CanCreate")]
        public async Task<ActionResult<ModelDto>> CreateModel(ModelDto modelDto)
        {
            // Check if the brand exists
            var brandExists = await _context.Brands.AnyAsync(b => b.Id == modelDto.BrandId);
            if (!brandExists)
            {
                return BadRequest($"Brand with ID {modelDto.BrandId} does not exist.");
            }

            // Check if a model with the same name already exists for this brand
            var existingModel = await _context.Models.FirstOrDefaultAsync(m => 
                m.Name == modelDto.Name && m.BrandId == modelDto.BrandId);
                
            if (existingModel != null)
            {
                return BadRequest($"A model with the name '{modelDto.Name}' already exists for this brand.");
            }

            var model = new Model
            {
                Name = modelDto.Name,
                BrandId = modelDto.BrandId,
                IsActive = true
            };

            await _modelRepository.AddAsync(model);
            
            // Fetch the complete model with brand for the response
            var createdModel = await _modelRepository.GetByIdWithIncludesAsync(model.Id, "Brand");
            return CreatedAtAction(nameof(GetModel), new { id = model.Id }, _mapper.Map<ModelDto>(createdModel));
        }

        [HttpPut("{id}")]
       // [Authorize(Policy = "CanEdit")]
        public async Task<IActionResult> UpdateModel(int id, ModelDto modelDto)
        {
            var model = await _modelRepository.GetByIdAsync(id);
            if (model == null)
            {
                return NotFound();
            }

            // Check if the brand exists
            var brandExists = await _context.Brands.AnyAsync(b => b.Id == modelDto.BrandId);
            if (!brandExists)
            {
                return BadRequest($"Brand with ID {modelDto.BrandId} does not exist.");
            }

            // Check if another model with the same name already exists for this brand
            var existingModel = await _context.Models.FirstOrDefaultAsync(m => 
                m.Name == modelDto.Name && m.BrandId == modelDto.BrandId && m.Id != id);
                
            if (existingModel != null)
            {
                return BadRequest($"Another model with the name '{modelDto.Name}' already exists for this brand.");
            }

            model.Name = modelDto.Name;
            model.BrandId = modelDto.BrandId;
            model.IsActive = modelDto.IsActive;

            await _modelRepository.UpdateAsync(model);
            return NoContent();
        }

        [HttpDelete("{id}")]
        //[Authorize(Policy = "CanDelete")]
        public async Task<IActionResult> DeleteModel(int id)
        {
            var model = await _modelRepository.GetByIdAsync(id);
            if (model == null)
            {
                return NotFound();
            }

            // Check if there are any inventories using this model
            var hasInventories = await _context.Inventories.AnyAsync(i => i.ModelId == id);
            if (hasInventories)
            {
                // Instead of deleting, mark as inactive
                model.IsActive = false;
                await _modelRepository.UpdateAsync(model);
                return Ok(new { message = "Model has associated inventories and cannot be deleted. It has been marked as inactive instead." });
            }

            await _modelRepository.DeleteAsync(id);
            return NoContent();
        }

        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<ModelDto>>> GetActiveModels()
        {
            var models = await _context.Models
                .Where(m => m.IsActive)
                .Include(m => m.Brand)
                .ToListAsync();
                
            return Ok(_mapper.Map<IEnumerable<ModelDto>>(models));
        }

        [HttpGet("by-brand/{brandId}")]
        public async Task<ActionResult<IEnumerable<ModelDto>>> GetModelsByBrand(int brandId)
        {
            // Check if the brand exists
            var brandExists = await _context.Brands.AnyAsync(b => b.Id == brandId);
            if (!brandExists)
            {
                return NotFound($"Brand with ID {brandId} does not exist.");
            }

            var models = await _context.Models
                .Where(m => m.BrandId == brandId && m.IsActive)
                .Include(m => m.Brand)
                .ToListAsync();
                
            return Ok(_mapper.Map<IEnumerable<ModelDto>>(models));
        }
    }
}