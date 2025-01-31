using InventoryManagement.Core.DTOs.ProblemType;
using InventoryManagement.Core.Entities;
using InventoryManagement.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class ProblemTypesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ProblemTypesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProblemType>>> GetProblemTypes()
    {
        var problemTypes = await _context.ProblemTypes
            .Include(p => p.Department)
            .ToListAsync();
        return Ok(problemTypes);
    }

    [HttpGet("active")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<ProblemType>>> GetActiveProblemTypes()
    {
        var problemTypes = await _context.ProblemTypes
            .Include(p => p.Department)
            .Where(p => p.IsActive)
            .ToListAsync();
        return Ok(problemTypes);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<ProblemType>> CreateProblemType(CreateProblemTypeDto dto)
    {
        var department = await _context.Departments.FindAsync(dto.DepartmentId);
        if (department == null)
            return BadRequest("Invalid department ID");

        var problemType = new ProblemType
        {
            Name = dto.Name,
            DepartmentId = dto.DepartmentId,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        _context.ProblemTypes.Add(problemType);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProblemTypes), new { id = problemType.Id }, problemType);
    }
    
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProblemType(int id, ProblemType problemType)
    {
        if (id != problemType.Id)
            return BadRequest();

        var department = await _context.Departments.FindAsync(problemType.DepartmentId);
        if (department == null)
            return BadRequest("Invalid department ID");

        _context.Entry(problemType).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.ProblemTypes.AnyAsync(p => p.Id == id))
                return NotFound();
            throw;
        }

        return NoContent();
    }
}