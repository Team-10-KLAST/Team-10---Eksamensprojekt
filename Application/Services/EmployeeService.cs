using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

        public Employee? GetEmployeeByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            email = email.Trim();

            try
            {
                var addr = new MailAddress(email);
                // Sikrer at MailAddress har normaliseret formatet
                email = addr.Address;
            }
            catch (Exception)
            {
                // Alt der ikke kan parses som email anses som “ikke fundet”
                return null;
            }

            var employees = GetAllEmployees() ?? Enumerable.Empty<Employee>();
            return employees.FirstOrDefault(e =>
                !string.IsNullOrEmpty(e.Email) &&
                e.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }


        // Validates that the approver email exists and belongs to a manager
        public string ValidateApproverEmail(string email)
        {
            var formatError = ValidateEmailFormat(email);
            if (!string.IsNullOrEmpty(formatError))
                return formatError;

            var employee = GetEmployeeByEmail(email);
            if (employee == null)
                return "No employee found with that email.";

            if (employee.RoleID != 1)
                return "Only managers can approve device assignments.";

            return string.Empty;
        }

        // Validates a single employee email (format + uniqueness)
        public string ValidateEmployeeEmail(string email)
        {
            var formatError = ValidateEmailFormat(email);
            if (!string.IsNullOrEmpty(formatError))
                return formatError;

            var existing = GetAllEmployees()
                .FirstOrDefault(e => e.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

            if (existing != null)
                return "Email already exists in the system.";

            return string.Empty;
        }

        // Basic email format validation for ValidateApproverEmail and ValidateEmployeeEmail
        private string ValidateEmailFormat(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return "Email is required.";

            try
            {
                var addr = new MailAddress(email);
                if (!addr.Address.Equals(email, StringComparison.OrdinalIgnoreCase))
                    return "Invalid email format.";
            }
            catch
            {
                return "Invalid email format.";
            }

            return string.Empty;
        }

        // Validates that the email exists in the system & employee is not terminated
        public string ValidateExistingEmployeeEmail(string email)
        {
            var formatError = ValidateEmailFormat(email);
            if (!string.IsNullOrEmpty(formatError))
                return formatError;

            var existing = GetAllEmployees()
                .FirstOrDefault(e => e.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

            if (existing == null)
                return "No employee found with that email.";

            if (existing.TerminationDate.HasValue)
                return "The employee is terminated.";

            return string.Empty;
        }
    }
}
