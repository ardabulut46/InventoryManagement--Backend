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
    
    public InventoryController(IGenericRepository<Inventory> inventoryRepository, IMapper mapper, IValidator<CreateInventoryDto> validator, IUserRepository userRepository)
    {
        _inventoryRepository = inventoryRepository;
        _mapper = mapper;
        _validator = validator;
        _userRepository = userRepository;
    }


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
    }

    [HttpGet]
   //[Authorize(Policy = "CanView")]
    public async Task<ActionResult<IEnumerable<InventoryDto>>> GetInventories()
    {
        var inventories = await _inventoryRepository.GetAllWithIncludesAsync(
            "AssignedUser",
            "SupportCompany",
            "InventoryHistory");
        return Ok(_mapper.Map<IEnumerable<InventoryDto>>(inventories));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<InventoryDto>> GetInventory(int id)
    {
        var inventory = await _inventoryRepository.GetByIdWithIncludesAsync(
            id,
            "AssignedUser",
            "SupportCompany",
            "InventoryHistory");
        
        if (inventory == null)
            return NotFound();

        return Ok(_mapper.Map<InventoryDto>(inventory));
    }

    //[Authorize(Roles = "Admin")]
    [Authorize(Policy = "CanCreate")]
    [HttpPost]
    public async Task<ActionResult<InventoryDto>> CreateInventory(CreateInventoryDto createInventoryDto)
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
        
        var inventory = _mapper.Map<Inventory>(createInventoryDto);
        var createdInventory = await _inventoryRepository.AddAsync(inventory);
        
        if (createInventoryDto.AssignedUserId.HasValue)
        {
            await _inventoryRepository.AddInventoryHistoryAsync(
                createdInventory.Id,
                createInventoryDto.AssignedUserId.Value,
                "İlk atama");
        }

        return CreatedAtAction(
            nameof(GetInventory), 
            new { id = createdInventory.Id }, 
            _mapper.Map<InventoryDto>(createdInventory));
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
        var inventory = await _inventoryRepository.GetByIdAsync(id);
        if (inventory == null)
            return NotFound();
        
        // Eğer assigned user değişmişse ve yeni bir atama yapılmışsa
        if (inventory.AssignedUserId != updateInventoryDto.AssignedUserId && 
            updateInventoryDto.AssignedUserId.HasValue)
        {
            await _inventoryRepository.AddInventoryHistoryAsync(
                id,
                updateInventoryDto.AssignedUserId.Value,
                "User reassignment"); //  "Kullanıcı değişimi" gibi bir not
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

        await _inventoryRepository.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<InventoryDto>>> SearchInventories(
        [FromQuery] string? searchTerm,
        [FromQuery] string? status,
        [FromQuery] bool? hasUser)
    {
        var query = await _inventoryRepository.SearchWithIncludesAsync(
            inventory => 
                (string.IsNullOrEmpty(searchTerm) || inventory.Model.Contains(searchTerm)) &&
                (string.IsNullOrEmpty(status) || inventory.Status == status) &&
                (!hasUser.HasValue || (hasUser.Value ? inventory.AssignedUserId != null : inventory.AssignedUserId == null)),
            "AssignedUser", "SupportCompany", "InventoryHistory");

        
        var result = _mapper.Map<IEnumerable<InventoryDto>>(query);
        return Ok(result);
    }
    
    [HttpPost("{inventoryId}/upload-invoice")]
    [Authorize]
    public async Task<IActionResult> UploadInvoice(int inventoryId, IFormFile file)
    {
        // Verify inventory exists
        var inventory = await _inventoryRepository.GetByIdAsync(inventoryId);
        if (inventory == null)
            return NotFound("Inventory not found");

        if (file == null || file.Length == 0)
            return BadRequest("No file was provided");

        // Define allowed file types
        var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".txt", ".jpg", ".jpeg", ".png" };
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(fileExtension))
            return BadRequest("Invalid file type. Allowed types are: pdf, doc, docx, txt, jpg, jpeg, png");

        try
        {
            // Create unique filename
            var fileName = $"{Guid.NewGuid()}{fileExtension}";
        
            // Define upload path - make sure this directory exists
            var uploadPath = Path.Combine("wwwroot", "uploads", "invoices");
        
            // Create directory if it doesn't exist
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            var filePath = Path.Combine(uploadPath, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Update inventory with file path
            var relativePath = Path.Combine("uploads", "invoices", fileName).Replace("\\", "/");
            inventory.InvoiceAttachmentPath = relativePath;
            await _inventoryRepository.UpdateAsync(inventory);

            return Ok(new { filePath = relativePath });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
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
            i => i.AssignedUserId == userId,
            "AssignedUser",
            "SupportCompany",
            "InventoryHistory"
        );

        return Ok(_mapper.Map<IEnumerable<InventoryDto>>(inventories));
    }

    [HttpGet("warranty-expiring")]
    public async Task<ActionResult<IEnumerable<InventoryDto>>> GetWarrantyExpiringInventories(int days = 30)
    {
        var expiryDate = DateTime.Now.AddDays(days);
        
        var inventories =  await _inventoryRepository.SearchWithIncludesAsync(
            i=> i.WarrantyEndDate.HasValue && 
                i.WarrantyEndDate.Value <= expiryDate && 
                i.WarrantyEndDate.Value >= DateTime.Now,
            "AssignedUser", "SupportCompany");
        
        return Ok(_mapper.Map<IEnumerable<InventoryDto>>(inventories));
    }

    [HttpGet("by-location/{location}")]
    public async Task<ActionResult<IEnumerable<InventoryDto>>> GetInventoryByLocation(string location)
    {
        var inventories = await _inventoryRepository.SearchWithIncludesAsync(
            i=> i.Location.ToLower() == location.ToLower(),
            "AssignedUser","SupportCompany");
        
        return Ok(_mapper.Map<IEnumerable<InventoryDto>>(inventories));
    }
    
    
}
}
