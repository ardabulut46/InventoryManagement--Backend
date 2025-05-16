using AutoMapper;
using InventoryManagement.Core.DTOs.Inventory;
using InventoryManagement.Core.Entities;
using InventoryManagement.Core.Enums;
using InventoryManagement.Core.Interfaces;
using Microsoft.AspNetCore.Http; // For IFormFile
using Microsoft.AspNetCore.Identity; // For UserManager
// using Microsoft.EntityFrameworkCore; // No longer needed for this service directly
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
// using InventoryManagement.Infrastructure.Data; // REMOVED: This caused the circular dependency
// Potentially: using Microsoft.AspNetCore.Hosting; // For IWebHostEnvironment if saving files
using Microsoft.Extensions.Logging; // For logging
using System.IO; // For Path operations

namespace InventoryManagement.Core.Services;

public class InventoryService : IInventoryService
{
    private readonly IGenericRepository<Inventory> _inventoryRepository;
    private readonly IGenericRepository<Family> _familyRepository;
    private readonly IGenericRepository<InventoryType> _inventoryTypeRepository;
    private readonly IGenericRepository<Brand> _brandRepository;
    private readonly IGenericRepository<Model> _modelRepository;
    private readonly IGenericRepository<Company> _companyRepository; 
    private readonly UserManager<User> _userManager; // For user lookups by email etc.
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork; // ADDED
    // private readonly IWebHostEnvironment _hostingEnvironment; // If saving files
    // private readonly ApplicationDbContext _context; // REMOVED
    private readonly ILogger<InventoryService> _logger;


    public InventoryService(
        IGenericRepository<Inventory> inventoryRepository,
        IGenericRepository<Family> familyRepository,
        IGenericRepository<InventoryType> inventoryTypeRepository,
        IGenericRepository<Brand> brandRepository,
        IGenericRepository<Model> modelRepository,
        IGenericRepository<Company> companyRepository,
        UserManager<User> userManager,
        IMapper mapper,
        IUnitOfWork unitOfWork, // ADDED
        ILogger<InventoryService> logger
        // IWebHostEnvironment hostingEnvironment
        )
    {
        _inventoryRepository = inventoryRepository;
        _familyRepository = familyRepository;
        _inventoryTypeRepository = inventoryTypeRepository;
        _brandRepository = brandRepository;
        _modelRepository = modelRepository;
        _companyRepository = companyRepository;
        _userManager = userManager;
        _mapper = mapper;
        _unitOfWork = unitOfWork; // ADDED
        // _context = context; // REMOVED
        _logger = logger;
        // _hostingEnvironment = hostingEnvironment;
    }

