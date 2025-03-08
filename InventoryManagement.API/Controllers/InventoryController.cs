using System.Security.Claims;
using AutoMapper;
using FluentValidation;
using InventoryManagement.Core.DTOs.Inventory;
using InventoryManagement.Core.Entities;
using InventoryManagement.Core.Interfaces;
using InventoryManagement.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using OfficeOpenXml;
using System.ComponentModel.DataAnnotations;
using InventoryManagement.Core.DTOs.InventoryAttachment;
using InventoryManagement.Infrastructure.Repositories;

namespace InventoryManagement.API.Controllers
{ 
    [ApiController] 
    [Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly IGenericRepository<Inventory> _inventoryRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateInventoryDto> _validator;
    private readonly IUserRepository _userRepository;
    private readonly ApplicationDbContext _context;
    
    public InventoryController(IGenericRepository<Inventory> inventoryRepository, IMapper mapper, IValidator<CreateInventoryDto> validator, IUserRepository userRepository, ApplicationDbContext context)
    {
        _inventoryRepository = inventoryRepository;
        _mapper = mapper;
        _validator = validator;
        _userRepository = userRepository;
        _context = context;
        
    }


    /*
    [HttpGet("export-template")]
    public IActionResult DownloadExcelTemplate()
    {
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Inventory Template");

        // Add headers with required fields marked with asterisk
        var headers = new[]
        {
            "Barcode*", "Serial Number*", "Family*", "Type*", "Brand*", "Model*",
            "Location", "Status", "Room", "Floor", "Block", "Department",
            "Purchase Date", "Warranty Start Date", "Warranty End Date", "Supplier",
            "Assigned User Email", "Support Company ID"
        };

        for (int i = 0; i < headers.Length; i++)
        {
            worksheet.Cells[1, i + 1].Value = headers[i];
            worksheet.Cells[1, i + 1].Style.Font.Bold = true;
        }

        // Add example data
        worksheet.Cells[2, 1].Value = "123456789";
        worksheet.Cells[2, 2].Value = "SN123456";
        worksheet.Cells[2, 3].Value = "Laptop";
        worksheet.Cells[2, 4].Value = "Computer";
        worksheet.Cells[2, 5].Value = "Dell";
        worksheet.Cells[2, 6].Value = "Latitude 5420";
        worksheet.Cells[2, 7].Value = "Head Office";
        worksheet.Cells[2, 8].Value = "Active";
        worksheet.Cells[2, 9].Value = "Room 101";
        worksheet.Cells[2, 10].Value = "1st Floor";
        worksheet.Cells[2, 11].Value = "Block A";
        worksheet.Cells[2, 12].Value = "IT Department";
        worksheet.Cells[2, 13].Value = DateTime.Now.ToString("yyyy-MM-dd");
        worksheet.Cells[2, 14].Value = DateTime.Now.ToString("yyyy-MM-dd");
        worksheet.Cells[2, 15].Value = DateTime.Now.AddYears(1).ToString("yyyy-MM-dd");
        worksheet.Cells[2, 16].Value = "Supplier Name";
        worksheet.Cells[2, 17].Value = "user@example.com"; // Example user email
        worksheet.Cells[2, 18].Value = "1"; // Example SupportCompanyId

        // Add date format to date columns
        var dateStyle = "yyyy-mm-dd";
        worksheet.Cells[1, 13, worksheet.Dimension.End.Row, 13].Style.Numberformat.Format = dateStyle;
        worksheet.Cells[1, 14, worksheet.Dimension.End.Row, 14].Style.Numberformat.Format = dateStyle;
        worksheet.Cells[1, 15, worksheet.Dimension.End.Row, 15].Style.Numberformat.Format = dateStyle;

        worksheet.Cells.AutoFitColumns();

        var content = package.GetAsByteArray();
        return File(
            content,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "inventory_template.xlsx");
    }

    [HttpPost("import-excel")]
    [Authorize(Policy = "CanCreate")]
    public async Task<IActionResult> ImportFromExcel(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded");

        if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Please upload an Excel file (.xlsx)");

        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(currentUserId, out int userId))
        {
            return Unauthorized("Invalid user ID");
        }

        try
        {
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);

            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets[0];
            var rowCount = worksheet.Dimension.Rows;

            var importedInventories = new List<CreateInventoryDto>();
            var errors = new List<string>();

            // Start from row 2 (assuming row 1 is header)
            for (int row = 2; row <= rowCount; row++)
            {
                try
                {
                    var inventoryDto = new CreateInventoryDto
                    {
                        Barcode = worksheet.Cells[row, 1].Value?.ToString(),
                        SerialNumber = worksheet.Cells[row, 2].Value?.ToString(),
                        Family = worksheet.Cells[row, 3].Value?.ToString(),
                        Type = worksheet.Cells[row, 4].Value?.ToString(),
                        Brand = worksheet.Cells[row, 5].Value?.ToString(),
                        Model = worksheet.Cells[row, 6].Value?.ToString(),
                        Location = worksheet.Cells[row, 7].Value?.ToString(),
                        Status = worksheet.Cells[row, 8].Value?.ToString(),
                        Room = worksheet.Cells[row, 9].Value?.ToString(),
                        Floor = worksheet.Cells[row, 10].Value?.ToString(),
                        Block = worksheet.Cells[row, 11].Value?.ToString(),
                        Department = worksheet.Cells[row, 12].Value?.ToString(),
                        CreatedUserId = userId
                    };

                    // Handle dates
                    if (DateTime.TryParse(worksheet.Cells[row, 13].Value?.ToString(), out DateTime purchaseDate))
                        inventoryDto.PurchaseDate = purchaseDate;

                    if (DateTime.TryParse(worksheet.Cells[row, 14].Value?.ToString(), out DateTime warrantyStart))
                        inventoryDto.WarrantyStartDate = warrantyStart;

                    if (DateTime.TryParse(worksheet.Cells[row, 15].Value?.ToString(), out DateTime warrantyEnd))
                        inventoryDto.WarrantyEndDate = warrantyEnd;

                    inventoryDto.Supplier = worksheet.Cells[row, 16].Value?.ToString();

                    // Handle IDs
                     var userEmail = worksheet.Cells[row, 17].Value?.ToString();
                     if (!string.IsNullOrEmpty(userEmail))
                     {
                         var user = await _userRepository.GetByEmailAsync(userEmail);
                         if (user == null)
                         {
                             errors.Add("$Row {row}: User with email {userEmail} not found");
                             continue;
                         }
                         inventoryDto.AssignedUserId = user.Id;
                     }

                     if (int.TryParse(worksheet.Cells[row, 18].Value?.ToString(), out int supportCompanyId))
                        inventoryDto.SupportCompanyId = supportCompanyId;

                    // Validate the DTO
                    var validationContext = new ValidationContext(inventoryDto);
                    var validationResults = new List<ValidationResult>();
                    if (!Validator.TryValidateObject(inventoryDto, validationContext, validationResults, true))
                    {
                        errors.Add($"Row {row}: {string.Join(", ", validationResults.Select(x => x.ErrorMessage))}");
                        continue;
                    }

                    importedInventories.Add(inventoryDto);
                }
                catch (Exception ex)
                {
                    errors.Add($"Error in row {row}: {ex.Message}");
                }
            }

            if (errors.Any())
            {
                return BadRequest(new { Errors = errors });
            }

            // Save all valid inventories
            foreach (var inventoryDto in importedInventories)
            {
                var inventory = _mapper.Map<Inventory>(inventoryDto);
                await _inventoryRepository.AddAsync(inventory);
            }

            return Ok(new { Message = $"Successfully imported {importedInventories.Count} inventories" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }*/

    [HttpGet]
    //[Authorize(Policy = "CanView")]
    public async Task<ActionResult<IEnumerable<InventoryDto>>> GetInventories()
    {
        var inventories = await _inventoryRepository.SearchWithIncludesAsync(
            i=>i.IsActive,
            "AssignedUser",
            "LastUser",
            "CreatedUser",
            "SupportCompany",
            "InventoryHistory",
            "Family",
            "Type",
            "Brand",
            "Model",
            "Attachments");
        return Ok(_mapper.Map<IEnumerable<InventoryDto>>(inventories));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<InventoryDto>> GetInventory(int id)
    {
        var inventory = await _inventoryRepository.GetByIdWithIncludesAsync(
            id,
            "AssignedUser",
            "LastUser",
            "CreatedUser",
            "SupportCompany",
            "InventoryHistory",
            "Attachments",
            "Family",
            "Type",
            "Brand",
            "Model");

        if (inventory == null || !inventory.IsActive)
            return NotFound();

        return Ok(_mapper.Map<InventoryDto>(inventory));
    }

    [HttpPost]
    public async Task<ActionResult<InventoryDto>> CreateInventory(
        [FromForm] CreateInventoryDto createInventoryDto,
        [FromForm] List<IFormFile> files = null,
        [FromForm] string fileDescription = "")
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(currentUserId, out int userId))
        {
            return Unauthorized("Invalid user ID");
        }

        var validationResult = await _validator.ValidateAsync(createInventoryDto);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        createInventoryDto.CreatedUserId = userId;

        // Declare createdInventory outside the try block
        Inventory createdInventory = null;

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Verify that the referenced entities exist
            var familyExists = await _context.Families.AnyAsync(f => f.Id == createInventoryDto.FamilyId);
            if (!familyExists)
            {
                return BadRequest($"Family with ID {createInventoryDto.FamilyId} does not exist.");
            }

            var typeExists = await _context.InventoryTypes.AnyAsync(t => t.Id == createInventoryDto.TypeId);
            if (!typeExists)
            {
                return BadRequest($"Type with ID {createInventoryDto.TypeId} does not exist.");
            }

            var brandExists = await _context.Brands.AnyAsync(b => b.Id == createInventoryDto.BrandId);
            if (!brandExists)
            {
                return BadRequest($"Brand with ID {createInventoryDto.BrandId} does not exist.");
            }

            var modelExists = await _context.Models.AnyAsync(m => m.Id == createInventoryDto.ModelId);
            if (!modelExists)
            {
                return BadRequest($"Model with ID {createInventoryDto.ModelId} does not exist.");
            }

            var inventory = _mapper.Map<Inventory>(createInventoryDto);
            createdInventory = await _inventoryRepository.AddAsync(inventory);

            if (createInventoryDto.AssignedUserId.HasValue)
            {
                await _inventoryRepository.AddInventoryHistoryAsync(
                    createdInventory.Id,
                    createInventoryDto.AssignedUserId.Value,
                    "İlk atama");
            }
            
            if (files != null && files.Any() && !files.All(f => f.Length == 0))
            {
                // Define allowed file types
                var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".txt", ".jpg", ".jpeg", ".png" };
                var inventoryId = createdInventory.Id;

                var uploadedFiles = new List<string>(); // Track files we've written to disk
                var attachmentsAdded = false;

                foreach (var file in files)
                {
                    var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        continue; // Skip invalid files
                    }

                    // Create unique filename
                    var fileName = $"{Guid.NewGuid()}{fileExtension}";

                    // Define upload path
                    var uploadPath = Path.Combine("wwwroot", "uploads", "inventory", inventoryId.ToString());

                    // Create directory if it doesn't exist
                    if (!Directory.Exists(uploadPath))
                        Directory.CreateDirectory(uploadPath);

                    var filePath = Path.Combine(uploadPath, fileName);
                    var relativePath = Path.Combine("uploads", "inventory", inventoryId.ToString(), fileName)
                        .Replace("\\", "/");

                    // Save file
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    uploadedFiles.Add(filePath); // Track the file we just wrote

                    // Create attachment
                    var attachment = new InventoryAttachment
                    {
                        InventoryId = inventoryId,
                        FileName = file.FileName,
                        FilePath = relativePath,
                        ContentType = file.ContentType,
                        FileSize = file.Length,
                        UploadDate = DateTime.Now,
                        Description = fileDescription
                    };

                    // Add attachment to inventory
                    if (createdInventory.Attachments == null)
                    {
                        createdInventory.Attachments = new List<InventoryAttachment>();
                    }

                    createdInventory.Attachments.Add(attachment);
                    attachmentsAdded = true;
                }

