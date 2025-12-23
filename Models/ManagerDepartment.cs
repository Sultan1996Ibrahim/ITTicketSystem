namespace ITTicketSystem.Models
{
    public class ManagerDepartment
    {
        public int ManagerUserId { get; set; }
        public AppUser? ManagerUser { get; set; }

        public int DepartmentId { get; set; }
        public Department? Department { get; set; }
    }
}