    // --- CRUD --- 
    public async Task<InventoryDto> CreateInventoryAsync(CreateInventoryDto createInventoryDto, int creatingUserId, List<IFormFile>? files, string? fileDescription)
    {
        // Set the creator ID (already part of createInventoryDto or passed in)
        createInventoryDto.CreatedUserId = creatingUserId;

        // Set the status to a default value if not provided
        if (!createInventoryDto.Status.HasValue)
        {
            createInventoryDto.Status = InventoryStatus.Available;
        }
        
        Inventory? createdInventoryEntity = null;

        // using var transaction = await _context.Database.BeginTransactionAsync(); // REMOVED
        await _unitOfWork.BeginTransactionAsync(); // ADDED
        try
        {
            // Verify that the referenced entities exist
            var familyExists = (await _familyRepository.SearchAsync(f => f.Id == createInventoryDto.FamilyId && f.IsActive)).Any();
            if (!familyExists)
            {
                throw new ArgumentException($"Active family with ID {createInventoryDto.FamilyId} does not exist.");
            }

            var typeExists = (await _inventoryTypeRepository.SearchAsync(t => t.Id == createInventoryDto.TypeId && t.IsActive)).Any();
            if (!typeExists)
            {
                throw new ArgumentException($"Active type with ID {createInventoryDto.TypeId} does not exist.");
            }

            var brandExists = (await _brandRepository.SearchAsync(b => b.Id == createInventoryDto.BrandId && b.IsActive)).Any();
            if (!brandExists)
            {
                throw new ArgumentException($"Active brand with ID {createInventoryDto.BrandId} does not exist.");
            }

            var modelExists = (await _modelRepository.SearchAsync(m => m.Id == createInventoryDto.ModelId && m.IsActive && m.BrandId == createInventoryDto.BrandId)).Any();
            if (!modelExists)
            {
                throw new ArgumentException($"Active model with ID {createInventoryDto.ModelId} (belonging to brand {createInventoryDto.BrandId}) does not exist.");
            }

            if (createInventoryDto.AssignedUserId.HasValue)
            {
                var assignedUser = await _userManager.FindByIdAsync(createInventoryDto.AssignedUserId.Value.ToString());
                if (assignedUser == null)
                {
                    throw new ArgumentException($"Assigned user with ID {createInventoryDto.AssignedUserId.Value} does not exist.");
                }
            }
            
            if (createInventoryDto.SupportCompanyId.HasValue)
            {
                var companyExists = (await _companyRepository.SearchAsync(c => c.Id == createInventoryDto.SupportCompanyId.Value && c.IsActive)).Any();
                if(!companyExists)
                {
                     throw new ArgumentException($"Active Support Company with ID {createInventoryDto.SupportCompanyId.Value} does not exist.");
                }
            }


            var inventory = _mapper.Map<Inventory>(createInventoryDto);
            createdInventoryEntity = await _inventoryRepository.AddAsync(inventory); // This calls SaveChanges internally

            if (createInventoryDto.AssignedUserId.HasValue)
            {
                // This also calls SaveChanges internally via GenericRepository
                await _inventoryRepository.AddInventoryHistoryAsync(
                    createdInventoryEntity.Id,
                    createInventoryDto.AssignedUserId.Value,
                    "İlk atama"); // "Initial assignment" 
            }

            if (files != null && files.Any() && !files.All(f => f.Length == 0))
            {
                var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".txt", ".jpg", ".jpeg", ".png" };
                var inventoryId = createdInventoryEntity.Id;
                bool attachmentsAdded = false;

                foreach (var file in files)
                {
                    var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        _logger.LogWarning("Skipping file {FileName} due to invalid extension {FileExtension} for inventory {InventoryId}", file.FileName, fileExtension, inventoryId);
                        continue; 
                    }

                    var fileName = $"{Guid.NewGuid()}{fileExtension}";
                    var uploadPath = Path.Combine("wwwroot", "uploads", "inventory", inventoryId.ToString());

                    if (!Directory.Exists(uploadPath))
                        Directory.CreateDirectory(uploadPath);

                    var filePath = Path.Combine(uploadPath, fileName);
                    var relativePath = Path.Combine("uploads", "inventory", inventoryId.ToString(), fileName).Replace("\\", "/");

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    var attachment = new InventoryAttachment
                    {
                        InventoryId = inventoryId,
                        FileName = file.FileName, // Original filename
                        FilePath = relativePath,
                        ContentType = file.ContentType,
                        FileSize = file.Length,
                        UploadDate = DateTime.UtcNow,
                        Description = fileDescription
                    };

                    if (createdInventoryEntity.Attachments == null)
                    {
                        createdInventoryEntity.Attachments = new List<InventoryAttachment>();
                    }
                    createdInventoryEntity.Attachments.Add(attachment);
                    attachmentsAdded = true;
                }

                if (attachmentsAdded)
                {
                    // This calls SaveChanges internally. All these SaveChanges calls will be part of the UoW transaction.
                    await _inventoryRepository.UpdateAsync(createdInventoryEntity); 
                }
            }

            // await transaction.CommitAsync(); // REMOVED
            await _unitOfWork.CommitAsync(); // ADDED - This will call SaveChanges() then commit the transaction.
            
