using InventoryManagement.Core.DTOs.SolutionType;
using InventoryManagement.Core.Entities;
using InventoryManagement.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class SolutionTypesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public SolutionTypesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SolutionType>>> GetSolutionTypes()
    {
        var solutionTypes = await _context.SolutionTypes.ToListAsync();
        return Ok(solutionTypes);
    }

    [HttpGet("active")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<SolutionType>>> GetActiveSolutionTypes()
    {
        var solutionTypes = await _context.SolutionTypes
            .Where(s => s.IsActive)
            .ToListAsync();
        return Ok(solutionTypes);
    }

   // [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<SolutionType>> CreateSolutionType(CreateSolutionTypeDto dto)
    {
        var solutionType = new SolutionType
        {
            Name = dto.Name,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        _context.SolutionTypes.Add(solutionType);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetSolutionTypes), new { id = solutionType.Id }, solutionType);
    }
    
    //[Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSolutionType(int id, SolutionType solutionType)
    {
        if (id != solutionType.Id)
            return BadRequest();

        _context.Entry(solutionType).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.SolutionTypes.AnyAsync(s => s.Id == id))
                return NotFound();
            throw;
        }

        return NoContent();
    }
}