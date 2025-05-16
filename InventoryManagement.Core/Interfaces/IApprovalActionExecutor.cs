using System.Threading.Tasks;

namespace InventoryManagement.Core.Interfaces;

public interface IApprovalActionExecutor
{
    Task ExecuteActionAsync(string entityType, int entityId, string actionType);
} 