using System.ComponentModel.DataAnnotations;

namespace ITTicketSystem.Models
{
    public class AppUser
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public UserRole Role { get; set; } = UserRole.User;

        // Optional: user belongs to a department
        public int? DepartmentId { get; set; }
        public Department? Department { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<ManagerDepartment> ManagedDepartments { get; set; } = new List<ManagerDepartment>();

        public bool CanManageDeptTickets { get; set; } = false;
    }
}
