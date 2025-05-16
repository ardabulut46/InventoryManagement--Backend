using InventoryManagement.Core.DTOs.Inventory;
using InventoryManagement.Core.Entities;
using InventoryManagement.Core.Enums;
using Microsoft.AspNetCore.Http; // For IFormFile if service handles file uploads directly
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InventoryManagement.Core.Interfaces;

public interface IInventoryService
{
    Task<IEnumerable<InventoryDto>> GetInventoriesAsync();
    Task<IEnumerable<InventoryDto>> GetMyGroupInventoriesAsync(int userId);
    Task<InventoryDto?> GetInventoryByIdAsync(int id);
    
    Task<InventoryDto> CreateInventoryAsync(CreateInventoryDto createInventoryDto, int creatingUserId, List<IFormFile>? files, string? fileDescription);
    Task UpdateInventoryAsync(int id, UpdateInventoryDto updateInventoryDto, int updatingUserId); // Added updatingUserId
    Task UpdateInventoryStatusAsync(int id, InventoryStatus status, int updatingUserId); // Added updatingUserId

    // For Approval Workflow
    Task<Inventory> RequestDeleteInventoryAsync(int inventoryId, int requestingUserId, string? comments = null);
    Task ExecuteSoftDeleteInventoryAsync(int inventoryId); // Called by ApprovalService

    Task AssignUserToInventoryAsync(int inventoryId, int newUserId, int assigningUserId, string? notes = null); // Added assigningUserId
    Task<IEnumerable<InventoryHistoryDto>> GetInventoryAssignmentHistoryAsync(int inventoryId);

    Task<IEnumerable<InventoryDto>> SearchInventoriesAsync(
        string? searchTerm, InventoryStatus? status, bool? hasUser, 
        int? familyId, int? typeId, int? brandId, int? modelId);

    // Methods related to attachments (could also be a separate IAttachmentService)
    Task UploadAttachmentsAsync(int inventoryId, List<IFormFile> files, string? description, int uploaderUserId);
    Task DeleteAttachmentAsync(int inventoryId, int attachmentId, int deleterUserId);
    // GetAttachments is likely handled by GetInventoryByIdAsync including attachments

    // Methods related to warranty status (could be combined with Search or kept separate)
    Task<IEnumerable<InventoryDto>> GetWarrantyExpiringInventoriesAsync(int days = 30);
    Task<IEnumerable<InventoryDto>> GetWarrantyExpiredInventoriesAsync();
    Task<IEnumerable<InventoryDto>> GetActiveWarrantyInventoriesAsync();

    Task<IEnumerable<InventoryDto>> GetInventoriesByLocationAsync(string location);

    // Note: Methods like DownloadExcelTemplate, ImportFromExcel, GetFamilies, GetTypes etc., 
    // might stay in the controller if they are purely for presentation/data shaping for UI, 
    // or could be moved to specialized services if they involve more complex logic.
} 