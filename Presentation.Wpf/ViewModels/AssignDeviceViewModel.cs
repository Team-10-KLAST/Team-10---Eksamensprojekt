using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Application.Interfaces;
using Application.Interfaces.Service;
using Application.Models;
using Application.Models.DisplayModels;
using Presentation.Wpf.Commands;

namespace Presentation.Wpf.ViewModels
{
    public class AssignDeviceViewModel : OverlayPanelViewModelBase
    {
        // Services for loaning devices and retrieving device information
        private readonly ILoanService _loanService;
        private readonly IDeviceService _deviceService;
        private readonly IDeviceDescriptionService _deviceDescriptionService;
        private readonly IEmployeeService _employeeService;

        // Employee information (readonly)
        public int EmployeeID { get; }
        public string EmployeeEmail { get; }
        public string Department { get; }

        // Device type selection
        public ObservableCollection<string> DeviceTypes { get; } = new();

        private string _selectedDeviceType = string.Empty;
        public string SelectedDeviceType
        {
            get => _selectedDeviceType;
            set
            {
                if (SetProperty(ref _selectedDeviceType, value))
                {
                    LoadAvailableDevices();
                    (AssignCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        // Available devices dropdown
        private ObservableCollection<DeviceDisplayModel> _availableDevices = new();
        public ObservableCollection<DeviceDisplayModel> AvailableDevices
        {
            get => _availableDevices;
            set => SetProperty(ref _availableDevices, value);
        }

        private DeviceDisplayModel? _selectedDevice;
        public DeviceDisplayModel? SelectedDevice
        {
            get => _selectedDevice;
            set
            {
                if (SetProperty(ref _selectedDevice, value))
                    (AssignCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        // Approver info
        private string _approverEmail = string.Empty;
        public string ApproverEmail
        {
            get => _approverEmail;
            set
            {
                if (SetProperty(ref _approverEmail, value))
                {
                    ValidateApproverEmail();
                    (AssignCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        // Specific error for approver email
        private string _approverEmailError = string.Empty;
        public string ApproverEmailError
        {
            get => _approverEmailError;
            set
            {
                if (SetProperty(ref _approverEmailError, value))
                    (AssignCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        // Command to assign the selected device
        public ICommand AssignCommand { get; }

        // Constructor to initialize the ViewModel with necessary services and data
        public AssignDeviceViewModel(int employeeID, string employeeEmail, string department,
            IEmployeeService employeeService, ILoanService loanService, IDeviceService deviceService,
            IDeviceDescriptionService deviceDescriptionService)
        {
            EmployeeID = employeeID;
            EmployeeEmail = employeeEmail;
            Department = department;

            _employeeService = employeeService;
            _loanService = loanService;
            _deviceService = deviceService;
            _deviceDescriptionService = deviceDescriptionService;

            AssignCommand = new RelayCommand(AssignDevice, CanAssign);

            LoadDeviceTypes();
            LoadAvailableDevices();
        }

        // Check if a device is selected before allowing assignment
        private bool CanAssign()
        {
            return SelectedDevice != null
                && string.IsNullOrWhiteSpace(ApproverEmailError)
                && !string.IsNullOrWhiteSpace(ApproverEmail);
        }

        // Assign the selected device to the employee
        private void AssignDevice()
        {
            var approver = _employeeService.GetEmployeeByEmail(ApproverEmail)
                ?? throw new InvalidOperationException("Approver not found.");

            var device = SelectedDevice
                ?? throw new InvalidOperationException("No device selected.");

            _loanService.AssignDeviceToEmployee(
                device.DeviceID,
                EmployeeID,
                approver.EmployeeID
            );
            CloseOverlay();
        }

        // Validate the approver email input
        private void ValidateApproverEmail()
        {
            ApproverEmailError = _employeeService.ValidateApproverEmail(ApproverEmail);
        }

        // Load available devices from the device service
        private void LoadDeviceTypes()
        {
            var descriptions = _deviceDescriptionService.GetAllDeviceTypeOptions();

            DeviceTypes.Clear();
            foreach (var description in descriptions)
                DeviceTypes.Add(description);
        }

        // Load available devices based on the selected device type
        private void LoadAvailableDevices()
        {
            AvailableDevices = new ObservableCollection<DeviceDisplayModel>(
                _deviceService.GetAvailableDeviceDisplayModels(SelectedDeviceType));
            (AssignCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }
}