                // Update inventory with attachments if any were added
                if (attachmentsAdded)
                {
                    await _inventoryRepository.UpdateAsync(createdInventory);
                }
            }

            // If we've reached this point without exceptions, commit the transaction
            await transaction.CommitAsync();

            return CreatedAtAction(
                nameof(GetInventory),
                new { id = createdInventory.Id },
                _mapper.Map<InventoryDto>(createdInventory));
        }
        catch (Exception ex)
        {
            // Roll back the transaction
            await transaction.RollbackAsync();

            // Delete any uploaded files if they exist
            CleanupUploadedFiles(createdInventory?.Id);

            return StatusCode(500, $"Failed to create inventory: {ex.Message}");
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
            }
            catch (Exception ex)
            {
                // Add proper logging here
                // _logger.LogWarning(ex, "Failed to clean up uploaded files for inventory {InventoryId}", inventoryId.Value);
            }
        }
    }

    [HttpPut("{id}/assign-user")]
    public async Task<IActionResult> AssignUser(int id, int userId, string notes = null)
    {
        var inventory = await _inventoryRepository.GetByIdWithIncludesAsync(
            id,
            "InventoryHistory");
        if (inventory == null)
            return NotFound();

        // Store current user as last user before updating
        inventory.LastUserId = inventory.AssignedUserId;
    
        // Update to new user
        inventory.AssignedUserId = userId;
    
        // Close the previous assignment by setting its return date
        var currentAssignment = inventory.InventoryHistory
            .Where(h => h.ReturnDate == null)
            .OrderByDescending(h => h.AssignmentDate)
            .FirstOrDefault();

        if (currentAssignment != null)
        {
            currentAssignment.ReturnDate = DateTime.Now;
            currentAssignment.Notes += " | Ürün el değiştirdi";
        }

        // Create new assignment history
        await _inventoryRepository.AddInventoryHistoryAsync(id, userId, notes);
    
        // Save changes
        await _inventoryRepository.UpdateAsync(inventory);

        return NoContent();
    }
    
   /* [HttpPut("{id}/return")]
    public async Task<IActionResult> ReturnInventory(int id, string notes = null)
    {
        var inventory = await _inventoryRepository.GetByIdWithIncludesAsync(
            id,
            "InventoryHistory");
        if (inventory == null)
            return NotFound();

        // Store current user as last user before clearing
        inventory.LastUserId = inventory.AssignedUserId;
    
        // Clear current assignment
        inventory.AssignedUserId = null;

        // Close the current assignment
        var currentAssignment = inventory.InventoryHistory
            .Where(h => h.ReturnDate == null)
            .OrderByDescending(h => h.AssignmentDate)
            .FirstOrDefault();

        if (currentAssignment != null)
        {
            currentAssignment.ReturnDate = DateTime.Now;
            currentAssignment.Notes += " | " + (notes ?? "Returned to inventory");
        }

        await _inventoryRepository.UpdateAsync(inventory);

        return NoContent();
    }*/
    
    [HttpGet("{id}/assignment-history")]
    public async Task<ActionResult<IEnumerable<InventoryHistoryDto>>> GetAssignmentHistory(int id)
    {
        var inventory = await _inventoryRepository.GetByIdWithIncludesAsync(
            id,
            "InventoryHistory.User");
    
        if (inventory == null)
            return NotFound();

        var history = inventory.InventoryHistory
            .OrderByDescending(h => h.AssignmentDate);

        return Ok(_mapper.Map<IEnumerable<InventoryHistoryDto>>(history));
    }

    // Eğer bir ürün,envanter el değiştirdiyse buradan güncellenecek, ilk atamalar inventory'i post ederken verilebilir  
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateInventory(int id, UpdateInventoryDto updateInventoryDto)
    {
        var inventory = await _inventoryRepository.GetByIdWithIncludesAsync(
            id,
            "AssignedUser",
            "LastUser",
            "CreatedUser",
            "SupportCompany",
            "InventoryHistory",
            "Attachments",
            "Family",
            "Type",
            "Brand",
            "Model");

        if (inventory == null)
            return NotFound();

        // Verify that the referenced entities exist
        var familyExists = await _context.Families.AnyAsync(f => f.Id == updateInventoryDto.FamilyId);
        if (!familyExists)
        {
            return BadRequest($"Family with ID {updateInventoryDto.FamilyId} does not exist.");
        }

        var typeExists = await _context.InventoryTypes.AnyAsync(t => t.Id == updateInventoryDto.TypeId);
        if (!typeExists)
        {
            return BadRequest($"Type with ID {updateInventoryDto.TypeId} does not exist.");
        }

        var brandExists = await _context.Brands.AnyAsync(b => b.Id == updateInventoryDto.BrandId);
        if (!brandExists)
        {
            return BadRequest($"Brand with ID {updateInventoryDto.BrandId} does not exist.");
        }

        var modelExists = await _context.Models.AnyAsync(m => m.Id == updateInventoryDto.ModelId);
        if (!modelExists)
        {
            return BadRequest($"Model with ID {updateInventoryDto.ModelId} does not exist.");
        }

        // Check if assigned user exists if provided
        if (updateInventoryDto.AssignedUserId.HasValue)
        {
            var userExists = await _userRepository.GetByIdAsync(updateInventoryDto.AssignedUserId.Value);
            if (userExists == null)
            {
                return BadRequest($"User with ID {updateInventoryDto.AssignedUserId.Value} does not exist.");
            }

            // If the assigned user is changing, update the LastUserId
            if (inventory.AssignedUserId != updateInventoryDto.AssignedUserId)
            {
                inventory.LastUserId = inventory.AssignedUserId;
            }
        }

        // Check if support company exists if provided
        if (updateInventoryDto.SupportCompanyId.HasValue)
        {
            var companyExists =
                await _context.Companies.AnyAsync(c => c.Id == updateInventoryDto.SupportCompanyId.Value);
            if (!companyExists)
            {
                return BadRequest($"Company with ID {updateInventoryDto.SupportCompanyId.Value} does not exist.");
            }
        }

        _mapper.Map(updateInventoryDto, inventory);
        await _inventoryRepository.UpdateAsync(inventory);
        return NoContent();
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteInventory(int id)
    {
        var inventory = await _inventoryRepository.GetByIdAsync(id);
        if (inventory == null)
            return NotFound();

        inventory.IsActive = false;
        await _inventoryRepository.UpdateAsync(inventory);
        return NoContent();
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<InventoryDto>>> SearchInventories(
        [FromQuery] string? searchTerm,
        [FromQuery] string? status,
        [FromQuery] bool? hasUser,
        [FromQuery] int? familyId,
        [FromQuery] int? typeId,
        [FromQuery] int? brandId,
        [FromQuery] int? modelId)
    {
        var inventories = await _inventoryRepository.SearchWithIncludesAsync(
            i=>i.IsActive,
            "AssignedUser",
            "LastUser",
            "CreatedUser",
            "SupportCompany",
            "Family",
            "Type",
            "Brand",
            "Model");

        var filteredInventories = inventories.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            filteredInventories = filteredInventories.Where(i =>
                i.Barcode.Contains(searchTerm) ||
                i.SerialNumber.Contains(searchTerm) ||
                (i.Family != null && i.Family.Name.Contains(searchTerm)) ||
                (i.Type != null && i.Type.Name.Contains(searchTerm)) ||
                (i.Brand != null && i.Brand.Name.Contains(searchTerm)) ||
                (i.Model != null && i.Model.Name.Contains(searchTerm)) ||
                i.Location.Contains(searchTerm) ||
                i.Room.Contains(searchTerm) ||
                i.Floor.Contains(searchTerm) ||
                i.Block.Contains(searchTerm) ||
                i.Department.Contains(searchTerm) ||
                i.Supplier.Contains(searchTerm));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            filteredInventories = filteredInventories.Where(i => i.Status == status);
        }

        if (hasUser.HasValue)
        {
            filteredInventories = hasUser.Value
                ? filteredInventories.Where(i => i.AssignedUserId.HasValue)
                : filteredInventories.Where(i => !i.AssignedUserId.HasValue);
        }

        if (familyId.HasValue)
        {
            filteredInventories = filteredInventories.Where(i => i.FamilyId == familyId.Value);
        }

        if (typeId.HasValue)
        {
            filteredInventories = filteredInventories.Where(i => i.TypeId == typeId.Value);
        }

        if (brandId.HasValue)
        {
            filteredInventories = filteredInventories.Where(i => i.BrandId == brandId.Value);
        }

        if (modelId.HasValue)
        {
            filteredInventories = filteredInventories.Where(i => i.ModelId == modelId.Value);
        }

        return Ok(_mapper.Map<IEnumerable<InventoryDto>>(filteredInventories.ToList()));
    }

    [HttpPost("{inventoryId}/upload-invoice")]
    [Authorize]
    public async Task<IActionResult> UploadAttachments(int inventoryId, [FromForm] List<IFormFile> files,
        [FromForm] string description = "")
    {
        // Verify inventory exists
        var inventory = await _inventoryRepository.GetByIdWithIncludesAsync(inventoryId, "Attachments");
        if (inventory == null)
            return NotFound("Inventory not found");

        if (files == null || !files.Any() || files.All(f => f.Length == 0))
            return BadRequest("No files were provided");

        // Define allowed file types
        var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".txt", ".jpg", ".jpeg", ".png" };

        var results = new List<object>();

        try
        {
            foreach (var file in files)
            {
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    results.Add(new { fileName = file.FileName, error = "Invalid file type" });
                    continue;
                }

                // Create unique filename
                var fileName = $"{Guid.NewGuid()}{fileExtension}";

                // Define upload path - make sure this directory exists
                var uploadPath = Path.Combine("wwwroot", "uploads", "inventory", inventoryId.ToString());

                // Create directory if it doesn't exist
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                var filePath = Path.Combine(uploadPath, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Create attachment
                var relativePath = Path.Combine("uploads", "inventory", inventoryId.ToString(), fileName)
                    .Replace("\\", "/");

                var attachment = new InventoryAttachment
                {
                    InventoryId = inventoryId,
                    FileName = file.FileName,
                    FilePath = relativePath,
                    ContentType = file.ContentType,
                    FileSize = file.Length,
                    UploadDate = DateTime.Now,
                    Description = description
                };

                inventory.Attachments.Add(attachment);
                results.Add(new { fileName = file.FileName, filePath = relativePath });
            }

            await _inventoryRepository.UpdateAsync(inventory);
            return Ok(results);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
        
    }
    
    [HttpGet("{inventoryId}/attachments")]
    public async Task<ActionResult<IEnumerable<InventoryAttachmentDto>>> GetAttachments(int inventoryId)
    {
        var inventory = await _inventoryRepository.GetByIdWithIncludesAsync(inventoryId, "Attachments");
        if (inventory == null)
            return NotFound("Inventory not found");
        
        return Ok(_mapper.Map<IEnumerable<InventoryAttachmentDto>>(inventory.Attachments));
    }
    
    [HttpGet("{inventoryId}/attachments/{attachmentId}/download")]
    public async Task<IActionResult> DownloadAttachment(int inventoryId, int attachmentId)
    {
        var inventory = await _inventoryRepository.GetByIdWithIncludesAsync(inventoryId, "Attachments");
        if (inventory == null)
            return NotFound("Inventory not found");
        
        var attachment = inventory.Attachments.FirstOrDefault(a => a.Id == attachmentId);
        if (attachment == null)
            return NotFound("Attachment not found");
        
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", attachment.FilePath);
        if (!System.IO.File.Exists(filePath))
            return NotFound("Attachment file not found");
        
        var extension = Path.GetExtension(attachment.FileName).ToLowerInvariant();
        var mimeType = GetMimeType(extension);
    
        var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        return new FileStreamResult(fileStream, mimeType)
        {
            FileDownloadName = attachment.FileName
        };
    }

    [HttpGet("{inventoryId}/download-invoice")]
    public async Task<IActionResult> DownloadInvoice(int inventoryId)
    {
        var inventory = await _inventoryRepository.GetByIdAsync(inventoryId);
        if (inventory == null)
            return NotFound("Inventory not found");

        if (string.IsNullOrEmpty(inventory.InvoiceAttachmentPath))
            return NotFound("No invoice attachment found for this inventory");

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", inventory.InvoiceAttachmentPath);
        if (!System.IO.File.Exists(filePath))
            return NotFound("Invoice file not found");

        // Get the original filename and extension
        var fileName = Path.GetFileName(inventory.InvoiceAttachmentPath);
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var mimeType = GetMimeType(extension);

        // Use FileStreamResult for better memory management
        var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        return new FileStreamResult(fileStream, mimeType)
        {
            FileDownloadName = fileName // This preserves the original filename with extension
        };
    }

    private string GetMimeType(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".txt" => "text/plain",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };
    }
    
    [HttpDelete("{inventoryId}/attachments/{attachmentId}")]
    public async Task<IActionResult> DeleteAttachment(int inventoryId, int attachmentId)
    {
        var inventory = await _inventoryRepository.GetByIdWithIncludesAsync(inventoryId, "Attachments");
        if (inventory == null)
            return NotFound("Inventory not found");
        
        var attachment = inventory.Attachments.FirstOrDefault(a => a.Id == attachmentId);
        if (attachment == null)
            return NotFound("Attachment not found");
    
        try
        {
            // Delete the file from the file system
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", attachment.FilePath);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        
            // Remove the attachment from the inventory
            inventory.Attachments.Remove(attachment);
            await _inventoryRepository.UpdateAsync(inventory);
        
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet("assigned")]
    [Authorize] 
    public async Task<ActionResult<IEnumerable<InventoryDto>>> GetMyInventories()
    {
        // Get the current user's ID from claims
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(currentUserId))
        {
            return Unauthorized("User ID not found in claims");
        }

        // Convert the string ID to int since your AssignedUserId is an int
        if (!int.TryParse(currentUserId, out int userId))
        {
            return BadRequest("Invalid user ID format");
        }

        var inventories = await _inventoryRepository.SearchWithIncludesAsync(
            i => i.AssignedUserId == userId && i.IsActive,
            "AssignedUser",
            "LastUser",
            "CreatedUser",
            "SupportCompany",
            "InventoryHistory",
            "Family",
            "Type",
            "Brand",
            "Model",
            "Attachments"
        );

        return Ok(_mapper.Map<IEnumerable<InventoryDto>>(inventories));
    }

    [HttpGet("warranty-expiring")]
    public async Task<ActionResult<IEnumerable<InventoryDto>>> GetWarrantyExpiringInventories(int days = 30)
    {
        var expiryDate = DateTime.Now.AddDays(days);

        var inventories = await _inventoryRepository.SearchWithIncludesAsync(
            i => i.IsActive && i.WarrantyEndDate.HasValue && 
                 i.WarrantyEndDate.Value <= expiryDate && 
                 i.WarrantyEndDate.Value >= DateTime.Now,
            "AssignedUser", "LastUser", "CreatedUser", "SupportCompany", 
            "Family", "Type", "Brand", "Model", "Attachments");

        return Ok(_mapper.Map<IEnumerable<InventoryDto>>(inventories));
    }

    [HttpGet("warranty-expired")]
    public async Task<ActionResult<IEnumerable<InventoryDto>>> GetWarrantyExpiredInventories()
    {
        var inventories = await _inventoryRepository.SearchWithIncludesAsync(
            i => i.IsActive && i.WarrantyEndDate.HasValue && 
                 i.WarrantyEndDate.Value < DateTime.Now,
            "AssignedUser", "LastUser", "CreatedUser", "SupportCompany", 
            "Family", "Type", "Brand", "Model", "Attachments");

        return Ok(_mapper.Map<IEnumerable<InventoryDto>>(inventories));
    }

    [HttpGet("warranty-active")]
    public async Task<ActionResult<IEnumerable<InventoryDto>>> GetActiveWarrantyInventories()
    {
        var thirtyDaysFromNow = DateTime.Now.AddDays(30);

        var inventories = await _inventoryRepository.SearchWithIncludesAsync(
            i => i.IsActive && i.WarrantyEndDate.HasValue && 
                 i.WarrantyEndDate.Value > thirtyDaysFromNow,
            "AssignedUser", "LastUser", "CreatedUser", "SupportCompany", 
            "Family", "Type", "Brand", "Model", "Attachments");

        return Ok(_mapper.Map<IEnumerable<InventoryDto>>(inventories));
    }

    [HttpGet("by-location/{location}")]
    public async Task<ActionResult<IEnumerable<InventoryDto>>> GetInventoryByLocation(string location)
    {
        var inventories = await _inventoryRepository.SearchWithIncludesAsync(
            i => i.IsActive && i.Location.ToLower() == location.ToLower(),
            "AssignedUser", "LastUser", "CreatedUser", "SupportCompany", 
            "Family", "Type", "Brand", "Model", "Attachments");

        return Ok(_mapper.Map<IEnumerable<InventoryDto>>(inventories));
    }
    
    [HttpGet("families")]
    public async Task<ActionResult<IEnumerable<object>>> GetFamilies()
    {
        var families = await _context.Families.Where(f => f.IsActive).Select(f => new { f.Id, f.Name }).ToListAsync();
        return Ok(families);
    }

    [HttpGet("types")]
    public async Task<ActionResult<IEnumerable<object>>> GetTypes()
    {
        var types = await _context.InventoryTypes.Where(t => t.IsActive).Select(t => new { t.Id, t.Name }).ToListAsync();
        return Ok(types);
    }

    [HttpGet("brands")]
    public async Task<ActionResult<IEnumerable<object>>> GetBrands()
    {
        var brands = await _context.Brands.Where(b => b.IsActive).Select(b => new { b.Id, b.Name }).ToListAsync();
        return Ok(brands);
    }

    [HttpGet("models")]
    public async Task<ActionResult<IEnumerable<object>>> GetModels()
    {
        var models = await _context.Models.Where(m => m.IsActive)
            .Select(m => new { m.Id, m.Name, m.BrandId })
            .ToListAsync();
        return Ok(models);
    }

    [HttpGet("models/by-brand/{brandId}")]
    public async Task<ActionResult<IEnumerable<object>>> GetModelsByBrand(int brandId)
    {
        var models = await _context.Models
            .Where(m => m.BrandId == brandId && m.IsActive)
            .Select(m => new { m.Id, m.Name })
            .ToListAsync();
        return Ok(models);
    }
    
    
}
}
