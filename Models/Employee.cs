namespace ITTicketSystem.Models
{
    public class Employee
    {
        public int Id { get; set; }

        // Display name used in assignment
        public string Name { get; set; } = string.Empty;

        // Department name this employee belongs to (should match Department.Name)
        public string DepartmentName { get; set; } = string.Empty;

        // If false, the employee is archived and should not receive new tickets
        public bool IsActive { get; set; } = true;
    }
}
