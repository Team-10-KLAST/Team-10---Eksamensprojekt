using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models
{
    public class Employee
    {
        public int EmployeeId { get; set; }          // PK in Employee table
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";

        // FK values that point to Department and Role tables.
        public int DepartmentId { get; set; }
        public int RoleId { get; set; }

        // Parameterless constructor. Needed for ADO.NET.
        public Employee() { }

        // Constructor with parameters (excluding EmployeeId).
        public Employee(string firstName, string lastName, string email,
                        int departmentId, int roleId)
        {
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            DepartmentId = departmentId;
            RoleId = roleId;
        }
    }
}
