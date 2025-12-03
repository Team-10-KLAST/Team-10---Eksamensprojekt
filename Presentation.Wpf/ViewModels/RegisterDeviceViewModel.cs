using System;
using System.Collections.ObjectModel;
using System.Linq;
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

        // ComboBox collections
        public ObservableCollection<string> DeviceTypeOptions { get; }
        public ObservableCollection<string> OSOptions { get; }
        public ObservableCollection<string> CountryOptions { get; }
        public ObservableCollection<string> OwnershipOptions { get; }

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

        private string _selectedOwnership = string.Empty;
        public string SelectedOwnership
        {
            get => _selectedOwnership;
            set
            {
                if (SetProperty(ref _selectedOwnership, value))
                {
                    (RegisterCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        // Optional owner name field
        private string _ownerName = string.Empty;
        public string OwnerName
        {
            get => _ownerName;
            set => SetProperty(ref _ownerName, value);
        }

        // Date fields
        private DateTime? _registrationDate = DateTime.Today;
        public DateTime? RegistrationDate
        {
            get => _registrationDate;
            set
            {
                if (SetProperty(ref _registrationDate, value))
                {
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

            // Ownership options are currently UI only (DB has no column yet)
            OwnershipOptions = new ObservableCollection<string>
            {
                "Company device",
                "BYOD (employee owned)"
            };

            // Set default selections
            SelectedDeviceType = DeviceTypeOptions.FirstOrDefault() ?? string.Empty;
            SelectedOS = OSOptions.FirstOrDefault() ?? string.Empty;
            SelectedCountry = CountryOptions.FirstOrDefault() ?? string.Empty;
            SelectedOwnership = OwnershipOptions.FirstOrDefault() ?? string.Empty;

            RegisterCommand = new RelayCommand(RegisterDevice, CanRegisterDevice);
            CancelCommand = new RelayCommand(Cancel);
        }

        // Actions

        private void RegisterDevice()
        {
            // Sanity check - should not be possible to reach here if dates are null
            if (RegistrationDate is null || ExpiryDate is null)
                return;

            // 1) Get DeviceDescriptionID from selected options
            int deviceDescriptionId = _deviceDescriptionService
                .GetDeviceDescriptionID(SelectedDeviceType, SelectedOS, SelectedCountry);

            // 2) Determine device status based on owner name
            string status = string.IsNullOrWhiteSpace(OwnerName)
                ? "Not in use"
                : "In use";

            // 3) Create Device object
            var device = new Device
            {
                DeviceDescriptionID = deviceDescriptionId,
                DeviceStatus = status,
                // UI does not collect price yet – set to 0 for now
                Price = 0m,
                PurchaseDate = DateOnly.FromDateTime(RegistrationDate.Value),
                ExpectedEndDate = DateOnly.FromDateTime(ExpiryDate.Value)
            };

            // 4) Save to DB via service
            _deviceService.AddDevice(device);

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
                string.IsNullOrWhiteSpace(SelectedOwnership) ||
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
            OwnerName = string.Empty;

            SelectedDeviceType = DeviceTypeOptions.FirstOrDefault() ?? string.Empty;
            SelectedOS = OSOptions.FirstOrDefault() ?? string.Empty;
            SelectedCountry = CountryOptions.FirstOrDefault() ?? string.Empty;
            SelectedOwnership = OwnershipOptions.FirstOrDefault() ?? string.Empty;

            RegistrationDate = DateTime.Today;
            ExpiryDate = DateTime.Today.AddYears(3);

            (RegisterCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }
}
