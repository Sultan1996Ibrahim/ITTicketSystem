using System.Collections.Generic;

namespace ITTicketSystem.Models
{
    public class UserDashboardViewModel
    {
        
        public int TotalCreated { get; set; }
        public int TotalAssignedToMe { get; set; }
        public int NewCount { get; set; }
        public int InProgressCount { get; set; }
        public int ResolvedCount { get; set; }
        public int ClosedCount { get; set; }

       
        public int TotalTickets { get; set; }

        
        public TicketStatus? CurrentFilter { get; set; }

        
        public List<Ticket> Tickets { get; set; } = new();
    }
}
