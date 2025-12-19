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

        private DateTime? _registrationDate = DateTime.Today;

        // When RegistrationDate changes, auto-update ExpiryDate to 3 years later
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
                        if (!ExpiryDate.HasValue ||
                            (oldRegistrationDate.HasValue &&
                             ExpiryDate.Value == oldRegistrationDate.Value.AddYears(3)))
                        {
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

            SelectedDeviceType = DeviceTypeOptions.FirstOrDefault() ?? string.Empty;
            SelectedOS = OSOptions.FirstOrDefault() ?? string.Empty;
            SelectedCountry = CountryOptions.FirstOrDefault() ?? string.Empty;

            RegisterCommand = new RelayCommand(RegisterDevice, CanRegisterDevice);
            CancelCommand = new RelayCommand(Cancel);
        }


        //Register a device and fire an event to update the devicelist
        private void RegisterDevice()
        {
            if (RegistrationDate is null || ExpiryDate is null)
                return;

            int deviceDescriptionId = _deviceDescriptionService
                .GetDeviceDescriptionID(SelectedDeviceType, SelectedOS, SelectedCountry);

            var device = new Device
            {
                DeviceDescriptionID = deviceDescriptionId,
                Status = DeviceStatus.INSTOCK,
                PurchaseDate = DateOnly.FromDateTime(RegistrationDate.Value),
                ExpectedEndDate = DateOnly.FromDateTime(ExpiryDate.Value)
            };

            _deviceService.AddDevice(device);
            DeviceUpdated?.Invoke(this, EventArgs.Empty);

            ClearFields();
            CloseOverlay();
        }

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
