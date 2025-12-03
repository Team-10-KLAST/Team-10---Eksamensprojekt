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
        // Services and Repositories
        private readonly IRequestService _requestService;
        private readonly IDeviceDescriptionService _deviceDescriptionService;
        private readonly IEmployeeService _employeeService;
        private readonly IDeviceService _deviceService;
        private readonly ILoanService _loanService;

        private readonly IRepository<Loan> _loanRepository;
        private readonly IRepository<Employee> _employeeRepository;
        private readonly IRepository<DeviceDescription> _deviceDescriptionRepository;


        public ICommand SelectEmployeeCommand { get; }
        public ICommand SelectAssetManagerCommand { get; }

        private readonly MainWindowViewModel _mainWindow;

        public RoleSelectionViewModel(
        MainWindowViewModel mainWindow,
        IRequestService requestService,
        IDeviceDescriptionService descriptionService,
        IEmployeeService employeeService,
        IDeviceService deviceService,
        ILoanService loanService,
        IRepository<Loan> loanRepository,
        IRepository<Employee> employeeRepository,
        IRepository<DeviceDescription> deviceDescriptionRepository)
        {
            _mainWindow = mainWindow;

            _requestService = requestService;
            _deviceDescriptionService = descriptionService;
            _employeeService = employeeService;
            _deviceService = deviceService;
            _loanService = loanService;

            _loanRepository = loanRepository;
            _employeeRepository = employeeRepository;
            _deviceDescriptionRepository = deviceDescriptionRepository;

            SelectEmployeeCommand = new RelayCommand(() =>
            {
                _mainWindow.CurrentView = new AddRequestViewModel(
                    _requestService,
                    _deviceDescriptionService,
                    _employeeService,
                    _deviceService,
                    _loanService
                );
            });

            SelectAssetManagerCommand = new RelayCommand(() =>
            {
                _mainWindow.CurrentView = new NavigationViewModel(
                    _requestService,
                    _deviceService,
                    _employeeService,
                    _loanRepository,
                    _employeeRepository,
                    _deviceDescriptionRepository
                );
            });
        }
    }
}
