using System;
using System.ComponentModel.DataAnnotations;

namespace ITTicketSystem.Models
{
    public class TicketAttachment
    {
        public int Id { get; set; }

        // FK
        public int TicketId { get; set; }
        public Ticket Ticket { get; set; } = null!;

        [Required]
        [StringLength(255)]
        public string FileName { get; set; } = string.Empty;

        
        [Required]
        public string FilePath { get; set; } = string.Empty;

        public string? ContentType { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}
