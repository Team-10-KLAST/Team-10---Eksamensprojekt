namespace Application.Models
{
    public enum Department
    {
        Engineering,
        Sales,
        Accounting,
        Administration
    }

    public enum Role
    {
        Manager,
        Employee
    }

    public class Employee
    {
        public int EmployeeId { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public Department Department { get; set; }

        public Role Role { get; set; }
    }
}
