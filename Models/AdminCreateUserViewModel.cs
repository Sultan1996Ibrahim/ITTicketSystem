using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ITTicketSystem.Models
{
    public class AdminCreateUserViewModel
    {
        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        public UserRole Role { get; set; } = UserRole.User;

        
        public int? DepartmentId { get; set; }

        
        public List<int> ManagedDepartmentIds { get; set; } = new();

        public bool IsActive { get; set; } = true;

        public bool CanManageDeptTickets { get; set; }

    }
}