            // We need to fetch the entity with includes for the DTO mapping if attachments were added
            // This read operation does not need to be part of the transaction typically, but it's fine.
            var resultInventory = await _inventoryRepository.GetByIdWithIncludesAsync(createdInventoryEntity.Id, nameof(Inventory.Attachments));
            return _mapper.Map<InventoryDto>(resultInventory ?? createdInventoryEntity);
        }
        catch (Exception ex)
        {
            // await transaction.RollbackAsync(); // REMOVED
            await _unitOfWork.RollbackAsync(); // ADDED
            _logger.LogError(ex, "Error creating inventory. Cleaning up uploaded files for potential inventory ID {InventoryId}", createdInventoryEntity?.Id);
            CleanupUploadedFiles(createdInventoryEntity?.Id); // Call cleanup
            throw; // Re-throw the exception to be handled by the controller or global error handler
        }
    }

    private void CleanupUploadedFiles(int? inventoryId)
    {
        if (!inventoryId.HasValue) return;

        var uploadPath = Path.Combine("wwwroot", "uploads", "inventory", inventoryId.Value.ToString());
        if (Directory.Exists(uploadPath))
        {
            try
            {
                Directory.Delete(uploadPath, true); // Recursive delete
                 _logger.LogInformation("Successfully cleaned up uploaded files for inventory {InventoryId}", inventoryId.Value);
            }
            catch (IOException ex) // More specific exception
            {
                _logger.LogWarning(ex, "Failed to clean up uploaded files for inventory {InventoryId} due to IOException. Files might be in use.", inventoryId.Value);
            }
            catch (UnauthorizedAccessException ex)
            {
                 _logger.LogWarning(ex, "Failed to clean up uploaded files for inventory {InventoryId} due to UnauthorizedAccessException.", inventoryId.Value);
            }
            catch (Exception ex) // Catch-all for other unexpected errors
            {
                _logger.LogError(ex, "An unexpected error occurred while cleaning up uploaded files for inventory {InventoryId}", inventoryId.Value);
            }
        }
    }

    public async Task<IEnumerable<InventoryDto>> GetInventoriesAsync()
    {
        var inventories = await _inventoryRepository.SearchWithIncludesAsync(
            i => i.IsActive,
            nameof(Inventory.AssignedUser),
            nameof(Inventory.LastUser),
            nameof(Inventory.CreatedUser),
            nameof(Inventory.SupportCompany),
            nameof(Inventory.InventoryHistory),
            nameof(Inventory.Family),
            nameof(Inventory.Type),
            nameof(Inventory.Brand),
            nameof(Inventory.Model),
            nameof(Inventory.Attachments));
        return _mapper.Map<IEnumerable<InventoryDto>>(inventories);
    }

    public async Task<InventoryDto?> GetInventoryByIdAsync(int id)
    {
        var inventory = await _inventoryRepository.GetByIdWithIncludesAsync(
            id,
            nameof(Inventory.AssignedUser),
            nameof(Inventory.LastUser),
            nameof(Inventory.CreatedUser),
            nameof(Inventory.SupportCompany),
            nameof(Inventory.InventoryHistory),
            nameof(Inventory.Attachments),
            nameof(Inventory.Family),
            nameof(Inventory.Type),
            nameof(Inventory.Brand),
            nameof(Inventory.Model));

        if (inventory == null || !inventory.IsActive)
            return null; // Service returns null, controller handles NotFound()

        return _mapper.Map<InventoryDto>(inventory);
    }

    public async Task UpdateInventoryAsync(int id, UpdateInventoryDto updateInventoryDto, int updatingUserId)
    {
        var inventory = await _inventoryRepository.GetByIdAsync(id);
        if (inventory == null || !inventory.IsActive)
        {
            throw new KeyNotFoundException($"Active inventory with ID {id} not found.");
        }

        // Verify that the referenced entities exist and are active
        if (updateInventoryDto.FamilyId.HasValue) {
            var familyExists = (await _familyRepository.SearchAsync(f => f.Id == updateInventoryDto.FamilyId && f.IsActive)).Any();
            if (!familyExists) throw new ArgumentException($"Active Family with ID {updateInventoryDto.FamilyId} does not exist.");
        }
        
        if (updateInventoryDto.TypeId.HasValue) {
            var typeExists = (await _inventoryTypeRepository.SearchAsync(t => t.Id == updateInventoryDto.TypeId && t.IsActive)).Any();
            if (!typeExists) throw new ArgumentException($"Active Type with ID {updateInventoryDto.TypeId} does not exist.");
        }

        if (updateInventoryDto.BrandId.HasValue) {
            var brandExists = (await _brandRepository.SearchAsync(b => b.Id == updateInventoryDto.BrandId && b.IsActive)).Any();
            if (!brandExists) throw new ArgumentException($"Active Brand with ID {updateInventoryDto.BrandId} does not exist.");
        }

        if (updateInventoryDto.ModelId.HasValue) {
             if(!updateInventoryDto.BrandId.HasValue && inventory.BrandId == 0) {
                throw new ArgumentException("BrandId must be provided when ModelId is being updated if the inventory does not already have a BrandId.");
            }
            var modelExists = (await _modelRepository.SearchAsync(m => m.Id == updateInventoryDto.ModelId && m.IsActive && m.BrandId == (updateInventoryDto.BrandId ?? inventory.BrandId))).Any();
            if (!modelExists) throw new ArgumentException($"Active Model with ID {updateInventoryDto.ModelId} (belonging to brand {updateInventoryDto.BrandId ?? inventory.BrandId}) does not exist.");
        }

        // Check if assigned user exists if provided
        if (updateInventoryDto.AssignedUserId.HasValue)
        {
            var userExists = await _userManager.FindByIdAsync(updateInventoryDto.AssignedUserId.Value.ToString());
            if (userExists == null) // Assuming UserManager returns null if user not found
            {
                throw new ArgumentException($"User with ID {updateInventoryDto.AssignedUserId.Value} does not exist.");
            }

            // If the assigned user is changing, update the LastUserId
            if (inventory.AssignedUserId != updateInventoryDto.AssignedUserId)
            {
                inventory.LastUserId = inventory.AssignedUserId;
            }
        }
        else // If AssignedUserId is explicitly set to null in the DTO, clear the assignment
        {
            if (inventory.AssignedUserId.HasValue) // It was assigned before
            {
                inventory.LastUserId = inventory.AssignedUserId; 
            }
        }

        // Check if support company exists if provided
        if (updateInventoryDto.SupportCompanyId.HasValue)
        {
            var companyExists = (await _companyRepository.SearchAsync(c => c.Id == updateInventoryDto.SupportCompanyId.Value && c.IsActive)).Any();
            if (!companyExists)
            {
                throw new ArgumentException($"Active Company with ID {updateInventoryDto.SupportCompanyId.Value} does not exist.");
            }
        }

        _mapper.Map(updateInventoryDto, inventory);
        inventory.UpdatedById = updatingUserId;
        inventory.UpdatedDate = DateTime.UtcNow;

        await _inventoryRepository.UpdateAsync(inventory);
    }
    
    public Task UpdateInventoryStatusAsync(int id, InventoryStatus status, int updatingUserId)
    {
        // Logic from controller's UpdateInventoryStatus to be moved here
        throw new NotImplementedException();
    }

    // --- Approval Workflow --- 
    public async Task<Inventory> RequestDeleteInventoryAsync(int inventoryId, int requestingUserId, string? comments = null)
    {
        var inventory = await _inventoryRepository.GetByIdAsync(inventoryId);
        if (inventory == null || !inventory.IsActive)
        {
            throw new ArgumentException("Active inventory not found.", nameof(inventoryId)); 
        }

        // Additional checks can be added here if certain inventories cannot be deleted
        // or if only specific roles can request deletion.
        
        return inventory;
    }

    public async Task ExecuteSoftDeleteInventoryAsync(int inventoryId)
    {
        var inventory = await _inventoryRepository.GetByIdAsync(inventoryId);
        if (inventory == null)
        {
            throw new ArgumentException("Inventory not found for execution of soft delete.", nameof(inventoryId));
        }

        if (!inventory.IsActive)
        {
            _logger.LogWarning("Inventory {InventoryId} is already inactive.", inventoryId);
            return;
        }

        inventory.IsActive = false;
        await _inventoryRepository.UpdateAsync(inventory);
    }


    // --- Other Methods --- 
    public Task<IEnumerable<InventoryDto>> GetMyGroupInventoriesAsync(int userId)
    {
        // Logic from controller's GetMyGroupInventories to be moved here
        throw new NotImplementedException();
    }

    public Task AssignUserToInventoryAsync(int inventoryId, int newUserId, int assigningUserId, string? notes = null)
    {
        // Logic from controller's AssignUser to be moved here
        throw new NotImplementedException();
    }

    public Task<IEnumerable<InventoryHistoryDto>> GetInventoryAssignmentHistoryAsync(int inventoryId)
    {
        // Logic from controller's GetAssignmentHistory to be moved here
        throw new NotImplementedException();
    }

    public Task<IEnumerable<InventoryDto>> SearchInventoriesAsync(string? searchTerm, InventoryStatus? status, bool? hasUser, int? familyId, int? typeId, int? brandId, int? modelId)
    {
        // Logic from controller's SearchInventories to be moved here
        throw new NotImplementedException();
    }

    public Task UploadAttachmentsAsync(int inventoryId, List<IFormFile> files, string? description, int uploaderUserId)
    {
        // Logic from controller's UploadAttachments to be moved here
        throw new NotImplementedException();
    }

    public Task DeleteAttachmentAsync(int inventoryId, int attachmentId, int deleterUserId)
    {
        // Logic from controller's DeleteAttachment to be moved here
        throw new NotImplementedException();
    }

    public Task<IEnumerable<InventoryDto>> GetWarrantyExpiringInventoriesAsync(int days = 30)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<InventoryDto>> GetWarrantyExpiredInventoriesAsync()
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<InventoryDto>> GetActiveWarrantyInventoriesAsync()
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<InventoryDto>> GetInventoriesByLocationAsync(string location)
    {
        throw new NotImplementedException();
    }
} 