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
    internal class AddRequestViewModel : ViewModelBase
    {
        private readonly IRequestService _requestService;
        private readonly IDeviceDescriptionService _deviceDescriptionService;
        private readonly IEmployeeService _employeeService;
        private readonly Action _navigateBack;

        //form fields
        private string _email = string.Empty;
        public string Email
        {
            get => _email;
            set
            {
                if (SetProperty(ref _email, value))
                {
                    EmailErrorMsg = _employeeService.ValidateExistingEmployeeEmail(value);
                    (SubmitCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        private DateTime _neededByDate = DateTime.Today;
        public DateTime NeededByDate
        {
            get => _neededByDate;
            set
            {
                if (SetProperty(ref _neededByDate, value))
                {
                    DateErrorMsg = string.Empty;
                    (SubmitCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }
        private string _dateErrorMsg = string.Empty;
        public string DateErrorMsg
        {
            get => _dateErrorMsg;
            private set => SetProperty(ref _dateErrorMsg, value);
        }


        private string _emailErrorMsg = string.Empty;
        public string EmailErrorMsg
        {
            get => _emailErrorMsg;
            private set => SetProperty(ref _emailErrorMsg, value);
        }

        private string _justification = string.Empty;
        public string RequestComment
        {
            get => _justification;
            set
            {
                if (SetProperty(ref _justification, value))
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
        public ObservableCollection<string> DeviceTypeOptions { get; }
        public ObservableCollection<string> CountryOptions { get; }

        //Commands
        public ICommand SubmitCommand { get; }
        public ICommand CancelCommand { get; }

        //Constructor. Creating all relevant instances, populating all collections.
        public AddRequestViewModel (IRequestService requestService, IDeviceDescriptionService descriptionService, IEmployeeService employeeService, Action navigateBack)
        {
            _requestService = requestService;
            _deviceDescriptionService = descriptionService;
            _employeeService = employeeService;

            OSOptions = new ObservableCollection<string>(_deviceDescriptionService.GetAllOSOptions());
            DeviceTypeOptions = new ObservableCollection<string>(_deviceDescriptionService.GetAllDeviceTypeOptions());
            CountryOptions = new ObservableCollection<string>(_deviceDescriptionService.GetAllCountryOptions());

            SelectedOS = OSOptions.FirstOrDefault();
            SelectedDeviceType = DeviceTypeOptions.FirstOrDefault();
            SelectedCountry = CountryOptions.FirstOrDefault();

            SubmitCommand = new RelayCommand(SubmitRequest, CanSubmitRequest);
            CancelCommand = new RelayCommand(Cancel);
            _navigateBack = navigateBack;
        }


        private void SubmitRequest ()
        {
            _requestService.SubmitRequest(Email, SelectedDeviceType, SelectedOS, SelectedCountry, RequestComment, DateOnly.FromDateTime(NeededByDate.Date));
            Cancel();
        }



        //Comment must not be empty, email must exists in DB.
        private bool CanSubmitRequest()
        {
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(RequestComment))
                return false;

            if (NeededByDate.Date < DateTime.Today.Date)
            {
                DateErrorMsg = "Date must not be in the past.";
                return false;
            }

            return string.IsNullOrEmpty(EmailErrorMsg);
        }

        private void Cancel()
        {
            ClearFields();
            _navigateBack();
        }

        private void ClearFields()
        {
            Email = string.Empty;
            EmailErrorMsg = string.Empty;
            RequestComment = string.Empty;
            NeededByDate = DateTime.Today;

            SelectedOS = OSOptions.FirstOrDefault();
            SelectedDeviceType = DeviceTypeOptions.FirstOrDefault();
            SelectedCountry = CountryOptions.FirstOrDefault();

            (SubmitCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }
}
