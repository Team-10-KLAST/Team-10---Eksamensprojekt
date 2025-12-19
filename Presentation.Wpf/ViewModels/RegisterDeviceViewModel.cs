using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Application.Interfaces.Service;
using Application.Models;
using Presentation.Wpf.Commands;

namespace Presentation.Wpf.ViewModels
{
    // Same base as AddRequestViewModel so we can reuse CloseOverlay()
    internal class RegisterDeviceViewModel : OverlayPanelViewModelBase
    {
        private readonly IDeviceDescriptionService _deviceDescriptionService;
        private readonly IDeviceService _deviceService;
        public event EventHandler? DeviceUpdated;

        // ComboBox collections
        public ObservableCollection<string> DeviceTypeOptions { get; }
        public ObservableCollection<string> OSOptions { get; }
        public ObservableCollection<string> CountryOptions { get; }

        // Selected values 
        private string _selectedDeviceType = string.Empty;
        public string SelectedDeviceType
        {
            get => _selectedDeviceType;
            set
            {
                if (SetProperty(ref _selectedDeviceType, value))
                {
                    (RegisterCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        private string _selectedOS = string.Empty;
        public string SelectedOS
        {
            get => _selectedOS;
            set
            {
                if (SetProperty(ref _selectedOS, value))
                {
                    (RegisterCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        private string _selectedCountry = string.Empty;
        public string SelectedCountry
        {
            get => _selectedCountry;
            set
            {
                if (SetProperty(ref _selectedCountry, value))
                {
                    (RegisterCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        // Date fields
        private DateTime? _registrationDate = DateTime.Today;
        public DateTime? RegistrationDate
        {
            get => _registrationDate;
            set
            {
                // Keep track of old registration date for expiry date logic
                var oldRegistrationDate = _registrationDate;

                if (SetProperty(ref _registrationDate, value))
                {
                    // Auto-update ExpiryDate based on new RegistrationDate
                    if (_registrationDate.HasValue)
                    {
                        // If ExpiryDate is not set or was previously auto-set to 3 years after old RegistrationDate
                        if (!ExpiryDate.HasValue ||
                            (oldRegistrationDate.HasValue &&
                             ExpiryDate.Value == oldRegistrationDate.Value.AddYears(3)))
                        {
                            // Auto-set ExpiryDate to 3 years after new RegistrationDate
                            ExpiryDate = _registrationDate.Value.AddYears(3);
                        }
                    }

                    (RegisterCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }


        private DateTime? _expiryDate = DateTime.Today.AddYears(3);
        public DateTime? ExpiryDate
        {
            get => _expiryDate;
            set
            {
                if (SetProperty(ref _expiryDate, value))
                {
                    (RegisterCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand RegisterCommand { get; }
        public ICommand CancelCommand { get; }

        // Get list from DB services 
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

            // Set default selections
            SelectedDeviceType = DeviceTypeOptions.FirstOrDefault() ?? string.Empty;
            SelectedOS = OSOptions.FirstOrDefault() ?? string.Empty;
            SelectedCountry = CountryOptions.FirstOrDefault() ?? string.Empty;

            RegisterCommand = new RelayCommand(RegisterDevice, CanRegisterDevice);
            CancelCommand = new RelayCommand(Cancel);
        }

        // Actions
        //Register a device and fire an event to update the devicelist
        private void RegisterDevice()
        {
            if (RegistrationDate is null || ExpiryDate is null)
                return;

            // 1) Get DeviceDescriptionID from selected options
            int deviceDescriptionId = _deviceDescriptionService
                .GetDeviceDescriptionID(SelectedDeviceType, SelectedOS, SelectedCountry);

            // 3) Create Device object
            var device = new Device
            {
                DeviceDescriptionID = deviceDescriptionId,
                Status = DeviceStatus.INSTOCK,
                PurchaseDate = DateOnly.FromDateTime(RegistrationDate.Value),
                ExpectedEndDate = DateOnly.FromDateTime(ExpiryDate.Value)
            };

            // 4) Save to DB via service and fire an event
            _deviceService.AddDevice(device);
            DeviceUpdated?.Invoke(this, EventArgs.Empty);
            // 5) Clear form and close overlay panel
            ClearFields();
            CloseOverlay();
        }

        private bool CanRegisterDevice()
        {
            // All required selections must be made
            if (string.IsNullOrWhiteSpace(SelectedDeviceType) ||
                string.IsNullOrWhiteSpace(SelectedOS) ||
                string.IsNullOrWhiteSpace(SelectedCountry) ||
                RegistrationDate is null ||
                ExpiryDate is null)
            {
                return false;
            }

            // Expiry date must be after or on registration date
            return ExpiryDate.Value >= RegistrationDate.Value;
        }

        private void Cancel()
        {
            ClearFields();
            CloseOverlay();
        }

        private void ClearFields()
        {
            SelectedDeviceType = DeviceTypeOptions.FirstOrDefault() ?? string.Empty;
            SelectedOS = OSOptions.FirstOrDefault() ?? string.Empty;
            SelectedCountry = CountryOptions.FirstOrDefault() ?? string.Empty;

            RegistrationDate = DateTime.Today;
            ExpiryDate = DateTime.Today.AddYears(3);

            (RegisterCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }
}
