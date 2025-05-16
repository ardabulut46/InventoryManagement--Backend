using System;
using System.Threading.Tasks;
using InventoryManagement.Core.Entities;
using InventoryManagement.Core.Interfaces;

namespace InventoryManagement.Core.Services;

public class ApprovalActionExecutor : IApprovalActionExecutor
{
    private readonly IInventoryService _inventoryService;

    public ApprovalActionExecutor(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    public async Task ExecuteActionAsync(string entityType, int entityId, string actionType)
    {
        if (entityType == nameof(Inventory))
        {
            switch (actionType.ToLower())
            {
                case "delete":
                    await _inventoryService.ExecuteSoftDeleteInventoryAsync(entityId);
                    break;
                default:
                    throw new InvalidOperationException($"Unknown action type '{actionType}' for entity type '{entityType}'");
            }
        }
        else
        {
            throw new InvalidOperationException($"Unknown entity type '{entityType}'");
        }
    }
} 