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

namespace Presentation.Wpf.ViewModels
{
    public class AddEmployeeViewModel : ViewModelBase
    {
        //form fields

        private string _firstName = string.Empty;
        public string FirstName
        {
            get => _firstName;
            set => SetProperty(ref _firstName, value);
        }

        private string _lastName = string.Empty;
        public string LastName
        {
            get => _lastName;
            set => SetProperty(ref _lastName, value);
        }

        private string _email = string.Empty;
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        private Department _selectedDepartment;
        public Department SelectedDepartment
        {
            get => _selectedDepartment;
            set => SetProperty(ref _selectedDepartment, value);
        }

        private Role _selectedRole;
        public Role SelectedRole
        {
            get => _selectedRole;
            set => SetProperty(ref _selectedRole, value);
        }

        //Collections for Departments and roles

        public ObservableCollection<Department> Departments { get; }
        public ObservableCollection<Role> Roles { get; }

        //Commands

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand {get;}

        //Constructor

        public AddEmployeeViewModel()
        {
            // Insert values into the Collections of Department and Role
            Departments = new ObservableCollection<Department>(
                (Department[])Enum.GetValues(typeof(Department))
            );

            Roles = new ObservableCollection<Role>(
                (Role[])Enum.GetValues(typeof(Role))
            );

            // Default department and role
            SelectedDepartment = Departments.Count > 0 ? Departments[0] : default;
            SelectedRole = Roles.Count > 0 ? Roles[0] : default;

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
                Department = this.SelectedDepartment,
                Role = this.SelectedRole
            };            

            //trigger a save to repository

            //Clear form after saving
            ClearFields();
        }

        private void ClearFields()
        {
            FirstName = LastName = Email = string.Empty;
            SelectedDepartment = Departments[0];
            SelectedRole = Roles[0];
        }

        private bool CanSaveEmployee()
        {
            // Basic validation
            return !string.IsNullOrWhiteSpace(FirstName)
                && !string.IsNullOrWhiteSpace(LastName)
                && !string.IsNullOrWhiteSpace(Email);
        }

        private void Cancel()
        {
            ClearFields();
            //RequestClose?.Invoke(this, EventArgs.Empty);  Not yet implemented
        }
    }
}
