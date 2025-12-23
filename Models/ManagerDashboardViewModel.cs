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

        // عدد التذاكر الجديدة
        public int NewTickets { get; set; }

        // التذاكر المسندة أو قيد التنفيذ
        public int AssignedOrInProgressTickets { get; set; }

        // التذاكر المغلقة
        public int ClosedTickets { get; set; }

        public List<DepartmentTicketCount> TicketsByDepartment { get; set; } = new();
        public List<Ticket> RecentTickets { get; set; } = new();
    }
}
