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
using Data;
using Microsoft.Data.SqlClient;
using System.Net.Mail;

namespace Presentation.Wpf.ViewModels
{
    public class AddEmployeeViewModel : OverlayPanelViewModelBase
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

        private string _selectedDepartment;
        public string SelectedDepartment
        {
            get => _selectedDepartment;
            set => SetProperty(ref _selectedDepartment, value);
        }

        private string _selectedRole;
        public string SelectedRole
        {
            get => _selectedRole;
            set => SetProperty(ref _selectedRole, value);
        }

        //Collections for Departments and roles

        public ObservableCollection<string> Departments { get; }
        public ObservableCollection<string> Roles { get; }

        //Commands

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        //Constructor

        public AddEmployeeViewModel()
        {
            // Insert values into the Collections of Department and Role
            Departments = new ObservableCollection<string>();
            LoadDepartments();


            Roles = new ObservableCollection<string>();
            LoadRoles();

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
                DepartmentId = GetIDFromName(Departments, this.SelectedDepartment),
                RoleId = GetIDFromName(Roles, this.SelectedRole)
            };

            //trigger a save to repository

            //Clear form after saving
            ClearFields();
            CloseOverlay();
        }

        private void ClearFields()
        {
            FirstName = LastName = Email = string.Empty;
            SelectedDepartment = Departments[0];
            SelectedRole = Roles[0];
        }

        private bool CanSaveEmployee()
        {
            if (string.IsNullOrWhiteSpace(FirstName) ||
                string.IsNullOrWhiteSpace(LastName) ||
                string.IsNullOrWhiteSpace(Email))
            {
                return false;
            }

            // Validate email format
            try
            {
                var addr = new MailAddress(Email);
                return addr.Address == Email;
            }
            catch
            {
                return false;
            }
        }

        private void Cancel()
        {
            ClearFields();
            CloseOverlay();
        }

        private void LoadDepartments()
        {
            var db = DatabaseConnection.GetInstance();
            using (SqlConnection conn = db.CreateConnection())
            {
                conn.Open();
                string query = "SELECT DepartmentName FROM Department ORDER BY DepartmentID";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Departments.Add(reader.GetString(0));
                    }
                }
            }
        }
        private void LoadRoles()
        {
            var db = DatabaseConnection.GetInstance();
            using (SqlConnection conn = db.CreateConnection())
            {
                conn.Open();
                string query = "SELECT RoleName FROM Role ORDER BY RoleID";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Roles.Add(reader.GetString(0));
                    }
                }
            }
        }
        private int GetIDFromName(ObservableCollection<string> collection, string name)
        {
            int ID = collection.IndexOf(name);

            // Because DepartmentID starts at 1, not 0
            return ID >= 0 ? ID + 1 : -1;
        }
    }
}
