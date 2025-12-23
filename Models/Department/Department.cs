namespace ITTicketSystem.Models
{
    public class Department
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // Parent department (null means a top-level department)
        public int? ParentDepartmentId { get; set; }
        public Department? ParentDepartment { get; set; }

        // Child departments
        public List<Department> SubDepartments { get; set; } = new();

        public ICollection<ManagerDepartment> Managers { get; set; } = new List<ManagerDepartment>();
    }
}
