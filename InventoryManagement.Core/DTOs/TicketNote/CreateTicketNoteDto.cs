using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace InventoryManagement.Core.DTOs.TicketNote;

public class CreateTicketNoteDto
{
    [Required]
    public string Note { get; set; }
    public string? NoteType { get; set; }
    public List<IFormFile> Files { get; set; }
}