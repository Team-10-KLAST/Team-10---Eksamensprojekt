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
using Presentation.Wpf.Commands;

namespace Presentation.Wpf.ViewModels
{
    public class RoleSelectionViewModel : ViewModelBase
    {
        // Services 
        private readonly IRequestService _requestService;
        private readonly IDeviceDescriptionService _deviceDescriptionService;
        private readonly IEmployeeService _employeeService;
        private readonly IDeviceService _deviceService;
        private readonly ILoanService _loanService;
        private readonly IDashboardService _dashboardService;

        


        public ICommand SelectEmployeeCommand { get; }
        public ICommand SelectAssetManagerCommand { get; }

        private readonly MainWindowViewModel _mainWindow;

        public RoleSelectionViewModel(
        MainWindowViewModel mainWindow,
        IRequestService requestService,
        IDeviceDescriptionService descriptionService,
        IEmployeeService employeeService,
        IDeviceService deviceService,
        ILoanService loanService, IDashboardService dashboardService)
        {
            _mainWindow = mainWindow;

            _requestService = requestService;
            _deviceDescriptionService = descriptionService;
            _employeeService = employeeService;
            _deviceService = deviceService;
            _loanService = loanService;
            _dashboardService = dashboardService;

            Action NavigateBackToRoleSelection = () =>
            {                
                _mainWindow.CurrentView = new RoleSelectionViewModel(
                    _mainWindow,
                    _requestService,
                    _deviceDescriptionService,
                    _employeeService,
                    _deviceService,
                    _loanService,
                    _dashboardService
                );
            };

            SelectEmployeeCommand = new RelayCommand(() =>
            {
                _mainWindow.CurrentView = new AddRequestViewModel(
                    _requestService,
                    _deviceDescriptionService,
                    _employeeService,
                    NavigateBackToRoleSelection
                );
            });

            SelectAssetManagerCommand = new RelayCommand(() =>
            {
                _mainWindow.CurrentView = new NavigationViewModel(
                    _requestService,
                    _deviceService,
                    _employeeService,
                    _deviceDescriptionService,
                    _dashboardService
                );
            });
        }
    }
}
