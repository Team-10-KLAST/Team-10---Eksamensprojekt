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
using Application.Interfaces.Service;
using Application.Models;
using Application.Models.DisplayModels;
using Presentation.Wpf.Commands;

namespace Presentation.Wpf.ViewModels
{
    public class EmployeeViewModel : OverlayHostViewModel
    {
        // Services
        private readonly IEmployeeService _employeeService;
        private readonly ILoanService _loanService;
        private readonly IDeviceService _deviceService;
        private readonly IDeviceDescriptionService _deviceDescriptionService;

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
        public ICommand AssignDeviceCommand { get; }
        public ICommand TerminateEmployeeCommand { get; }

        // Constructor
        public EmployeeViewModel(IEmployeeService employeeService, ILoanService loanService,
            IDeviceService deviceService, IDeviceDescriptionService deviceDescriptionService)
        {
            _employeeService = employeeService;
            _loanService = loanService;
            _deviceService = deviceService;
            _deviceDescriptionService = deviceDescriptionService;

            AddEmployeeCommand = new RelayCommand(OpenAddEmployeeOverlay);
            AssignDeviceCommand = new RelayCommand<EmployeeDisplayModel>(OpenAssignDeviceOverlay,
                employee => employee != null && employee.TerminationDate == null);
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

        // Opens the Assign Device overlay for the selected employee
        private void OpenAssignDeviceOverlay(EmployeeDisplayModel displayModel)
        {
            // Her skal du kende approverID — typisk den bruger der er logget ind.
            // Hvis du har en CurrentUserService, så brug den.
            int approverID = 1; // midlertidigt — vi sætter den rigtigt senere

            var overlayVm = new AssignDeviceViewModel(
                displayModel.EmployeeID,
                displayModel.Email,
                displayModel.DepartmentName,
                _employeeService,               
                _loanService,
                _deviceService,
                _deviceDescriptionService
            );

            ShowOverlayAndReload(overlayVm);
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
