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
using InventoryManagement.API.Extensions;
using InventoryManagement.Core.DTOs.InventoryAttachment;
using InventoryManagement.Core.Enums;
using InventoryManagement.Core.Helpers;
using InventoryManagement.Infrastructure.Repositories;
using QuestPDF.Fluent;
using Microsoft.Extensions.Logging;

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
    private readonly IInventoryService _inventoryService;
    private readonly ILogger<InventoryController> _logger;
    private readonly IApprovalService _approvalService;
    
    public InventoryController(IGenericRepository<Inventory> inventoryRepository, IMapper mapper, IValidator<CreateInventoryDto> validator, IUserRepository userRepository, ApplicationDbContext context, IInventoryService inventoryService, ILogger<InventoryController> logger, IApprovalService approvalService)
    {
        _inventoryRepository = inventoryRepository;
        _mapper = mapper;
        _validator = validator;
        _userRepository = userRepository;
        _context = context;
        _inventoryService = inventoryService;
        _logger = logger;
        _approvalService = approvalService;
    }


    [HttpGet("export-template")]
    public IActionResult DownloadExcelTemplate()
    {
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Envanter Şablonu");

        // Add headers with required fields marked with asterisk (Turkish headers)
        // Removed location-related fields as they'll be populated from the user
        var headers = new[]
        {
            "Barkod*", "Seri Numarası*", "Aile*", "Tip*", "Marka*", "Model*",
            "Durum", "Satın Alma Tarihi", "Satın Alma Fiyatı", "Para Birimi",
            "Garanti Başlangıç Tarihi", "Garanti Bitiş Tarihi", "Tedarikçi",
            "Atanan Kullanıcı Email", "Destek Şirketi ID"
        };

        for (int i = 0; i < headers.Length; i++)
        {
            worksheet.Cells[1, i + 1].Value = headers[i];
            worksheet.Cells[1, i + 1].Style.Font.Bold = true;
        }

        // Add example data
        worksheet.Cells[2, 1].Value = "BRKDS123";
        worksheet.Cells[2, 2].Value = "SN123456";
        worksheet.Cells[2, 3].Value = "Bilgisayar"; // Family name instead of ID
        worksheet.Cells[2, 4].Value = "Dizüstü"; // Type name instead of ID
        worksheet.Cells[2, 5].Value = "Dell"; // Brand name instead of ID
        worksheet.Cells[2, 6].Value = "Latitude 5420"; // Model name instead of ID
        worksheet.Cells[2, 7].Value = "Aktif";
        worksheet.Cells[2, 8].Value = DateTime.Now.ToString("yyyy-MM-dd");
        worksheet.Cells[2, 9].Value = "5000";
        worksheet.Cells[2, 10].Value = "TRY"; // PurchaseCurrency
        worksheet.Cells[2, 11].Value = DateTime.Now.ToString("yyyy-MM-dd");
        worksheet.Cells[2, 12].Value = DateTime.Now.AddYears(1).ToString("yyyy-MM-dd");
        worksheet.Cells[2, 13].Value = "Tedarikçi Adı";
        worksheet.Cells[2, 14].Value = "kullanici@ornek.com"; // Example user email
        worksheet.Cells[2, 15].Value = "1"; // Example SupportCompanyId

        // Add a second example row to show multiple entries
        worksheet.Cells[3, 1].Value = "BRKDASF324";
        worksheet.Cells[3, 2].Value = "SN654321";
        worksheet.Cells[3, 3].Value = "Monitör"; // Family name instead of ID
        worksheet.Cells[3, 4].Value = "LCD"; // Type name instead of ID
        worksheet.Cells[3, 5].Value = "HP"; // Brand name instead of ID
        worksheet.Cells[3, 6].Value = "EliteDisplay E243"; // Model name instead of ID
        worksheet.Cells[3, 7].Value = "Aktif";
        worksheet.Cells[3, 8].Value = DateTime.Now.ToString("yyyy-MM-dd");
        worksheet.Cells[3, 9].Value = "7500";
        worksheet.Cells[3, 10].Value = "TRY"; // PurchaseCurrency
        worksheet.Cells[3, 11].Value = DateTime.Now.ToString("yyyy-MM-dd");
        worksheet.Cells[3, 12].Value = DateTime.Now.AddYears(1).ToString("yyyy-MM-dd");
        worksheet.Cells[3, 13].Value = "Tedarikçi Adı";
        worksheet.Cells[3, 14].Value = "kullanici2@ornek.com"; // Example user email
        worksheet.Cells[3, 15].Value = "2"; // Example SupportCompanyId

        // Add date format to date columns
        var dateStyle = "yyyy-mm-dd";
        worksheet.Cells[1, 8, worksheet.Dimension.End.Row, 8].Style.Numberformat.Format = dateStyle;
        worksheet.Cells[1, 11, worksheet.Dimension.End.Row, 11].Style.Numberformat.Format = dateStyle;
        worksheet.Cells[1, 12, worksheet.Dimension.End.Row, 12].Style.Numberformat.Format = dateStyle;

        worksheet.Cells.AutoFitColumns();

        var content = package.GetAsByteArray();
        return File(
            content,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "envanter_sablonu.xlsx");
    }

    [HttpPost("import-excel")]
    [Authorize]
    public async Task<IActionResult> ImportFromExcel(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Dosya yüklenmedi");

        if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Lütfen bir Excel dosyası (.xlsx) yükleyin");

        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(currentUserId, out int userId))
        {
            return Unauthorized("Geçersiz kullanıcı ID");
        }

        // Get the current user to populate location-related fields
        var currentUser = await _userRepository.GetByIdAsync(userId);
        if (currentUser == null)
        {
            return NotFound("Kullanıcı bulunamadı");
        }

        try
        {
            // Load all families, types, brands, and models for name-to-id mapping
            var allFamilies = await _context.Families.Where(f => f.IsActive).ToListAsync();
            var allTypes = await _context.InventoryTypes.Where(t => t.IsActive).ToListAsync();
            var allBrands = await _context.Brands.Where(b => b.IsActive).ToListAsync();
            var allModels = await _context.Models.Where(m => m.IsActive).Include(m => m.Brand).ToListAsync();

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
                // Skip empty rows
                if (worksheet.Cells[row, 1].Value == null && worksheet.Cells[row, 2].Value == null)
                    continue;

                try
                {
                    var inventoryDto = new CreateInventoryDto
                    {
                        Barcode = worksheet.Cells[row, 1].Value?.ToString(),
                        SerialNumber = worksheet.Cells[row, 2].Value?.ToString(),
                        CreatedUserId = userId,
                        // Set location information from the current user
                        Location = currentUser.Location,
                        Department = currentUser.Department.ToString(),
                        Room = currentUser.Room,
                        Floor = currentUser.Floor
                        // Block field has been removed
                    };

                    // Handle Family by name
                    var familyName = worksheet.Cells[row, 3].Value?.ToString();
                    if (!string.IsNullOrEmpty(familyName))
                    {
                        var family = allFamilies.FirstOrDefault(f =>
                            f.Name.Equals(familyName, StringComparison.OrdinalIgnoreCase));
                        if (family == null)
                        {
                            errors.Add($"Satır {row}: '{familyName}' adında bir aile bulunamadı");
                            continue;
                        }

                        inventoryDto.FamilyId = family.Id;
                    }
                    else
                    {
                        errors.Add($"Satır {row}: Aile adı boş olamaz");
                        continue;
                    }

                    // Handle Type by name
                    var typeName = worksheet.Cells[row, 4].Value?.ToString();
                    if (!string.IsNullOrEmpty(typeName))
                    {
                        var type = allTypes.FirstOrDefault(t =>
                            t.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase));
                        if (type == null)
                        {
                            errors.Add($"Satır {row}: '{typeName}' adında bir tip bulunamadı");
                            continue;
                        }

                        inventoryDto.TypeId = type.Id;
                    }
                    else
                    {
                        errors.Add($"Satır {row}: Tip adı boş olamaz");
                        continue;
                    }

                    // Handle Brand by name
                    var brandName = worksheet.Cells[row, 5].Value?.ToString();
                    if (!string.IsNullOrEmpty(brandName))
                    {
                        var brand = allBrands.FirstOrDefault(b =>
                            b.Name.Equals(brandName, StringComparison.OrdinalIgnoreCase));
                        if (brand == null)
                        {
                            errors.Add($"Satır {row}: '{brandName}' adında bir marka bulunamadı");
                            continue;
                        }

                        inventoryDto.BrandId = brand.Id;
                    }
                    else
                    {
                        errors.Add($"Satır {row}: Marka adı boş olamaz");
                        continue;
                    }

                    // Handle Model by name (and ensure it belongs to the selected brand)
                    var modelName = worksheet.Cells[row, 6].Value?.ToString();
                    if (!string.IsNullOrEmpty(modelName))
                    {
                        var model = allModels.FirstOrDefault(m =>
                            m.Name.Equals(modelName, StringComparison.OrdinalIgnoreCase) &&
                            m.BrandId == inventoryDto.BrandId);

                        if (model == null)
                        {
                            // Try to find any model with this name, regardless of brand
                            var anyModel = allModels.FirstOrDefault(m =>
                                m.Name.Equals(modelName, StringComparison.OrdinalIgnoreCase));
                            if (anyModel != null)
                            {
                                errors.Add(
                                    $"Satır {row}: '{modelName}' modeli seçilen '{brandName}' markasına ait değil");
                            }
                            else
                            {
                                errors.Add($"Satır {row}: '{modelName}' adında bir model bulunamadı");
                            }

                            continue;
                        }

                        inventoryDto.ModelId = model.Id;
                    }
                    else
                    {
                        errors.Add($"Satır {row}: Model adı boş olamaz");
                        continue;
                    }

                    // Status field (now at column 7)
                    // Status field (now at column 7)
                    string statusText = worksheet.Cells[row, 7].Value?.ToString() ?? "Aktif";
                    inventoryDto.Status = InventoryStatusHelper.MapStringToInventoryStatus(statusText);

                    // Handle dates and numeric values (adjusted column indices)
                    if (DateTime.TryParse(worksheet.Cells[row, 8].Value?.ToString(), out DateTime purchaseDate))
                        
                    if (int.TryParse(worksheet.Cells[row, 9].Value?.ToString(), out int purchasePrice))
                        inventoryDto.PurchasePrice = purchasePrice;

                    // Handle currency enum
                    var currencyStr = worksheet.Cells[row, 10].Value?.ToString();
                    if (!string.IsNullOrEmpty(currencyStr))
                    {
                        switch (currencyStr.ToUpper())
                        {
                            case "TRY":
                                inventoryDto.PurchaseCurrency = PurchaseCurrency.Try;
                                break;
                            case "USD":
                                inventoryDto.PurchaseCurrency = PurchaseCurrency.Usd;
                                break;
                            case "EUR":
                                inventoryDto.PurchaseCurrency = PurchaseCurrency.Eur;
                                break;
                            default:
                                errors.Add($"Satır {row}: Geçersiz para birimi: {currencyStr}. Geçerli değerler: TRY, USD, EUR");
                                break;
                        }
                    }

                    if (DateTime.TryParse(worksheet.Cells[row, 11].Value?.ToString(), out DateTime warrantyStart))
                        inventoryDto.WarrantyStartDate = warrantyStart;

                    if (DateTime.TryParse(worksheet.Cells[row, 12].Value?.ToString(), out DateTime warrantyEnd))
                        inventoryDto.WarrantyEndDate = warrantyEnd;
                    

                    // Handle user assignment by email
                    var userEmail = worksheet.Cells[row, 14].Value?.ToString();
                    if (!string.IsNullOrEmpty(userEmail))
                    {
                        var user = await _userRepository.GetByEmailAsync(userEmail);
                        if (user == null)
                        {
                            errors.Add($"Satır {row}: {userEmail} e-postasına sahip kullanıcı bulunamadı");
                            continue;
                        }

                        inventoryDto.AssignedUserId = user.Id;
                    }

                    if (int.TryParse(worksheet.Cells[row, 15].Value?.ToString(), out int supportCompanyId))
                        inventoryDto.SupportCompanyId = supportCompanyId;

                    // Validate the DTO
                    var validationContext = new ValidationContext(inventoryDto);
                    var validationResults = new List<ValidationResult>();
                    if (!Validator.TryValidateObject(inventoryDto, validationContext, validationResults, true))
                    {
                        errors.Add($"Satır {row}: {string.Join(", ", validationResults.Select(x => x.ErrorMessage))}");
                        continue;
                    }

                    importedInventories.Add(inventoryDto);
                }
                catch (Exception ex)
                {
                    errors.Add($"Satır {row} hata: {ex.Message}");
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

            return Ok(new { Message = $"{importedInventories.Count} envanter başarıyla içe aktarıldı" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Sunucu hatası: {ex.Message}");
        }
    }

    [HttpGet]
    //[Authorize(Policy = "CanView")]
    public async Task<ActionResult<IEnumerable<InventoryDto>>> GetInventories()
    {
        var inventories = await _inventoryService.GetInventoriesAsync();
        return Ok(inventories);
    }
    [HttpGet("group-inventories")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<InventoryDto>>> GetMyGroupInventories()
    {
        // Get current user ID
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(currentUserId, out int userId))
        {
            return Unauthorized("Invalid user ID format");
        }

        // Get user's group
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null || !user.GroupId.HasValue)
        {
            return BadRequest("User not found or not assigned to a group");
        }

        // Get all users in this group
        var groupUsers = await _context.Users
            .Where(u => u.GroupId == user.GroupId)
            .Select(u => u.Id)
            .ToListAsync();

        // Get all inventories assigned to users in this group
        var inventories = await _inventoryRepository.SearchWithIncludesAsync(
            i => i.IsActive && i.AssignedUserId.HasValue && groupUsers.Contains(i.AssignedUserId.Value),
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


    [HttpGet("{id}")]
    public async Task<ActionResult<InventoryDto>> GetInventory(int id)
    {
        var inventory = await _inventoryService.GetInventoryByIdAsync(id);
        if (inventory == null)
            return NotFound();

        return Ok(inventory);
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

    var currentUser = await _userRepository.GetByIdAsync(userId);
    if (currentUser == null)
    {
        return NotFound("User not found");
    }
    
    createInventoryDto.Location = currentUser.Location;
    createInventoryDto.Department = currentUser.Department.ToString();
    createInventoryDto.Room = currentUser.Room;
    createInventoryDto.Floor = currentUser.Floor;

    var validationResult = await _validator.ValidateAsync(createInventoryDto);
    if (!validationResult.IsValid)
    {
        validationResult.AddToModelState(ModelState);
        return BadRequest(ModelState);
    }

    try
    {
        var createdInventoryDto = await _inventoryService.CreateInventoryAsync(createInventoryDto, userId, files, fileDescription);
        return CreatedAtAction(
            nameof(GetInventory),
            new { id = createdInventoryDto.Id },
            createdInventoryDto);
    }
    catch (ArgumentException ex) // Catch specific exceptions thrown by the service
    {
        return BadRequest(ex.Message);
    }
    catch (Exception ex) // Catch any other exceptions
    {
        // Log the exception (using a logger injected into the controller is recommended)
        return StatusCode(500, $"An unexpected error occurred: {ex.Message}");
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
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(currentUserId, out int userId))
        {
            return Unauthorized("Invalid user ID");
        }

        // Basic DTO validation (e.g., required fields, string lengths) can still be done here via model state
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            await _inventoryService.UpdateInventoryAsync(id, updateInventoryDto, userId);
            return NoContent();
        }
        catch (KeyNotFoundException ex) // Specific exception for not found
        {
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex) // Specific exception for bad requests (validation failures from service)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex) // Catch-all for other unexpected errors
        {
            // Log the exception (using a logger injected into the controller is recommended)
            return StatusCode(500, $"An unexpected error occurred while updating inventory: {ex.Message}");
        }
    }
    
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateInventoryStatus(int id, [FromBody] InventoryStatus status)
    {
        var inventory = await _inventoryRepository.GetByIdAsync(id);
        if (inventory == null)
            return NotFound();
    
        // Update the status
        inventory.Status = status;
    
        await _inventoryRepository.UpdateAsync(inventory);
        return Ok(new { Message = $"Inventory status updated to {status}" });
    }
    
    [HttpDelete("{id}")]
    [Authorize] // It's good practice to ensure the user is authenticated
    public async Task<IActionResult> DeleteInventory(int id)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var requestingUserId))
        {
            return Unauthorized("User identifier is missing or invalid.");
        }

        try
        {
            // First validate the inventory
            var inventory = await _inventoryService.RequestDeleteInventoryAsync(id, requestingUserId, null);
            
            // If inventory is valid, create the approval request
            await _approvalService.CreateApprovalRequestAsync(
                requestingUserId,
                nameof(Inventory),
                id,
                "Delete",
                null);

            return Ok(new { message = "Deletion request submitted for approval." });
        }
        catch (ArgumentException ex) when (ex.Message.Contains("not found"))
        {
            _logger.LogWarning(ex, "Failed to request delete for inventory {InventoryId}: {ErrorMessage}", id, ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request to delete inventory {InventoryId}: {ErrorMessage}", id, ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while requesting deletion for inventory {InventoryId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<InventoryDto>>> SearchInventories(
        [FromQuery] string? searchTerm,
        [FromQuery] InventoryStatus? status,
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
                i.Department.Contains(searchTerm));
        }

        if (status.HasValue)
        {
            filteredInventories = filteredInventories.Where(i => i.Status == status.Value);
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
    [Authorize(Policy = "Inventory:Create")]
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
