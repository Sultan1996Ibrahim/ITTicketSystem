using System;

namespace ITTicketSystem.Models
{
    public class TicketHistory
    {
        public int Id { get; set; }

        public int TicketId { get; set; }
        public Ticket? Ticket { get; set; }

        public TicketStatus OldStatus { get; set; }
        public TicketStatus NewStatus { get; set; }

        public string? ChangedBy { get; set; }
        public string? Role { get; set; }

        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        public string? Comment { get; set; }
    }
}
