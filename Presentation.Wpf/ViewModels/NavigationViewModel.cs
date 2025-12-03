using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Application.Interfaces;
using Application.Interfaces.Repository;
using Application.Interfaces.Service;
using Application.Models;
using Application.Services;
using Data.AdoNet;
using Presentation.Wpf.Commands;

namespace Presentation.Wpf.ViewModels
{
    public class NavigationViewModel : ViewModelBase
    {
        private readonly IRequestService _requestService;
        private readonly IDeviceService _deviceService;
        private readonly IEmployeeService _employeeService;

        // Repositories used to load additional data needed for display strings
        private readonly IRepository<Loan> _loanRepository;
        private readonly IRepository<Employee> _employeeRepository;
        private readonly IRepository<DeviceDescription> _deviceDescriptionRepository;

        private object _currentView;
        public object CurrentView
        {
            get => _currentView;
            set
            {
                _currentView = value;
                OnPropertyChanged(nameof(CurrentView));
            }
        }

        public ICommand ShowDashboardCommand { get; }
        public ICommand ShowEmployeesCommand { get; }
        public ICommand ShowDevicesCommand { get; }

        private readonly DashboardViewModel _dashboard;
        private readonly EmployeeViewModel _employees;
        private readonly DeviceViewModel _devices;

        public NavigationViewModel(IRequestService requestService, IDeviceService deviceService, IEmployeeService employeeService,
            IRepository<Loan> loanRepository, IRepository<Employee> employeeRepository, IRepository<DeviceDescription> deviceDescriptionRepository)
        {
            _requestService = requestService;
            _deviceService = deviceService;
            _employeeService = employeeService;
            _loanRepository = loanRepository;
            _employeeRepository = employeeRepository;
            _deviceDescriptionRepository = deviceDescriptionRepository;

            _dashboard = new DashboardViewModel(requestService, deviceService, loanRepository, employeeRepository, deviceDescriptionRepository);
            _employees = new EmployeeViewModel(employeeService);
            _devices = new DeviceViewModel(deviceService);

            ShowDashboardCommand = new RelayCommand(() => CurrentView = _dashboard);
            ShowEmployeesCommand = new RelayCommand(() => CurrentView = _employees);
            ShowDevicesCommand = new RelayCommand(() => CurrentView = _devices);

            CurrentView = _dashboard;
        }
    }
}
