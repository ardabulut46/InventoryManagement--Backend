using AutoMapper;
using InventoryManagement.Core.DTOs.TicketSolution;
using InventoryManagement.Core.Entities;
using InventoryManagement.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using InventoryManagement.Core.DTOs;
using InventoryManagement.Core.DTOs.SolutionReview;
using InventoryManagement.Core.DTOs.User;
using InventoryManagement.Core.Enums;
using InventoryManagement.Core.Helpers;
using InventoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketSolutionsController : ControllerBase
    {
        private readonly IGenericRepository<TicketSolution> _ticketSolutionRepository;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;
        private readonly IGenericRepository<SolutionReview> _solutionReviewRepository;
        private readonly ILogger<TicketSolutionsController> _logger;

        public TicketSolutionsController(
            IGenericRepository<TicketSolution> ticketSolutionRepository,
            IMapper mapper,
            ApplicationDbContext context,
            IGenericRepository<SolutionReview> solutionReviewRepository,
            ILogger<TicketSolutionsController> logger)
        {
            _ticketSolutionRepository = ticketSolutionRepository;
            _mapper = mapper;
            _context = context;
            _solutionReviewRepository = solutionReviewRepository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TicketSolutionDto>>> GetTicketSolutions()
        {
            var solutions = await _ticketSolutionRepository.GetAllWithIncludesAsync(
                "Ticket",
                "User",
                "AssignedUser",
                "SolutionType");

            return Ok(_mapper.Map<IEnumerable<TicketSolutionDto>>(solutions));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TicketSolutionDto>> GetTicketSolution(int id)
        {
            var solution = await _ticketSolutionRepository.GetByIdWithIncludesAsync(
                id,
                "Ticket",
                "User",
                "AssignedUser",
                "SolutionType");

            if (solution == null)
                return NotFound();

            return Ok(_mapper.Map<TicketSolutionDto>(solution));
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<TicketSolutionDto>> CreateTicketSolution(
            TicketSolutionCreateUpdateDto createUpdateDto)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int currentUserId))
            {
                return BadRequest("Invalid user ID");
            }

            var ticket = await _context.Tickets
                .Include(t => t.Group)
                .FirstOrDefaultAsync(x => x.Id == createUpdateDto.TicketId);
            if (ticket == null)
            {
                return NotFound("Ticket not found");
            }

            if (ticket.UserId != currentUserId)
            {
                return BadRequest("You can only create solutions for tickets assigned to you");
            }

            var existingSolutions =
                await _ticketSolutionRepository.SearchAsync(s => s.TicketId == createUpdateDto.TicketId);
            if (existingSolutions.Any())
            {
                return BadRequest("Çağrı zaten çözümlendi.");
            }

            // Validate problem type and solution time before creating the solution
            var problemType = await _context.ProblemTypes
                .FirstOrDefaultAsync(pt => pt.Id == ticket.ProblemTypeId && pt.GroupId == ticket.GroupId);
            if (problemType == null)
            {
                return BadRequest("Problem type not found");
            }

            var solutionTime = await _context.SolutionTimes
                .FirstOrDefaultAsync(st => st.ProblemTypeId == problemType.Id);
            if (solutionTime == null)
            {
                return BadRequest("Solution time not configured for the problem type");
            }

            if (!ticket.AssignedDate.HasValue)
            {
                return BadRequest("Ticket does not have an assigned date");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var solution = _mapper.Map<TicketSolution>(createUpdateDto);
                solution.UserId = currentUserId;
                solution.AssignedUserId = currentUserId;
                solution.SolutionDate = DateTime.Now;

                _context.TicketSolutions.Add(solution);
                ticket.Status = TicketStatus.Resolved;
                await _context.SaveChangesAsync();

                var expectedSolutionTime = ticket.AssignedDate.Value.Add(solutionTime.TimeToSolve);
                bool isInTime = solution.SolutionDate <= expectedSolutionTime;
                TimeSpan? howMuchLate = isInTime ? null : solution.SolutionDate - expectedSolutionTime;

                string notes;
                if (!isInTime)
                {
                    notes = $"Geç kalma süresi: {TimeSpanFormatter.Format(howMuchLate.Value)}";
                }
                else
                {
                    notes = solution.IsChronicle == true
                        ? "Kronik çağrı işaretlendi ve zamanında çözümlendi."
                        : "Zamanında çözümlendi.";
                }

                var solutionReview = new SolutionReview
                {
                    TicketSolutionId = solution.Id,
                    IsInTime = isInTime,
                    Notes = notes,
                    HowMuchLate = howMuchLate
                };

                _context.SolutionReviews.Add(solutionReview);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return CreatedAtAction(
                    nameof(GetTicketSolution),
                    new { id = solution.Id },
                    _mapper.Map<TicketSolutionDto>(solution));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTicketSolution(int id, TicketSolutionCreateUpdateDto createUpdateDto)
        {
            var solution = await _ticketSolutionRepository.GetByIdAsync(id);
            if (solution == null)
                return NotFound();

            _mapper.Map(createUpdateDto, solution);
            await _ticketSolutionRepository.UpdateAsync(solution);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTicketSolution(int id)
        {
            var solution = await _ticketSolutionRepository.GetByIdAsync(id);
            if (solution == null)
                return NotFound();

            await _ticketSolutionRepository.DeleteAsync(id);
            return NoContent();
        }

        [HttpGet("by-ticket/{ticketId}")]
        public async Task<ActionResult<IEnumerable<TicketSolutionDto>>> GetSolutionsByTicket(int ticketId)
        {
            var solutions = await _ticketSolutionRepository.SearchWithIncludesAsync(
                s => s.TicketId == ticketId,
                "Ticket",
                "User",
                "AssignedUser",
                "SolutionType");

            return Ok(_mapper.Map<IEnumerable<TicketSolutionDto>>(solutions));
        }

        [HttpGet("by-assigned-user")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<TicketSolutionDto>>> GetSolutionsByAssignedUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int currentUserId))
            {
                return BadRequest("Invalid user ID");
            }

            var solutions = await _ticketSolutionRepository.SearchWithIncludesAsync(
                s => s.AssignedUserId == currentUserId,
                "Ticket",
                "User",
                "AssignedUser",
                "SolutionType");

            return Ok(_mapper.Map<IEnumerable<TicketSolutionDto>>(solutions));
        }
        

       /* [HttpGet("download/{solutionId}")]
        [Authorize]
        public async Task<IActionResult> DownloadSolutionFile(int solutionId)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(currentUserId, out int userId))
            {
                return Unauthorized("Invalid user ID");
            }

            var solution = await _ticketSolutionRepository.GetByIdAsync(solutionId);
            if (solution == null)
                return NotFound("Solution not found");

            if (string.IsNullOrEmpty(solution.AttachmentPath))
                return NotFound("No attachment found for this solution");

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", solution.AttachmentPath);
            if (!System.IO.File.Exists(filePath))
                return NotFound("Attachment file not found");

            var fileName = Path.GetFileName(solution.AttachmentPath);
            var mimeType = GetMimeType(Path.GetExtension(fileName));
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return new FileStreamResult(fileStream, mimeType)
            {
                FileDownloadName = fileName
            };
        }*/

        [HttpPost("{solutionId}/attachments")]
        [Authorize]
        public async Task<IActionResult> UploadFiles(int solutionId, [FromForm] List<IFormFile> files)
        {
            if (files == null || !files.Any())
                return BadRequest("No files were provided");

            var solution = await _ticketSolutionRepository.GetByIdAsync(solutionId);
            if (solution == null)
                return NotFound("Solution not found");

            // Define allowed file types
            var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".txt", ".jpg", ".jpeg", ".png" };
            var uploadedFiles = new List<TicketSolutionAttachmentDto>();

            foreach (var file in files)
            {
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(fileExtension))
                    continue; // Skip invalid file types

                try
                {
                    // Create unique filename
                    var fileName = $"{Guid.NewGuid()}{fileExtension}";
                    var uploadPath = Path.Combine("wwwroot", "uploads", "solutions", solutionId.ToString());

                    // Create directory if it doesn't exist
                    if (!Directory.Exists(uploadPath))
                        Directory.CreateDirectory(uploadPath);

                    var filePath = Path.Combine(uploadPath, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    var relativePath = Path.Combine("uploads", "solutions", solutionId.ToString(), fileName);
                    var attachment = new TicketSolutionAttachment
                    {
                        TicketSolutionId = solutionId,
                        FileName = file.FileName,
                        FilePath = relativePath.Replace("\\", "/"),
                        ContentType = file.ContentType,
                        FileSize = file.Length
                    };

                    await _context.TicketSolutionAttachments.AddAsync(attachment);
                    uploadedFiles.Add(_mapper.Map<TicketSolutionAttachmentDto>(attachment));
                }
                catch (Exception ex)
                {
                    // Log the error but continue with other files
                    _logger.LogError($"Error uploading file {file.FileName}: {ex.Message}");
                }
            }

            await _context.SaveChangesAsync();
            return Ok(uploadedFiles);
        }

        [HttpGet("{solutionId}/attachments")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<TicketSolutionAttachmentDto>>> GetAttachments(int solutionId)
        {
            var attachments = await _context.TicketSolutionAttachments
                .Where(a => a.TicketSolutionId == solutionId)
                .ToListAsync();

            return Ok(_mapper.Map<IEnumerable<TicketSolutionAttachmentDto>>(attachments));
        }

        [HttpGet("attachments/{attachmentId}")]
        [Authorize]
        public async Task<IActionResult> DownloadFile(int attachmentId)
        {
            var attachment = await _context.TicketSolutionAttachments.FindAsync(attachmentId);
            if (attachment == null)
                return NotFound("Attachment not found");

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", attachment.FilePath);
            if (!System.IO.File.Exists(filePath))
                return NotFound("File not found");

            var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return new FileStreamResult(stream, attachment.ContentType)
            {
                FileDownloadName = attachment.FileName
            };
        }

        [HttpDelete("attachments/{attachmentId}")]
        [Authorize]
        public async Task<IActionResult> DeleteAttachment(int attachmentId)
        {
            var attachment = await _context.TicketSolutionAttachments.FindAsync(attachmentId);
            if (attachment == null)
                return NotFound();

            // Delete physical file
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", attachment.FilePath);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            _context.TicketSolutionAttachments.Remove(attachment);
            await _context.SaveChangesAsync();

            return NoContent();
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