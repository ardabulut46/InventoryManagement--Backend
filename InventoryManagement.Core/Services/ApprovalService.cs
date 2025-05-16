using InventoryManagement.Core.Entities;
using InventoryManagement.Core.Enums;
using InventoryManagement.Core.Interfaces;
using Microsoft.AspNetCore.Identity; // Required for UserManager
using Microsoft.EntityFrameworkCore; // Keep for .Include() on _userManager.Users if still needed elsewhere
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InventoryManagement.Core.Services;

public class ApprovalService : IApprovalService
{
    private readonly IGenericRepository<ApprovalRequest> _approvalRequestRepository;
    private readonly UserManager<User> _userManager;
    private readonly IGenericRepository<Group> _groupRepository; // Added for fetching Group
    private readonly INotificationService _notificationService;
    private readonly IApprovalActionExecutor _actionExecutor;
    // We'll need a way to execute the actual action upon approval later.
    // This might involve a dictionary of action handlers or specific service calls.

    public ApprovalService(
        IGenericRepository<ApprovalRequest> approvalRequestRepository,
        UserManager<User> userManager,
        IGenericRepository<Group> groupRepository, // Added for fetching Group
        INotificationService notificationService,
        IApprovalActionExecutor actionExecutor)
    {
        _approvalRequestRepository = approvalRequestRepository;
        _userManager = userManager;
        _groupRepository = groupRepository; // Added for fetching Group
        _notificationService = notificationService;
        _actionExecutor = actionExecutor;
    }

    public async Task<ApprovalRequest> CreateApprovalRequestAsync(
        int requestingUserId,
        string entityType,
        int entityId,
        string actionType,
        string? requesterComments = null)
    {
        var requester = await _userManager.FindByIdAsync(requestingUserId.ToString());
        if (requester == null) throw new ArgumentException("Requester not found.", nameof(requestingUserId));
        if (!requester.GroupId.HasValue) throw new InvalidOperationException("Requester is not assigned to a group, cannot determine manager.");

        var group = await _groupRepository.GetByIdWithIncludesAsync(requester.GroupId.Value, nameof(Group.Manager));
        if (group == null) throw new InvalidOperationException($"Group with ID {requester.GroupId.Value} not found for requester.");
        if (!group.ManagerId.HasValue || group.Manager == null)
            throw new InvalidOperationException($"Group '{group.Name}' does not have a manager assigned or manager could not be loaded.");

        int managerUserId = group.ManagerId.Value;

        var approvalRequest = new ApprovalRequest
        {
            RequestingUserId = requestingUserId,
            ApproverUserId = managerUserId,
            EntityType = entityType,
            EntityId = entityId,
            ActionType = actionType,
            Status = ApprovalStatus.Pending,
            RequestedDate = DateTime.UtcNow,
            RequesterComments = requesterComments
        };

        await _approvalRequestRepository.AddAsync(approvalRequest);

        // Use manager's username if available, otherwise ID
        var managerUserName = group.Manager?.UserName ?? managerUserId.ToString();
        await _notificationService.CreateNotificationAsync(
            managerUserId,
            $"New approval request: {actionType} for {entityType} ID {entityId} from {requester.UserName}.",
            NotificationType.ApprovalRequest,
            nameof(ApprovalRequest),
            approvalRequest.Id);

        return approvalRequest;
    }

