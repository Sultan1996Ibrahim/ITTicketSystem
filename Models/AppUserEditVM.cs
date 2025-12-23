using System.Collections.Generic;

namespace ITTicketSystem.Models
{
    public class AppUserEditVM
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty; // عرض فقط

        public UserRole Role { get; set; }

        // User فقط
        public int? DepartmentId { get; set; }

        // Manager فقط
        public List<int> ManagedDepartmentIds { get; set; } = new();

        public bool IsActive { get; set; } = true;

        public bool CanManageDeptTickets { get; set; }

    }
}
