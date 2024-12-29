using AutoMapper;
using FluentValidation;
using InventoryManagement.Core.DTOs.Inventory;
using InventoryManagement.Core.Entities;
using InventoryManagement.Core.Interfaces;
using InventoryManagement.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;

namespace InventoryManagement.API.Controllers
{
    [ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly IGenericRepository<Inventory> _inventoryRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateInventoryDto> _validator;
    
    public InventoryController(IGenericRepository<Inventory> inventoryRepository, IMapper mapper, IValidator<CreateInventoryDto> validator)
    {
        _inventoryRepository = inventoryRepository;
        _mapper = mapper;
        _validator = validator;
    }

    [HttpGet]
    [Authorize(Policy = "CanView")]
    public async Task<ActionResult<IEnumerable<InventoryDto>>> GetInventories()
    {
        var inventories = await _inventoryRepository.GetAllWithIncludesAsync(
            "AssignedUser",
            "SupportCompany",
            "InventoryHistory");
        return Ok(_mapper.Map<IEnumerable<InventoryDto>>(inventories));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<InventoryDto>> GetInventory(int id)
    {
        var inventory = await _inventoryRepository.GetByIdWithIncludesAsync(
            id,
            "AssignedUser",
            "SupportCompany",
            "InventoryHistory");
        
        if (inventory == null)
            return NotFound();

        return Ok(_mapper.Map<InventoryDto>(inventory));
    }

    [HttpPost]
    public async Task<ActionResult<InventoryDto>> CreateInventory(CreateInventoryDto createInventoryDto)
    {
        
        var validationResult = await _validator.ValidateAsync(createInventoryDto);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        var inventory = _mapper.Map<Inventory>(createInventoryDto);
        var createdInventory = await _inventoryRepository.AddAsync(inventory);
        
        if (createInventoryDto.AssignedUserId.HasValue)
        {
            await _inventoryRepository.AddInventoryHistoryAsync(
                createdInventory.Id,
                createInventoryDto.AssignedUserId.Value,
                "İlk atama");
        }

        return CreatedAtAction(
            nameof(GetInventory), 
            new { id = createdInventory.Id }, 
            _mapper.Map<InventoryDto>(createdInventory));
    }
    
    [HttpPut("{id}/assign-user")]
    public async Task<IActionResult> AssignUser(int id, int userId, string notes = null)
    {
        var inventory = await _inventoryRepository.GetByIdAsync(id);
        if (inventory == null)
            return NotFound();

        inventory.AssignedUserId = userId;
        await _inventoryRepository.UpdateAsync(inventory);
        
        await _inventoryRepository.AddInventoryHistoryAsync(id, userId, notes);
    
        return NoContent();
    }

    // Eğer bir ürün,envanter el değiştirdiyse buradan güncellenecek, ilk atamalar inventory'i post ederken verilebilir  
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateInventory(int id, UpdateInventoryDto updateInventoryDto)
    {
        var inventory = await _inventoryRepository.GetByIdAsync(id);
        if (inventory == null)
            return NotFound();
        
        // Eğer assigned user değişmişse ve yeni bir atama yapılmışsa
        if (inventory.AssignedUserId != updateInventoryDto.AssignedUserId && 
            updateInventoryDto.AssignedUserId.HasValue)
        {
            await _inventoryRepository.AddInventoryHistoryAsync(
                id,
                updateInventoryDto.AssignedUserId.Value,
                "User reassignment"); //  "Kullanıcı değişimi" gibi bir not
        }

        _mapper.Map(updateInventoryDto, inventory);
        await _inventoryRepository.UpdateAsync(inventory);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteInventory(int id)
    {
        var inventory = await _inventoryRepository.GetByIdAsync(id);
        if (inventory == null)
            return NotFound();

        await _inventoryRepository.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<InventoryDto>>> SearchInventories(
        [FromQuery] string? searchTerm,
        [FromQuery] string? status,
        [FromQuery] bool? hasUser)
    {
        var query = await _inventoryRepository.SearchWithIncludesAsync(
            inventory => 
                (string.IsNullOrEmpty(searchTerm) || inventory.Model.Contains(searchTerm)) &&
                (string.IsNullOrEmpty(status) || inventory.Status == status) &&
                (!hasUser.HasValue || (hasUser.Value ? inventory.AssignedUserId != null : inventory.AssignedUserId == null)),
            "AssignedUser", "SupportCompany", "InventoryHistory");

        
        var result = _mapper.Map<IEnumerable<InventoryDto>>(query);
        return Ok(result);
    }

    [HttpGet("warranty-expiring")]
    public async Task<ActionResult<IEnumerable<InventoryDto>>> GetWarrantyExpiringInventories(int days = 30)
    {
        var expiryDate = DateTime.Now.AddDays(days);
        
        var inventories =  await _inventoryRepository.SearchWithIncludesAsync(
            i=> i.WarrantyEndDate.HasValue && 
                i.WarrantyEndDate.Value <= expiryDate && 
                i.WarrantyEndDate.Value >= DateTime.Now,
            "AssignedUser", "SupportCompany");
        
        return Ok(_mapper.Map<IEnumerable<InventoryDto>>(inventories));
    }

    [HttpGet("by-location/{location}")]
    public async Task<ActionResult<IEnumerable<InventoryDto>>> GetInventoryByLocation(string location)
    {
        var inventories = await _inventoryRepository.SearchWithIncludesAsync(
            i=> i.Location.ToLower() == location.ToLower(),
            "AssignedUser","SupportCompany");
        
        return Ok(_mapper.Map<IEnumerable<InventoryDto>>(inventories));
    }
    
    
}
}
