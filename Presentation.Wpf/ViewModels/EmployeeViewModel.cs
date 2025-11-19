using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Presentation.Wpf.Commands;
using System.Collections.ObjectModel;
using Application.Interfaces;
using Application.Models;

namespace Presentation.Wpf.ViewModels
{
    public class EmployeeViewModel : OverlayHostViewModel
    {
        private readonly IEmployeeService _employeeService;
        // Den ene skal nok laves til en CollectionView for bedre performance ved filtrering
        public ObservableCollection<EmployeeDisplayModel> Employees { get; set; } = new();
        public ObservableCollection<EmployeeDisplayModel> AllEmployees { get; } = new();
        public ObservableCollection<string> Departments { get; set; } = new();

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                    FilterEmployees();
            }
        }

        private string _selectedDepartment = "All";
        public string SelectedDepartment
        {
            get => _selectedDepartment;
            set
            {
                if (SetProperty(ref _selectedDepartment, value))
                    FilterEmployees();
            }
        }


        public ICommand AddEmployeeCommand { get; }
        public ICommand DeleteEmployeeCommand { get; }


        public EmployeeViewModel(IEmployeeService employeeService)
        {
            _employeeService = employeeService;

            AddEmployeeCommand = new RelayCommand(OpenAddEmployeeOverlay);
            DeleteEmployeeCommand = new RelayCommand<EmployeeDisplayModel>(OpenDeleteEmployeeOverlay);

            LoadEmployees();
        }

        // skal lægge noget af logikken i service laget
        private void LoadEmployees()
        {
            var displayModels = _employeeService.GetDisplayModels();

            AllEmployees.Clear();
            Employees.Clear();
            foreach (var e in displayModels)
            {
                AllEmployees.Add(e);
                Employees.Add(e);
            }

            var departmentNames = _employeeService.GetAllDepartments()
                .Select(d => d.Name)
                .Distinct()
                .OrderBy(n => n);

            Departments.Clear();
            Departments.Add("All");
            foreach (var name in departmentNames)
            {
                Departments.Add(name);
            }

            SelectedDepartment = "All";
        }



        //Skal lægge datafiltrering i EmployeeService?
        private void FilterEmployees()
        {
            var filtered = AllEmployees.Where(e =>
                (string.IsNullOrWhiteSpace(SearchText) || e.FullName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) &&
                (SelectedDepartment == "All" || e.DepartmentName == SelectedDepartment));

            Employees.Clear();
            foreach (var e in filtered)
            {
                Employees.Add(e);
            }
        }


        private void OpenAddEmployeeOverlay()
        {
            var vm = new AddEmployeeViewModel(_employeeService);
            vm.RequestClose += (s, e) =>
            {
                CurrentOverlay = null;
                LoadEmployees();
            };
            ShowOverlay(vm);
        }

        private void OpenDeleteEmployeeOverlay(EmployeeDisplayModel displayModel)
        {
            var employee = _employeeService.GetEmployeeById(displayModel.EmployeeId);
            var vm = new DeleteEmployeeViewModel(employee, 0, _employeeService);
            vm.RequestClose += (s, e) =>
            {
                CurrentOverlay = null;
                LoadEmployees();
            };
            ShowOverlay(vm);
        }
    }
}

