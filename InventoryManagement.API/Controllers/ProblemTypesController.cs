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
            .Include(p => p.Group)
            .Include(p => p.Group.Department)
            .ToListAsync();
        return Ok(problemTypes);
    }

    [HttpGet("active")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<ProblemType>>> GetActiveProblemTypes()
    {
        var problemTypes = await _context.ProblemTypes
            .Include(p => p.Group)
            .Include(p => p.Group.Department)
            .Where(p => p.IsActive)
            .ToListAsync();
        return Ok(problemTypes);
    }

    //[Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<ProblemType>> CreateProblemType(CreateProblemTypeDto dto)
    {
        var group = await _context.Groups.FindAsync(dto.GroupId);
        if (group == null)
            return BadRequest("Invalid Group ID");

        var problemType = new ProblemType
        {
            Name = dto.Name,
            GroupId = dto.GroupId,
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

        var Group = await _context.Groups.FindAsync(problemType.GroupId);
        if (Group == null)
            return BadRequest("Invalid Group ID");

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