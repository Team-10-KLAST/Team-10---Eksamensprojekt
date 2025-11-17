using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Presentation.Wpf.Commands;
using System.Collections.ObjectModel;

namespace Presentation.Wpf.ViewModels
{
    public class EmployeeViewModel : ViewModelBase
    {
        private readonly IEmployeeService _employeeService;

        public ObservableCollection<Employee> Employees { get; set; } = new();
        public ObservableCollection<string> Departments { get; set; } = new();
        public OverlayHostViewModel OverlayHost { get; } = new OverlayHostViewModel();

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                    FilterEmployees();
            }
        }

        private string _selectedDepartment;
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
            DeleteEmployeeCommand = new RelayCommand<Employee>(OpenDeleteEmployeeOverlay);

            LoadEmployees();
        }

        private void LoadEmployees()
        {
            var employees = _employeeService.GetAllEmployees();
            Employees.Clear();
            foreach (var employee in employees)
            {
                Employees.Add(employee);
            }
            // afdelinger
            var uniqueDepartments = employees
                .Select(e => e.Department)
                .Where(d => !string.IsNullOrWhiteSpace(d))
                .Distinct()
                .OrderBy(d => d);

            Departments.Clear();
            Departments.Add("All"); 
            foreach (var dept in uniqueDepartments)
            {
                Departments.Add(dept);
            }

            SelectedDepartment = "All"; 
        }


        //Skal måske lægge datafiltrering i EmployeeService?
        private void FilterEmployees()
        {
            var allEmployees = _employeeService.GetAllEmployees();

            var filtered = allEmployees.Where(e =>
                (string.IsNullOrWhiteSpace(SearchText) || e.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) &&
                (SelectedDepartment == "All" || e.Department == SelectedDepartment));

            Employees.Clear();
            foreach (var employee in filtered)
            {
                Employees.Add(employee);
            }
        }

        private void OpenAddEmployeeOverlay()
        {
            var vm = new AddEmployeeViewModel(_employeeService);
            vm.RequestClose += (s, e) =>
            {
                OverlayHost.CurrentOverlay = null;
                LoadEmployees();
            };
            OverlayHost.ShowOverlay(vm);
        }

        private void OpenDeleteEmployeeOverlay(Employee employee)
        {
            var vm = new DeleteEmployeeViewModel(employee, _employeeService);
            vm.RequestClose += (s, e) =>
            {
                OverlayHost.CurrentOverlay = null;
                LoadEmployees();
            };
            OverlayHost.ShowOverlay(vm);
        }






        //Metode der er brug for i EmployeeService
        public List<Employee> SearchEmployees(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return GetAllEmployees();

            return _repository.GetAll()
                .Where(e =>
                    e.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                    e.Email.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

    }
}
