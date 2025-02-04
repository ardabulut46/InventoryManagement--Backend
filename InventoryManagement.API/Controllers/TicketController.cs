using AutoMapper;
using InventoryManagement.Core.DTOs.Ticket;
using InventoryManagement.Core.Entities;
using InventoryManagement.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using InventoryManagement.Core.DTOs.Inventory;
using InventoryManagement.Core.DTOs.User;
using InventoryManagement.Core.Enums;
using InventoryManagement.Core.Helpers;
using InventoryManagement.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketController : ControllerBase
    {
        private readonly IGenericRepository<Ticket> _ticketRepository;
        private readonly IUserRepository _userRepository;
        private readonly IGenericRepository<Inventory> _inventoryRepository;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;

        public TicketController(
            IGenericRepository<Ticket> ticketRepository,
            IUserRepository userRepository,
            IGenericRepository<Inventory> inventoryRepository,
            IMapper mapper,
            ApplicationDbContext context)
        {
            _ticketRepository = ticketRepository;
            _userRepository = userRepository;
            _inventoryRepository = inventoryRepository;
            _mapper = mapper;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TicketDto>>> GetTickets()
        {
            // Update the include path: use "Group.Department" instead of "Department"
            var tickets = await _ticketRepository.GetAllWithIncludesAsync(
                "User", "Group.Department", "Group", "Inventory", "CreatedBy");
            return Ok(_mapper.Map<IEnumerable<TicketDto>>(tickets));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TicketDto>> GetTicket(int id)
        {
            var ticket = await _ticketRepository.GetByIdWithIncludesAsync(
                id, "User", "Group.Department", "CreatedBy", "Group", "Inventory");
            if (ticket == null)
                return NotFound();

            if (ticket.AssignedDate != default)
            {
                ticket.IdleDuration = ticket.AssignedDate - ticket.CreatedDate;
            }

            var ticketDto = _mapper.Map<TicketDto>(ticket);
            return Ok(ticketDto);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<TicketDto>> CreateTicket(CreateTicketDto createTicketDto)
        {
            // Get current user ID
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(currentUserId, out int userId))
            {
                return Unauthorized("Invalid user ID");
            }
            
            // Get current user's information including location and room
            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (currentUser == null)
            {
                return BadRequest("User not found");
            }

            // Validate problem type and get associated department
            var problemType = await _context.ProblemTypes.FirstOrDefaultAsync(p => p.Id == createTicketDto.ProblemTypeId && p.IsActive);
            if (problemType == null)
            {
                return BadRequest("Invalid or inactive problem type");
            }

            // Validate inventory if provided
            if (createTicketDto.InventoryId.HasValue)
            {
                var inventory = await _inventoryRepository.GetByIdAsync(createTicketDto.InventoryId.Value);
                if (inventory == null || inventory.AssignedUserId != userId)
                {
                    return BadRequest("You can only create tickets for inventories assigned to you");
                }
            }

            // Generate Registration Number
            var random = new Random();
            var now = DateTime.Now;
            var registrationNumber = $"{random.Next(1000, 9999)}-{now.Year}-{now.Day:D2}-{now.Month:D2}-{now.Hour:D2}";

            var ticket = new Ticket
            {
                RegistrationNumber = registrationNumber,
                GroupId = problemType.GroupId, // Assign based on problem type
                UserId = null, // Initially no user assigned
                ProblemType = problemType.Name,
                Location = currentUser.Location,
                Room = currentUser.Room,
                Subject = createTicketDto.Subject,
                InventoryId = createTicketDto.InventoryId,
                Description = createTicketDto.Description,
                Status = "New",
                AttachmentPath = createTicketDto.AttachmentPath,
                CreatedById = userId,
                Priority = createTicketDto.Priority,
                IdleDuration = null
            };

            var createdTicket = await _ticketRepository.AddAsync(ticket);
            // Reload the ticket with all necessary navigation properties
            var reloadedTicket = await _ticketRepository.GetByIdWithIncludesAsync(
                createdTicket.Id, "User", "Group.Department", "CreatedBy", "Group", "Inventory");
            return CreatedAtAction(
                nameof(GetTicket),
                new { id = reloadedTicket.Id },
                _mapper.Map<TicketDto>(reloadedTicket));
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateTicket(int id, UpdateTicketDto updateTicketDto)
        {
            var ticket = await _ticketRepository.GetByIdWithIncludesAsync(
                id, "User", "Group.Department", "CreatedBy", "Group", "Inventory");
            if (ticket == null)
                return NotFound();

            // Get current user ID
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(currentUserId, out int userId))
            {
                return Unauthorized("Invalid user ID");
            }

            // Verify if the user owns this ticket
            if (ticket.UserId != userId)
            {
                return Forbid("You can only update your own tickets");
            }

            _mapper.Map(updateTicketDto, ticket);
            await _ticketRepository.UpdateAsync(ticket);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteTicket(int id)
        {
            var ticket = await _ticketRepository.GetByIdAsync(id);
            if (ticket == null)
                return NotFound();

            // Get current user ID
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(currentUserId, out int userId))
            {
                return Unauthorized("Invalid user ID");
            }

            // Verify if the user owns this ticket
            if (ticket.UserId != userId)
            {
                return Forbid("You can only delete your own tickets");
            }

            await _ticketRepository.DeleteAsync(id);
            return NoContent();
        }

        // Helper endpoint to get users by department
        [HttpGet("department/{departmentId}/users")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsersByDepartment(int departmentId)
        {
            var allUsers = await _userRepository.GetAllAsync();
            var departmentUsers = allUsers.Where(u => u.DepartmentId == departmentId);
            return Ok(_mapper.Map<IEnumerable<UserDto>>(departmentUsers));
        }

        // Get tickets for user's department
        [HttpGet("department-tickets")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<TicketDto>>> GetDepartmentTickets()
        {
            // Get current user ID
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(currentUserId, out int userId))
            {
                return Unauthorized("Invalid user ID");
            }

            // Get user's department (via Group in this case)
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null || !user.GroupId.HasValue)
            {
                return BadRequest("User not found or not assigned to a department");
            }

            var tickets = await _ticketRepository.SearchWithIncludesAsync(
                t => t.GroupId == user.GroupId,
                "User", "Group.Department", "Group", "Inventory", "CreatedBy");
            return Ok(_mapper.Map<IEnumerable<TicketDto>>(tickets));
        }

        // Assign ticket to self
        [HttpPost("{id}/assign")]
        [Authorize]
        public async Task<ActionResult<TicketDto>> AssignTicket(int id)
        {
            // Get current user ID
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(currentUserId, out int userId))
            {
                return Unauthorized("Invalid user ID");
            }

            // Get user's group
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null || !user.GroupId.HasValue)
            {
                return BadRequest("User not found or not assigned to a group");
            }

            // Get the ticket (include Department)
            // Get the ticket (include Group and then its Department)
            var ticket = await _context.Tickets
                .Include(t => t.CreatedBy)
                .Include(t => t.Group)
                .ThenInclude(g => g.Department)
                .FirstOrDefaultAsync(t => t.Id == id);
            if (ticket == null)
            {
                return NotFound("Ticket not found");
            }

            // Check if ticket is already assigned
            if (ticket.UserId.HasValue)
            {
                return BadRequest("Ticket is already assigned");
            }

            // Check if ticket belongs to user's department
            if (ticket.GroupId != user.GroupId)
            {
                return BadRequest("Cannot assign ticket from different department");
            }

            // Assign the ticket
            ticket.UserId = userId;
            ticket.Status = "In Progress";
            ticket.AssignedDate = DateTime.Now;
            ticket.IdleDuration = ticket.AssignedDate - ticket.CreatedDate;

            await _context.SaveChangesAsync();
            var ticketDto = _mapper.Map<TicketDto>(ticket);
            ticketDto.IdleDurationDisplay = TimeSpanFormatter.Format(ticket.IdleDuration.Value);
            return Ok(ticketDto);
        }

        [HttpGet("my-tickets")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<TicketDto>>> GetMyTickets()
        {
            // Get current user ID
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(currentUserId, out int userId))
            {
                return Unauthorized("Invalid user ID");
            }

            var tickets = await _ticketRepository.SearchWithIncludesAsync(
                t => t.UserId == userId,
                "User", "Group.Department", "Group", "Inventory", "CreatedBy");
            return Ok(_mapper.Map<IEnumerable<TicketDto>>(tickets));
        }
        
        [HttpPost("upload")]
        [Authorize]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
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
                var uploadPath = Path.Combine("wwwroot", "uploads", "tickets");

                // Create directory if it doesn't exist
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                var filePath = Path.Combine(uploadPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var relativePath = Path.Combine("uploads", "tickets", fileName);
                return Ok(new { filePath = relativePath.Replace("\\", "/") });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        
        [HttpPut("{id}/priority")]
        [Authorize]
        public async Task<ActionResult<TicketDto>> UpdateTicketPriority(int id, [FromBody] TicketPriority priority)
        {
            // Get current user ID
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(currentUserId, out int userId))
            {
                return Unauthorized("Invalid user ID");
            }

            var ticket = await _ticketRepository.GetByIdWithIncludesAsync(
                id, "User", "Group.Department", "CreatedBy", "Group", "Inventory");
            if (ticket == null)
            {
                return NotFound("Ticket not found");
            }

            if (ticket.UserId != userId)
            {
                return Forbid("You can only update priority for tickets assigned to you");
            }

            ticket.Priority = priority;
            await _ticketRepository.UpdateAsync(ticket);
            return Ok(_mapper.Map<TicketDto>(ticket));
        }
        
        [HttpGet("download/{ticketId}")]
        [Authorize]
        public async Task<IActionResult> DownloadFile(int ticketId)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(currentUserId, out int userId))
            {
                return Unauthorized("Invalid user ID");
            }

            var ticket = await _ticketRepository.GetByIdAsync(ticketId);
            if (ticket == null)
                return NotFound("Ticket not found");

            if (string.IsNullOrEmpty(ticket.AttachmentPath))
                return NotFound("No attachment found for this ticket");

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", ticket.AttachmentPath);
            if (!System.IO.File.Exists(filePath))
                return NotFound("Attachment file not found");

            var fileName = Path.GetFileName(ticket.AttachmentPath);
            var mimeType = GetMimeType(Path.GetExtension(fileName));
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return new FileStreamResult(fileStream, mimeType)
            {
                FileDownloadName = fileName
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
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".xls" => "application/vnd.ms-excel",
                ".zip" => "application/zip",
                _ => "application/octet-stream"
            };
        }
    }
}