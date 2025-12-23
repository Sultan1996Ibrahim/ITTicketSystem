using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ITTicketSystem.Models
{
    public class Ticket
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        // Target Department (the department that will receive the ticket)
        public int DepartmentId { get; set; }
        public Department? Department { get; set; }

        // Sender Department (the department that created/sent the ticket)
        public int? FromDepartmentId { get; set; }
        public Department? FromDepartment { get; set; }

        // Workflow
        public TicketStatus Status { get; set; } = TicketStatus.New;

        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Assign to AppUser
        public int? AssignedUserId { get; set; }
        public AppUser? AssignedUser { get; set; }

        public ICollection<TicketAttachment> Attachments { get; set; } = new List<TicketAttachment>();

        // Manager sets this on approval
        public TicketPriority? Priority { get; set; }

        // Generated after saving
        public string? ReferenceNumber { get; set; }
        public int? CreatedByUserId { get; set; }
        public AppUser? CreatedByUser { get; set; }

    }
}
