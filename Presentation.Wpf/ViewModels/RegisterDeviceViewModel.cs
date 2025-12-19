using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Application.Interfaces.Service;
using Application.Models;
using Presentation.Wpf.Commands;

namespace Presentation.Wpf.ViewModels
{
    // ViewModel for the register device overlay
    internal class RegisterDeviceViewModel : OverlayPanelViewModelBase
    {
        // Services
        private readonly IDeviceDescriptionService _deviceDescriptionService;
        private readonly IDeviceService _deviceService;

        // Raised after a device is successfully registered
        // Used by parent views to refresh device lists
        public event EventHandler? DeviceUpdated;

        // ComboBox option collections
        public ObservableCollection<string> DeviceTypeOptions { get; }
        public ObservableCollection<string> OSOptions { get; }
        public ObservableCollection<string> CountryOptions { get; }

        // Selected device type
        private string _selectedDeviceType = string.Empty;
        public string SelectedDeviceType
        {
            get => _selectedDeviceType;
            set
            {
                if (SetProperty(ref _selectedDeviceType, value))
                    (RegisterCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        // Selected operating system
        private string _selectedOS = string.Empty;
        public string SelectedOS
        {
            get => _selectedOS;
            set
            {
                if (SetProperty(ref _selectedOS, value))
                    (RegisterCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        // Selected country
        private string _selectedCountry = string.Empty;
        public string SelectedCountry
        {
            get => _selectedCountry;
            set
            {
                if (SetProperty(ref _selectedCountry, value))
                    (RegisterCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        // Registration date selected by the user
        // Used as the basis for calculating a default expiration date
        private DateTime? _registrationDate = DateTime.Today;
        public DateTime? RegistrationDate
        {
            get => _registrationDate;
            set
            {
                var oldRegistrationDate = _registrationDate;

                if (SetProperty(ref _registrationDate, value))
                {
                    if (_registrationDate.HasValue)
                    {
                        var newAutoExpiry =
                            _deviceService.CalculateDefaultExpiryDate(_registrationDate.Value);

                        var oldAutoExpiry = oldRegistrationDate.HasValue
                            ? _deviceService.CalculateDefaultExpiryDate(oldRegistrationDate.Value)
                            : (DateTime?)null;

                        if (!ExpiryDate.HasValue ||
                            (oldAutoExpiry.HasValue && ExpiryDate == oldAutoExpiry))
                        {
                            ExpiryDate = newAutoExpiry;
                        }
                    }

                    (RegisterCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        // Expiration date for the device
        // Automatically calculated from the registration date unless manually overridden
        private DateTime? _expiryDate;
        public DateTime? ExpiryDate
        {
            get => _expiryDate;
            set
            {
                if (SetProperty(ref _expiryDate, value))
                    (RegisterCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        // Commands
        public ICommand RegisterCommand { get; }
        public ICommand CancelCommand { get; }

        // Constructor
        // Initializes services, loads option lists, sets default selections and configures commands
        public RegisterDeviceViewModel(
            IDeviceDescriptionService deviceDescriptionService,
            IDeviceService deviceService)
        {
            _deviceDescriptionService = deviceDescriptionService;
            _deviceService = deviceService;

            DeviceTypeOptions = new ObservableCollection<string>(
                _deviceDescriptionService.GetAllDeviceTypeOptions());

            OSOptions = new ObservableCollection<string>(
                _deviceDescriptionService.GetAllOSOptions());

            CountryOptions = new ObservableCollection<string>(
                _deviceDescriptionService.GetAllCountryOptions());

            SelectedDeviceType = DeviceTypeOptions.FirstOrDefault() ?? string.Empty;
            SelectedOS = OSOptions.FirstOrDefault() ?? string.Empty;
            SelectedCountry = CountryOptions.FirstOrDefault() ?? string.Empty;

            RegisterCommand = new RelayCommand(RegisterDevice, CanRegisterDevice);
            CancelCommand = new RelayCommand(Cancel);
        }

        // Handles device registration by collecting UI input,
        // mapping it to a Device model and delegating persistence to the service layer
        private void RegisterDevice()
        {
            if (RegistrationDate is null || ExpiryDate is null)
                return;

            int deviceDescriptionId = _deviceDescriptionService
                .GetDeviceDescriptionID(SelectedDeviceType, SelectedOS, SelectedCountry);

            var device = new Device
            {
                DeviceDescriptionID = deviceDescriptionId,
                PurchaseDate = DateOnly.FromDateTime(RegistrationDate.Value),
                ExpectedEndDate = DateOnly.FromDateTime(ExpiryDate.Value)
            };

            _deviceService.AddDevice(device);
            DeviceUpdated?.Invoke(this, EventArgs.Empty);

            ClearFields();
            CloseOverlay();
        }

        // Determines whether all required input is present and valid
        // Used to enable or disable the register command
        private bool CanRegisterDevice()
        {
            if (string.IsNullOrWhiteSpace(SelectedDeviceType) ||
                string.IsNullOrWhiteSpace(SelectedOS) ||
                string.IsNullOrWhiteSpace(SelectedCountry) ||
                RegistrationDate is null ||
                ExpiryDate is null)
            {
                return false;
            }

            return ExpiryDate.Value >= RegistrationDate.Value;
        }

        // Cancels registration and closes the overlay
        private void Cancel()
        {
            ClearFields();
            CloseOverlay();
        }

        // Resets UI fields to default values
        private void ClearFields()
        {
            SelectedDeviceType = DeviceTypeOptions.FirstOrDefault() ?? string.Empty;
            SelectedOS = OSOptions.FirstOrDefault() ?? string.Empty;
            SelectedCountry = CountryOptions.FirstOrDefault() ?? string.Empty;

            RegistrationDate = DateTime.Today;
            ExpiryDate = _deviceService.CalculateDefaultExpiryDate(DateTime.Today);

            (RegisterCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }
}
