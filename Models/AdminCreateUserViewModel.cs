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

        // User فقط
        public int? DepartmentId { get; set; }

        // Manager فقط (عدة إدارات)
        public List<int> ManagedDepartmentIds { get; set; } = new();

        public bool IsActive { get; set; } = true;

        public bool CanManageDeptTickets { get; set; }

    }
}
