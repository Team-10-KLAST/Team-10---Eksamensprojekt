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
using Presentation.Wpf.Commands;

namespace Presentation.Wpf.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        // Services and repositories
        private readonly IRequestService _requestService;
        private readonly IDeviceDescriptionService _deviceDescriptionService;
        private readonly IEmployeeService _employeeService;
        private readonly IDeviceService _deviceService;
        private readonly ILoanService _loanService;

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

        public ICommand SelectEmployeeCommand { get; }
        public ICommand SelectAssetManagerCommand { get; }

        public MainWindowViewModel(
        IRequestService requestService,
        IDeviceDescriptionService deviceDescriptionService,
        IEmployeeService employeeService,
        IDeviceService deviceService,
        ILoanService loanService,
        IRepository<Loan> loanRepository,
        IRepository<Employee> employeeRepository,
        IRepository<DeviceDescription> deviceDescriptionRepository)
        {
            _requestService = requestService;
            _deviceDescriptionService = deviceDescriptionService;
            _employeeService = employeeService;
            _deviceService = deviceService;
            _loanService = loanService;

            _loanRepository = loanRepository;
            _employeeRepository = employeeRepository;
            _deviceDescriptionRepository = deviceDescriptionRepository;

            // Start på rollevalg
            CurrentView = new RoleSelectionViewModel(
            this,
            _requestService,
            _deviceDescriptionService,
            _employeeService,
            _deviceService,
            _loanService,
            _loanRepository,
            _employeeRepository,
            _deviceDescriptionRepository
        );


            SelectEmployeeCommand = new RelayCommand(() =>
            {
                CurrentView = new AddRequestViewModel(
                    _requestService,
                    _deviceDescriptionService,
                    _employeeService
                );
                OnPropertyChanged(nameof(CurrentView));
            });

            SelectAssetManagerCommand = new RelayCommand(() =>
            {
                CurrentView = new NavigationViewModel(
                    _requestService,
                    _deviceService,
                    _employeeService,
                    _loanRepository,
                    _employeeRepository,
                    _deviceDescriptionRepository
                );
                OnPropertyChanged(nameof(CurrentView));
            });
        }

    }
}
