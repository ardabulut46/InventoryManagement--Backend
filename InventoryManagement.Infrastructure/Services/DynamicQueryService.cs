using AutoMapper;
using InventoryManagement.Core.Enums;
using InventoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Infrastructure.Services;

public class DynamicQueryService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public DynamicQueryService(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<object> ExecuteQuery(string queryType)
    {
        return queryType.ToLower() switch
        {
            "total_inventories" => await _context.Inventories.CountAsync(),
            "active_inventories" => await _context.Inventories.CountAsync(i => i.Status == InventoryStatus.Available),
            "total_tickets" => await _context.Tickets.CountAsync(),
            "open_tickets" => await _context.Tickets.CountAsync(t => t.Status == TicketStatus.Open),
            "total_users" => await _context.Users.CountAsync(),
            "warranty_expired" => await _context.Inventories.CountAsync(i => i.WarrantyEndDate < DateTime.Now),
            "most_common_inventory_type" => await _context.Inventories
                .GroupBy(i => i.Type)
                .OrderByDescending(g => g.Count())
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .FirstOrDefaultAsync(),
            _ => throw new ArgumentException("Unknown query type")
        };
    }
}