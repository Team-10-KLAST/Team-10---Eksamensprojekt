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
        private readonly IDeviceService _deviceService;
        private readonly ILoanService _loanService;

        
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

        //Constructor. Creating all relevant instances, populating all collections.
        public AddRequestViewModel (IRequestService requestService, IDeviceDescriptionService descriptionService, IEmployeeService employeeService, IDeviceService deviceService, ILoanService loanService)
        {
            _requestService = requestService;
            _deviceDescriptionService = descriptionService;
            _employeeService = employeeService;
            _loanService = loanService;
            _deviceService = deviceService;

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
        Request _request;
        private void SubmitRequest ()
        {
            _request = new Request
            {
                Justification = RequestComment,
                RequestDate = DateOnly.FromDateTime(DateTime.Now),
                RequestStatus = "Pending"
            };
            _requestService.SubmitRequest(_request);
            CreateVirtualDevice();
            CreateLoan();
        }

        //Create VirtualDevice with only DeviceDescription
        Device _device;

        private void CreateVirtualDevice ()
        {
            
            int deviceDescription = _deviceDescriptionService.GetDeviceDescriptionID(SelectedDeviceType, SelectedOS, SelectedCountry);
            _device = new Device
            {
                DeviceDescriptionID = deviceDescription
            };
            _deviceService.AddDevice(_device);
        }

        //Create a Loan to store BorrowID from email input
        private void CreateLoan ()
        {
            var _loan = new Loan()
            {
                RequestID = _request.RequestID,
                BorrowerID= _employeeService.GetEmployeeByEmail(Email).EmployeeID,
                DeviceID = _device.DeviceID,
            };
            _loanService.AddLoan(_loan);
        }

        //Comment must not be empty, email must exists in DB.
        private bool CanSubmitRequest()
        {
            if (string.IsNullOrEmpty(Email) ||
                string.IsNullOrEmpty(RequestComment))
            {
                return false;
            }
            return ValidateEmail (Email);
        }

        //Collection for all employee emails, populated when VM constructor is run
        private List<string> AllowedEmails;
        //Validate if email exists in DB
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
