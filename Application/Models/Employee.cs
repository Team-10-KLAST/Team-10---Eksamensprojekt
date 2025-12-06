using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models
{
    public class Employee
    {
        public int EmployeeID { get; set; }     
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public DateOnly? TerminationDate { get; set; }
        public int DepartmentID { get; set; }
        public int RoleID { get; set; }
        
        // Parameterless constructor. Needed for ADO.NET.
        public Employee() { }

        // Constructor with parameters (excluding EmployeeID).
        public Employee(string firstName, string lastName, string email,
                        int departmentID, int roleID)
        {
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            DepartmentID = departmentID;
            RoleID = roleID;
        }
    }
}
