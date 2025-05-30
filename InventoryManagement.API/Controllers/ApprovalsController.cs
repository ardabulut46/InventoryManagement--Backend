using InventoryManagement.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Threading.Tasks;
using InventoryManagement.Core.DTOs.Approval; // For RejectApprovalRequestDto
using System.Collections.Generic;             // For List<object>
using System;                                 // For Exception, KeyNotFoundException etc.
using System.Linq;                            // For .Any()
using Microsoft.AspNetCore.Http;              // For StatusCodes
// Add DTOs namespace if you create one for Approval e.g. InventoryManagement.Core.DTOs.Approval;

namespace InventoryManagement.API.Controllers
{
    [ApiController]
    [Route("api/approvals")]
    [Authorize] // General authorization for the controller, can be refined per method
    public class ApprovalsController : ControllerBase
    {
        private readonly IApprovalService _approvalService;
        private readonly ILogger<ApprovalsController> _logger;
        // private readonly UserManager<User> _userManager; // If needed for more specific checks

        public ApprovalsController(
            IApprovalService approvalService,
            ILogger<ApprovalsController> logger
            // UserManager<User> userManager
            )
        {
            _approvalService = approvalService;
            _logger = logger;
            // _userManager = userManager;
        }

        [HttpGet("pending")]
        // [Authorize(Roles = "Manager")] // Example: Add specific role authorization if you have a "Manager" role
        public async Task<IActionResult> GetPendingApprovals()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var managerId))
            {
                _logger.LogWarning("Attempt to access pending approvals with invalid user identifier.");
                return Unauthorized("User identifier is missing or invalid.");
            }

            try
            {
                var pendingApprovals = await _approvalService.GetPendingApprovalsForManagerAsync(managerId);
                if (pendingApprovals == null || !pendingApprovals.Any())
                {
                    return Ok(new List<object>()); // Return empty list if none, or OkObjectResult with a message
                }
                return Ok(pendingApprovals);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending approvals for manager {ManagerId}", managerId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving pending approvals.");
            }
        }

        [HttpGet("all-requests")]
        public async Task<IActionResult> GetAllRequests()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var managerId))
            {
                _logger.LogWarning("Attempt to access pending approvals with invalid user identifier.");
                return Unauthorized("User identifier is missing or invalid.");
            }

            try
            {
                var allRequests = await _approvalService.GetAllRequestsForManagerAsync(managerId);
                if (allRequests == null || !allRequests.Any())
                {
                    return Ok(new List<object>());
                }

                return Ok(allRequests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all requests for manager {ManagerId}", managerId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving all requests.");
            }
        }

        [HttpPost("{id}/approve")]
        public async Task<IActionResult> ApproveRequest(int id, [FromBody] ApproveApprovalRequestDto? approvalInput)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var approverId))
            {
                _logger.LogWarning("Attempt to approve request {ApprovalId} with invalid user identifier.", id);
                return Unauthorized("User identifier is missing or invalid.");
            }

            try
            {
                await _approvalService.ApproveRequestAsync(id, approverId, approvalInput?.Comments);
                return Ok(new { message = "Request approved successfully." });
            }
            catch (KeyNotFoundException ex) // Specific exception if approval request not found
            {
                _logger.LogWarning(ex, "Approval request {ApprovalId} not found for approving user {ApproverId}.", id, approverId);
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex) // If service layer checks if approverId matches assigned manager
            {
                _logger.LogWarning(ex, "User {ApproverId} is not authorized to approve request {ApprovalId}.", approverId, id);
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
            catch (InvalidOperationException ex) // E.g., if the request is not in a pending state
            {
                 _logger.LogWarning(ex, "Invalid operation to approve request {ApprovalId} by user {ApproverId}: {ErrorMessage}", id, approverId, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex) // Catch-all for other errors, including those from executing the action (e.g., soft delete)
            {
                _logger.LogError(ex, "An error occurred while approving request {ApprovalId} by user {ApproverId}. Error: {ErrorMessage}", id, approverId, ex.Message);
                // Consider if the error message from ex is safe to return to the client or if a generic message is better.
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = $"An error occurred while approving the request: {ex.Message}" });
            }
        }

        [HttpPost("{id}/reject")]
        public async Task<IActionResult> RejectRequest(int id, [FromBody] RejectApprovalRequestDto? rejectionInput)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var rejecterId))
            {
                _logger.LogWarning("Attempt to reject request {ApprovalId} with invalid user identifier.", id);
                return Unauthorized("User identifier is missing or invalid.");
            }

            try
            {
                await _approvalService.RejectRequestAsync(id, rejecterId, rejectionInput?.Comments);
                return Ok(new { message = "Request rejected successfully." });
            }
            catch (KeyNotFoundException ex) 
            {
                _logger.LogWarning(ex, "Approval request {ApprovalId} not found for rejecting user {RejecterId}.", id, rejecterId);
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex) 
            {
                _logger.LogWarning(ex, "User {RejecterId} is not authorized to reject request {ApprovalId}.", rejecterId, id);
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
            catch (InvalidOperationException ex) 
            {
                 _logger.LogWarning(ex, "Invalid operation to reject request {ApprovalId} by user {RejecterId}: {ErrorMessage}", id, rejecterId, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while rejecting request {ApprovalId} by user {RejecterId}. Error: {ErrorMessage}", id, rejecterId, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred while rejecting the request." });
            }
        }

        // Endpoints will be added here
    }
} 