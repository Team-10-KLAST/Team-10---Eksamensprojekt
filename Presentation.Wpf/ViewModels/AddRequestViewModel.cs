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
using Presentation.Wpf.Commands;

namespace Presentation.Wpf.ViewModels
{
    internal class AddRequestViewModel : OverlayPanelViewModelBase
    {
        private readonly IRequestService _requestService;
        private readonly IDeviceDescriptionService _deviceDescriptionService;
        private readonly IEmployeeService _employeeService;

        //Collection for Email
        private List<string> AllowedEmails;

        //form fields
        private string _email = string.Empty;
        public string Email
        {
            get => _email;
            set
            {
                if (SetProperty(ref _email, value))
                {
                    ValidateEmail(value);
                    (SubmitCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        private string _emailErrorMsg = string.Empty;
        public string EmailErrorMsg
        {
            get => _emailErrorMsg;
            private set => SetProperty(ref _emailErrorMsg, value);
        }

        private string _requestComment = string.Empty;
        public string RequestComment
        {
            get => _requestComment;
            set
            {
                if (SetProperty(ref _requestComment, value))
                {
                    (SubmitCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        //Selection for ComboBoxes
        private string _selectedOS = string.Empty;
        public string SelectedOS
        {
            get => _selectedOS;
            set
            {
                if (SetProperty(ref _selectedOS, value))
                {
                    (SubmitCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        private string _selectedDeviceType = string.Empty;
        public string SelectedDeviceType
        {
            get => _selectedDeviceType;
            set
            {
                if (SetProperty(ref _selectedDeviceType, value))
                {
                    (SubmitCommand as RelayCommand)?.RaiseCanExecuteChanged();
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
                    (SubmitCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        //Collections for Comboboxes
        public ObservableCollection<string> OSOptions { get; }
        public ObservableCollection<string> DeviceOptions { get; }
        public ObservableCollection<string> CountryOptions { get; }

        //Commands
        public ICommand SubmitCommand { get; }
        public ICommand CancelCommand { get; }

        //Constructor
        public AddRequestViewModel (IRequestService requestService, IDeviceDescriptionService descriptionService, IEmployeeService employeeService)
        {
            _requestService = requestService;
            _deviceDescriptionService = descriptionService;
            _employeeService = employeeService;

            OSOptions = new ObservableCollection<string>(_deviceDescriptionService.GetAllOSOptions());
            DeviceOptions = new ObservableCollection<string>(_deviceDescriptionService.GetAllDeviceTypeOptions());
            CountryOptions = new ObservableCollection<string>(_deviceDescriptionService.GetAllCountryOptions());
            AllowedEmails = new List<string>(_employeeService.GetAllEmployeeEmails());

            SelectedOS = OSOptions.FirstOrDefault();
            SelectedDeviceType = DeviceOptions.FirstOrDefault();
            SelectedCountry = CountryOptions.FirstOrDefault();

            SubmitCommand = new RelayCommand(SubmitRequest, CanSubmitRequest);
            CancelCommand = new RelayCommand(Cancel);
        }

        //Action Methods
        private void SubmitRequest ()
        {
            var request = new Request
            {
                Justification = RequestComment,
                RequestDate = DateOnly.FromDateTime(DateTime.Now),
                RequestStatus = "Pending"
            };
            _requestService.SubmitRequest(request);
        }

        private bool CanSubmitRequest()
        {
            if (string.IsNullOrEmpty(Email) ||
                string.IsNullOrEmpty(RequestComment))
            {
                return false;
            }
            return ValidateEmail (Email);
        }

        private bool ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                EmailErrorMsg = string.Empty;
                return false;
            }

            try
            {
                return AllowedEmails.Contains(email);
            }
            catch (FormatException)
            {
                EmailErrorMsg = "Invalid email";
                return false;
            }
        }

        private void Cancel()
        {
            ClearFields();
            CloseOverlay();
        }

        private void ClearFields()
        {
            Email = string.Empty;
            EmailErrorMsg = string.Empty;
            RequestComment = string.Empty;

            SelectedOS = OSOptions.FirstOrDefault();
            SelectedDeviceType = DeviceOptions.FirstOrDefault();
            SelectedCountry = CountryOptions.FirstOrDefault();

            (SubmitCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }
}
