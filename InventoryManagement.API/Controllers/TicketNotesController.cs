using System.Security.Claims;
using AutoMapper;
using InventoryManagement.Core.DTOs.TicketNote;
using InventoryManagement.Core.Entities;
using InventoryManagement.Core.Interfaces;
using InventoryManagement.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.API.Controllers
{
    [ApiController]
    [Route("api/tickets/{ticketId}/notes")]
    [Authorize]
    public class TicketNotesController : ControllerBase
    {
        private readonly IGenericRepository<TicketNote> _noteRepository;
        private readonly IGenericRepository<Ticket> _ticketRepository;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TicketNotesController> _logger;

        public TicketNotesController(
            IGenericRepository<TicketNote> noteRepository,
            IGenericRepository<Ticket> ticketRepository,
            IMapper mapper,
            ApplicationDbContext context,
            ILogger<TicketNotesController> logger)
        {
            _noteRepository = noteRepository;
            _ticketRepository = ticketRepository;
            _mapper = mapper;
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TicketNoteDto>>> GetTicketNotes(int ticketId)
        {
            var notes = await _noteRepository.SearchWithIncludesAsync(
                n => n.TicketId == ticketId,
                "Ticket",
                "CreatedBy",
                "Attachments");

            return Ok(_mapper.Map<IEnumerable<TicketNoteDto>>(notes));
        }

        [HttpGet("{noteId}")]
        public async Task<ActionResult<TicketNoteDto>> GetNoteById(int ticketId, int noteId)
        {
            var note = await _noteRepository.GetByIdWithIncludesAsync(
                noteId,
                "Ticket",
                "CreatedBy",
                "Attachments");

            if (note == null || note.TicketId != ticketId)
                return NotFound("Note not found");

            return Ok(_mapper.Map<TicketNoteDto>(note));
        }

        [HttpPost]
        public async Task<ActionResult<TicketNoteDto>> AddNote(int ticketId, [FromForm] CreateTicketNoteDto createDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userId, out int currentUserId))
            {
                return Unauthorized("Invalid user ID");
            }

            var ticket = await _ticketRepository.GetByIdAsync(ticketId);
            if (ticket == null)
            {
                return NotFound("Ticket not found");
            }

            if (ticket.CreatedById != currentUserId && ticket.UserId != currentUserId)
            {
                return Forbid("You don't have permission to add notes to this ticket");
            }

            var note = new TicketNote
            {
                TicketId = ticketId,
                CreatedById = currentUserId,
                Note = createDto.Note,
                NoteType = createDto.NoteType,
                Attachments = new List<TicketNoteAttachment>()
            };

            var created = await _noteRepository.AddAsync(note);

            if (createDto.Files != null && createDto.Files.Any())
            {
                foreach (var file in createDto.Files)
                {
                    var attachmentPath = await SaveFile(file, ticketId, created.Id);
                    if (attachmentPath != null)
                    {
                        var attachment = new TicketNoteAttachment
                        {
                            TicketNoteId = created.Id,
                            FileName = file.FileName,
                            FilePath = attachmentPath,
                            ContentType = file.ContentType,
                            FileSize = file.Length,
                            UploadDate = DateTime.Now
                        };
                        created.Attachments.Add(attachment);
                    }
                }
                await _noteRepository.UpdateAsync(created);
            }

            var noteWithIncludes = await _noteRepository.GetByIdWithIncludesAsync(
                created.Id,
                "Ticket",
                "CreatedBy",
                "Attachments");

            return CreatedAtAction(
                nameof(GetNoteById),
                new { ticketId, noteId = created.Id },
                _mapper.Map<TicketNoteDto>(noteWithIncludes));
        }

        [HttpGet("{noteId}/attachments/{attachmentId}/download")]
        public async Task<IActionResult> DownloadAttachment(int ticketId, int noteId, int attachmentId)
        {
            var attachment = await _context.TicketNoteAttachments
                .FirstOrDefaultAsync(a => a.Id == attachmentId && a.TicketNoteId == noteId);
            if (attachment == null)
                return NotFound("Attachment not found");

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", attachment.FilePath);
            if (!System.IO.File.Exists(filePath))
                return NotFound("File not found");

            var mimeType = GetMimeType(Path.GetExtension(attachment.FilePath));
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            return new FileStreamResult(fileStream, mimeType)
            {
                FileDownloadName = attachment.FileName
            };
        }

        [HttpDelete("{noteId}/attachments/{attachmentId}")]
        public async Task<IActionResult> DeleteAttachment(int ticketId, int noteId, int attachmentId)
        {
            var attachment = await _context.TicketNoteAttachments
                .FirstOrDefaultAsync(a => a.Id == attachmentId && a.TicketNoteId == noteId);
            if (attachment == null)
                return NotFound("Attachment not found");

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", attachment.FilePath);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            _context.TicketNoteAttachments.Remove(attachment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<string?> SaveFile(IFormFile file, int ticketId, int noteId)
        {
            var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".txt", ".jpg", ".jpeg", ".png" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(fileExtension))
                return null;

            try
            {
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var uploadPath = Path.Combine("wwwroot", "uploads", "notes", ticketId.ToString(), noteId.ToString());

                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                var filePath = Path.Combine(uploadPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return Path.Combine("uploads", "notes", ticketId.ToString(), noteId.ToString(), fileName)
                    .Replace("\\", "/");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error saving file: {ex.Message}");
                return null;
            }
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
                _ => "application/octet-stream"
            };
        }
    }
}