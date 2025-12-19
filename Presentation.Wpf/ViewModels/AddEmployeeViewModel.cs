using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Presentation.Wpf.Commands;
using System.Windows.Input;
using Application.Models;
using System.Net.Mail;
using Application.Services;
using Application.Interfaces;

namespace Presentation.Wpf.ViewModels
{
    public class AddEmployeeViewModel : OverlayPanelViewModelBase
    {
        private readonly IEmployeeService _employeeService;

        //form fields

        private string _firstName = string.Empty;
        public string FirstName
        {
            get => _firstName;
            set
            {
                if (SetProperty(ref _firstName, value))
                {
                    (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        private string _lastName = string.Empty;
        public string LastName
        {
            get => _lastName;
            set
            {
                if (SetProperty(ref _lastName, value))
                {
                    (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        private string _email = string.Empty;
        public string Email
        {
            get => _email;
            set
            {
                if (SetProperty(ref _email, value))
                {
                    EmailErrorMsg = _employeeService.ValidateEmployeeEmail(value);
                    (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        private string _emailErrorMsg = string.Empty;
        public string EmailErrorMsg
        {
            get => _emailErrorMsg;
            set => SetProperty(ref _emailErrorMsg, value);
        }

        private Department? _selectedDepartment;
        public Department? SelectedDepartment
        {
            get => _selectedDepartment;
            set => SetProperty(ref _selectedDepartment, value);
        }

        private Role? _selectedRole;
        public Role? SelectedRole
        {
            get => _selectedRole;
            set => SetProperty(ref _selectedRole, value);
        }

        //Collections for Departments and roles

        public ObservableCollection<Department> Departments { get; }
        public ObservableCollection<Role> Roles { get; }

        //Commands

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        //Constructor
        public AddEmployeeViewModel(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
            // Insert values into the Collections of Department and Role
            Departments = new ObservableCollection<Department>(_employeeService.GetAllDepartments());
            Roles = new ObservableCollection<Role>(_employeeService.GetAllRoles());

            // Default department and role
            SelectedDepartment = Departments.FirstOrDefault();
            SelectedRole = Roles.FirstOrDefault();

            // Initialize command
            SaveCommand = new RelayCommand(AddEmployee, CanSaveEmployee);
            CancelCommand = new RelayCommand(Cancel);
        }

        //Save method
        private void AddEmployee()
        {
            // Create new Employee object from form fields
            Employee newEmployee = new Employee
            {
                FirstName = this.FirstName,
                LastName = this.LastName,
                Email = this.Email,
                DepartmentID = SelectedDepartment?.DepartmentID ?? 0,
                RoleID = SelectedRole?.RoleID ?? 0                
            };
            
            try
            {
                _employeeService.AddEmployee(newEmployee);
                ClearFields();
                CloseOverlay();
            }
            catch (Exception ex)
            {
                EmailErrorMsg = "Error trying to add employee: " + ex.Message;
            }
        }

        private void ClearFields()
        {
            FirstName = LastName = Email = EmailErrorMsg = string.Empty;
            SelectedDepartment = Departments.FirstOrDefault();
            SelectedRole = Roles.FirstOrDefault();
        }

        // CanExecute method for SaveCommand
        private bool CanSaveEmployee() =>
            !string.IsNullOrWhiteSpace(FirstName) &&
            !string.IsNullOrWhiteSpace(LastName) &&
            !string.IsNullOrWhiteSpace(Email) &&
            string.IsNullOrEmpty(EmailErrorMsg);


        private void Cancel()
        {
            ClearFields();
            CloseOverlay();
        }        
    }
}
