using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using Application.Interfaces;
using Application.Models;
using Application.Models.DisplayModels;
using Presentation.Wpf.Commands;

namespace Presentation.Wpf.ViewModels
{
    public class EmployeeViewModel : OverlayHostViewModel
    {
        private readonly IEmployeeService _employeeService;

        // Collection of all employees (unfiltered)
        public ObservableCollection<EmployeeDisplayModel> AllEmployees { get; } = new();

        // Collectionview for filtered employees
        public ICollectionView EmployeesView { get; }

        // Departments for filtering
        public ObservableCollection<string> Departments { get; } = new();

        // Search text used for filtering employees by name
        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                    EmployeesView.Refresh();
            }
        }

        // Selected department for filtering employees - "All departments" means no filtering
        private const string AllDepartments = "All departments";
        private string _selectedDepartment = AllDepartments;
        public string SelectedDepartment
        {
            get => _selectedDepartment;
            set
            {
                if (SetProperty(ref _selectedDepartment, value))
                    EmployeesView.Refresh();
            }
        }

        // Error message property for displaying load errors
        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        // Commands
        public ICommand AddEmployeeCommand { get; }
        public ICommand DeleteEmployeeCommand { get; }
        public ICommand TerminateEmployeeCommand { get; }

        // Constructor
        public EmployeeViewModel(IEmployeeService employeeService)
        {
            _employeeService = employeeService;

            AddEmployeeCommand = new RelayCommand(OpenAddEmployeeOverlay);
            TerminateEmployeeCommand = new RelayCommand<EmployeeDisplayModel>(OpenTerminateEmployeeOverlay,
                employee => employee != null && employee.TerminationDate == null);

            EmployeesView = CollectionViewSource.GetDefaultView(AllEmployees);
            EmployeesView.Filter = EmployeeFilter;

            LoadEmployees();
        }

        // Loads employees and departments from the service layer
        private void LoadEmployees()
        {
            try
            {
                AllEmployees.Clear();
                foreach (var employeeDisplayModel in _employeeService.GetEmployeeDisplayModels())
                    AllEmployees.Add(employeeDisplayModel);

                var departmentNames = _employeeService.GetAllDepartments()
                    .Select(department => department.Name)
                    .Distinct()
                    .OrderBy(departmentName => departmentName);

                Departments.Clear();
                Departments.Add(AllDepartments);
                foreach (var departmentName in departmentNames)
                    Departments.Add(departmentName);

                SelectedDepartment = AllDepartments;
                EmployeesView.Refresh();
            }
            catch (Exception)
            {
                ErrorMessage = "Unable to load employees. Please try again later.";
            }
        }


        // Filtering logic for the CollectionView - applies both search text and department filter
        private bool EmployeeFilter(object obj)
        {
            if (obj is not EmployeeDisplayModel employeeDisplayModel) return false;

            bool matchesSearch = string.IsNullOrWhiteSpace(SearchText)
                || employeeDisplayModel.FullName.Contains(SearchText, StringComparison.OrdinalIgnoreCase);

            bool matchesDepartment = SelectedDepartment == AllDepartments
                || employeeDisplayModel.DepartmentName == SelectedDepartment;

            return matchesSearch && matchesDepartment;
        }

        // Handles showing an overlay and reloading employees when the overlay is closed
        private void ShowOverlayAndReload(OverlayPanelViewModelBase overlayViewModel)
        {
            overlayViewModel.RequestClose += (sender, eventArgs) =>
            {
                CurrentOverlay = null;
                LoadEmployees();
            };
            ShowOverlay(overlayViewModel);
        }

        // Opens the Add Employee overlay
        private void OpenAddEmployeeOverlay()
        {
            ShowOverlayAndReload(new AddEmployeeViewModel(_employeeService));
        }

        // Opens the Terminate Employee overlay for the selected employee
        private void OpenTerminateEmployeeOverlay(EmployeeDisplayModel displayModel)
        {
            ShowOverlayAndReload(
                new TerminateEmployeeViewModel(displayModel, _employeeService)
            );
        }
    }
}
