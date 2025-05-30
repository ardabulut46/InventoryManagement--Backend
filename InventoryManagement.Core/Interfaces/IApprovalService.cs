using InventoryManagement.Core.DTOs;
using InventoryManagement.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InventoryManagement.Core.Interfaces;

public interface IApprovalService
{
    Task<ApprovalRequest> CreateApprovalRequestAsync(
        int requestingUserId,
        string entityType,
        int entityId,
        string actionType,
        string? requesterComments = null);

    Task ApproveRequestAsync(int approvalRequestId, int approverUserId, string? approverComments = null);

    Task RejectRequestAsync(int approvalRequestId, int approverUserId, string? approverComments = null);

    Task CancelRequestAsync(int approvalRequestId, int requestingUserId); // Requester cancels their own request

    Task<IEnumerable<ApprovalRequest>> GetPendingApprovalsForManagerAsync(int managerUserId);
    Task<IEnumerable<ApprovalRequest>> GetAllRequestsForManagerAsync(int managerUserId);
    
    
    
    Task<ApprovalRequest?> GetApprovalRequestByIdAsync(int approvalRequestId);

    // Potentially, a method to get requests submitted by a user
    // Task<IEnumerable<ApprovalRequest>> GetSubmittedRequestsAsync(int requestingUserId);
} 