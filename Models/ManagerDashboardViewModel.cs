using System.Collections.Generic;

namespace ITTicketSystem.Models
{
    public class DepartmentTicketCount
    {
        public string DepartmentName { get; set; } = string.Empty;
        public int TicketCount { get; set; }
    }

    public class ManagerDashboardViewModel
    {
        public int TotalTickets { get; set; }

        
        public int NewTickets { get; set; }

        
        public int AssignedOrInProgressTickets { get; set; }

        
        public int ClosedTickets { get; set; }

        public List<DepartmentTicketCount> TicketsByDepartment { get; set; } = new();
        public List<Ticket> RecentTickets { get; set; } = new();
    }
}