    public async Task ApproveRequestAsync(int approvalRequestId, int approverUserId, string? approverComments = null)
    {
        var request = await GetApprovalRequestByIdAsync(approvalRequestId);
        if (request == null) throw new ArgumentException("Approval request not found.", nameof(approvalRequestId));
        if (request.ApproverUserId != approverUserId) throw new UnauthorizedAccessException("User not authorized to approve this request.");
        if (request.Status != ApprovalStatus.Pending) throw new InvalidOperationException("Request is not in a pending state.");

        try 
        {
            // Execute the action using the action executor
            await _actionExecutor.ExecuteActionAsync(request.EntityType, request.EntityId, request.ActionType);

            // Update approval request status
            request.Status = ApprovalStatus.Approved;
            request.ActionDate = DateTime.UtcNow;
            request.ApproverComments = approverComments;
            await _approvalRequestRepository.UpdateAsync(request);

            // Send notification
            await _notificationService.CreateNotificationAsync(
                request.RequestingUserId,
                $"Your request to {request.ActionType} {request.EntityType} ID {request.EntityId} has been approved. Approver: {request.ApproverUser?.UserName ?? approverUserId.ToString()}",
                NotificationType.ApprovalApproved,
                nameof(ApprovalRequest),
                request.Id);
        }
        catch (Exception ex)
        {
            // If action execution fails, we don't want to update the request status
            throw new InvalidOperationException($"Failed to execute {request.ActionType} action: {ex.Message}", ex);
        }
    }

    public async Task RejectRequestAsync(int approvalRequestId, int approverUserId, string? approverComments = null)
    {
        var request = await GetApprovalRequestByIdAsync(approvalRequestId);
        if (request == null) throw new ArgumentException("Approval request not found.", nameof(approvalRequestId));
        if (request.ApproverUserId != approverUserId) throw new UnauthorizedAccessException("User not authorized to reject this request.");
        if (request.Status != ApprovalStatus.Pending) throw new InvalidOperationException("Request is not in a pending state.");

        request.Status = ApprovalStatus.Rejected;
        request.ActionDate = DateTime.UtcNow;
        request.ApproverComments = approverComments;
        await _approvalRequestRepository.UpdateAsync(request);

        await _notificationService.CreateNotificationAsync(
            request.RequestingUserId,
            $"Your request to {request.ActionType} {request.EntityType} ID {request.EntityId} has been rejected by {request.ApproverUser?.UserName ?? approverUserId.ToString()}. Comments: {approverComments}",
            NotificationType.ApprovalRejected,
            nameof(ApprovalRequest),
            request.Id);
    }

    public async Task CancelRequestAsync(int approvalRequestId, int requestingUserId)
    {
        var request = await GetApprovalRequestByIdAsync(approvalRequestId);
        if (request == null) throw new ArgumentException("Approval request not found.", nameof(approvalRequestId));
        if (request.RequestingUserId != requestingUserId) throw new UnauthorizedAccessException("User not authorized to cancel this request.");
        if (request.Status != ApprovalStatus.Pending) throw new InvalidOperationException("Only pending requests can be cancelled.");

        request.Status = ApprovalStatus.Cancelled;
        request.ActionDate = DateTime.UtcNow;
        // request.ApproverComments = "Cancelled by requester"; // Optional
        await _approvalRequestRepository.UpdateAsync(request);

        // Optionally notify manager that a request was cancelled
        await _notificationService.CreateNotificationAsync(
            request.ApproverUserId,
            $"Approval request for {request.ActionType} {request.EntityType} ID {request.EntityId} by {request.RequestingUser?.UserName ?? requestingUserId.ToString()} was cancelled.",
            NotificationType.Information, // Or a specific "RequestCancelled" type
            nameof(ApprovalRequest),
            request.Id);
    }

    public async Task<IEnumerable<ApprovalRequest>> GetPendingApprovalsForManagerAsync(int managerUserId)
    {
        return await _approvalRequestRepository.SearchWithIncludesAsync(
            ar => ar.ApproverUserId == managerUserId && ar.Status == ApprovalStatus.Pending,
            nameof(ApprovalRequest.RequestingUser) // Pass navigation property name as string for include
        );
    }
    
    public async Task<ApprovalRequest?> GetApprovalRequestByIdAsync(int approvalRequestId)
    {
         return await _approvalRequestRepository.GetByIdWithIncludesAsync(approvalRequestId, 
            nameof(ApprovalRequest.RequestingUser), 
            nameof(ApprovalRequest.ApproverUser)
         );
    }
} 