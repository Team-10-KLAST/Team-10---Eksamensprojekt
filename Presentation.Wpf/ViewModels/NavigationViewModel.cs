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
        // Services
        private readonly IRequestService _requestService;
        private readonly IDeviceService _deviceService;
        private readonly IEmployeeService _employeeService;
        private readonly IDeviceDescriptionService _deviceDescriptionService;
        private readonly IDashboardService _dashboardService;
        private readonly ILoanService _loanService;

        // Navigation action to go back to role selection
        private readonly Action _navigateBack;

        // CurrentView property for navigation
        private object? _currentView;
        public object? CurrentView
        {
            get => _currentView;
            set
            {
                _currentView = value;
                OnPropertyChanged(nameof(CurrentView));
            }
        }

        // Commands
        public ICommand LogoutCommand { get; }
        public ICommand ShowDashboardCommand { get; }
        public ICommand ShowEmployeesCommand { get; }
        public ICommand ShowDevicesCommand { get; }

        // ViewModel instances for each section
        private readonly DashboardViewModel _dashboard;
        private readonly EmployeeViewModel _employees;
        private readonly DeviceViewModel _devices;

        // Constructor
        public NavigationViewModel(Action navigateBack, IRequestService requestService, IDeviceService deviceService, IEmployeeService employeeService, IDeviceDescriptionService deviceDescriptionService, IDashboardService dashboardService, ILoanService loanService)
        {
            _navigateBack = navigateBack;
            _requestService = requestService;
            _deviceService = deviceService;
            _employeeService = employeeService;
            _deviceDescriptionService = deviceDescriptionService;
            _dashboardService = dashboardService;
            _loanService = loanService;

            _dashboard = new DashboardViewModel(dashboardService,requestService, deviceService, employeeService, deviceDescriptionService);
            _employees = new EmployeeViewModel(employeeService, loanService, deviceService, deviceDescriptionService);
            _devices = new DeviceViewModel(deviceService, deviceDescriptionService);

            LogoutCommand = new RelayCommand(() => _navigateBack());
            ShowDashboardCommand = new RelayCommand(() =>
            {
                _dashboard.Refresh();
                CurrentView = _dashboard;
            });
            ShowEmployeesCommand = new RelayCommand(() =>
            {
                _employees.Refresh();
                CurrentView = _employees;
            });
            ShowDevicesCommand = new RelayCommand(() =>
            {
                _devices.Refresh();
                CurrentView = _devices;
            });

            CurrentView = _dashboard;            
        }
    }
}
