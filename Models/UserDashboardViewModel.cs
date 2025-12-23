using System.Collections.Generic;

namespace ITTicketSystem.Models
{
    public class UserDashboardViewModel
    {
        // القيم القديمة (ما زالت مستخدمة في الـ Controller)
        public int TotalCreated { get; set; }
        public int TotalAssignedToMe { get; set; }
        public int NewCount { get; set; }
        public int InProgressCount { get; set; }
        public int ResolvedCount { get; set; }
        public int ClosedCount { get; set; }

        // للـ UI الجديد
        public int TotalTickets { get; set; }

        // الفلتر الحالي في الـ Dashboard
        public TicketStatus? CurrentFilter { get; set; }

        // قائمة التذاكر المعروضة تحت الجدول
        public List<Ticket> Tickets { get; set; } = new();
    }
}
