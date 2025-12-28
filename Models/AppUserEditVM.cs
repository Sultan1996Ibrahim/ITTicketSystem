using System.Collections.Generic;

namespace ITTicketSystem.Models
{
    public class AppUserEditVM
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty; 

        public UserRole Role { get; set; }

        
        public int? DepartmentId { get; set; }

        
        public List<int> ManagedDepartmentIds { get; set; } = new();

        public bool IsActive { get; set; } = true;

        public bool CanManageDeptTickets { get; set; }

    }
}
