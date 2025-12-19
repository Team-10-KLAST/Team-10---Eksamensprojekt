using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Application.Interfaces;
using Application.Interfaces.Repository;
using Application.Models;
using Application.Models.DisplayModels;

namespace Application.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IRepository<Employee> _employeeRepository;
        private readonly IRepository<Department> _departmentRepository;
        private readonly IRepository<Role> _roleRepository;
        private readonly IRepository<Loan> _loanRepository;

        private readonly List<Employee> _employees = new();

        public EmployeeService(IRepository<Employee> employeeRepository, IRepository<Department> departmentRepository, 
            IRepository<Role> roleRepository, IRepository<Loan> loanRepository)
        {
            _employeeRepository = employeeRepository;
            _departmentRepository = departmentRepository;
            _roleRepository = roleRepository;
            _loanRepository = loanRepository;
        }

        //Adds a new employee
        public void AddEmployee(Employee employee)
        {
            if (string.IsNullOrWhiteSpace(employee.FirstName))
            {
                throw new ArgumentException("Firstname field cannot be empty.");
            }
            
            if (string.IsNullOrWhiteSpace(employee.LastName))
            {
                throw new ArgumentException("Lastname field cannot be empty.");
            }
            
            if (string.IsNullOrWhiteSpace(employee.Email))
            {
                throw new ArgumentException("Email field cannot be empty.");
            }
            
            if (employee.DepartmentID <= 0)
            {
                throw new ArgumentException("Department field cannot be empty.");
            }
            
            if (employee.RoleID <= 0)
            {
                throw new ArgumentException("Role field cannot be empty.");
            }
            
            _employeeRepository.Add(employee);
        }

        public void TerminateEmployee(int employeeID, DateOnly terminationDate)
        {
            if (terminationDate < DateOnly.FromDateTime(DateTime.Today))
                throw new ArgumentException("Termination date cannot be earlier than today.");

            var employee = _employeeRepository.GetByID(employeeID);

            if (employee == null)
            {
                throw new ArgumentException($"No employee found with ID {employeeID}");
            }

            employee.TerminationDate = terminationDate;

            _employeeRepository.Update(employee);
        }

        //Gets all employees
        public IEnumerable<Employee> GetAllEmployees()
        {
            return _employeeRepository.GetAll();
        }
                

        // Gets all departments and roles
        public IEnumerable<Department> GetAllDepartments() => _departmentRepository.GetAll();
        public IEnumerable<Role> GetAllRoles() => _roleRepository.GetAll();

        
        // Gets employee display models with full details for EmployeeViewModel
        public IEnumerable<EmployeeDisplayModel> GetEmployeeDisplayModels()
        {
            var allEmployees = _employeeRepository.GetAll();
            var departments = _departmentRepository.GetAll().ToDictionary(department => department.DepartmentID, department => department.Name);
            var roles = _roleRepository.GetAll().ToDictionary(role => role.RoleID, role => role.Name);
            var allLoans = _loanRepository.GetAll();

            var employeeDisplayModels = new List<EmployeeDisplayModel>();

            foreach (var employee in allEmployees)
            {
                var employeeDisplayModel = new EmployeeDisplayModel
                {
                    EmployeeID = employee.EmployeeID,
                    FullName = $"{employee.FirstName} {employee.LastName}",
                    Email = employee.Email,
                    DepartmentName = departments.TryGetValue(employee.DepartmentID, out var departmentName) ? departmentName : "Unknown",
                    RoleName = roles.TryGetValue(employee.RoleID, out var roleName) ? roleName : "Unknown",
                    TerminationDate = employee.TerminationDate,
                    DeviceCount = allLoans.Count(loan => loan.BorrowerID == employee.EmployeeID &&
                        loan.Status == LoanStatus.ACTIVE)
                };

                employeeDisplayModels.Add(employeeDisplayModel);
            }

            return employeeDisplayModels;
        }

        public IEnumerable<string> GetAllEmployeeEmails()
        {            
            var employees = GetAllEmployees();
            return employees
                   .Select(e => e.Email)
                   .ToList();
        }

        public Employee GetEmployeeByEmail(string email)
        {            
            try
            {
                var addr = new MailAddress(email);
            }
            catch (FormatException)
            {
                throw new ArgumentException("Invalid email");
            }
            var employees = GetAllEmployees();
            return employees.FirstOrDefault(e => e.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }

        // Validates that the approver email exists and belongs to a manager
        public string ValidateApproverEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return "Approver email is required.";

            try
            {
                var addr = new MailAddress(email);
            }
            catch
            {
                return "Invalid email format.";
            }

            var employee = GetEmployeeByEmail(email);
            if (employee == null)
                return "No employee found with that email.";

            if (employee.RoleID != 1)
                return "Only managers can approve device assignments.";

            return string.Empty;
        }
    }
}
