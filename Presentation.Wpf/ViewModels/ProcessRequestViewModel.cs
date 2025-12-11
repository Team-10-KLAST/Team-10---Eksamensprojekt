using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Application.Interfaces;
using Application.Interfaces.Service;
using Presentation.Wpf.Commands;

namespace Presentation.Wpf.ViewModels
{
    public class ProcessRequestViewModel : OverlayPanelViewModelBase
    {
        // Services and fields
        private readonly IRequestService _requestService;
        private readonly IEmployeeService _employeeService;
        private readonly int _requestId;

        // DisplayModel-data
        private string _tentativeAssigneeEmail = string.Empty;
        public string TentativeAssigneeEmail
        {
            get => _tentativeAssigneeEmail;
            set => SetProperty(ref _tentativeAssigneeEmail, value);
        }

        private string _deviceType = string.Empty;
        public string DeviceType
        {
            get => _deviceType;
            set => SetProperty(ref _deviceType, value);
        }

        private string _operatingSystem = string.Empty;
        public string OperatingSystem
        {
            get => _operatingSystem;
            set => SetProperty(ref _operatingSystem, value);
        }

        private string _location = string.Empty;
        public string Location
        {
            get => _location;
            set => SetProperty(ref _location, value);
        }

        private DateTime _neededByDate;
        public DateTime NeededByDate
        {
            get => _neededByDate;
            set
            {
                SetProperty(ref _neededByDate, value);
                OnPropertyChanged(nameof(NeededByDateString));
            }
        }
        public string NeededByDateString => NeededByDate != default
        ? NeededByDate.ToString("dd-MM-yyyy")
        : string.Empty;


        private string _requestComment = string.Empty;
        public string RequestComment
        {
            get => _requestComment;
            set => SetProperty(ref _requestComment, value);
        }

        //Validation of approver
        private string _approver = string.Empty;
        public string Approver
        {
            get => _approver;
            set
            {
                SetProperty(ref _approver, value);

                if (!ValidateEmailFormat(_approver))
                {
                    return;
                }

                if (!string.IsNullOrWhiteSpace(_approver))
                {
                    var employee = _employeeService.GetEmployeeByEmail(_approver);
                    if (employee == null)
                    {
                        EmailErrorMsg = "Approver not found.";
                    }
                    else if (employee.RoleID != 1)
                    {
                        EmailErrorMsg = "Approver not found.";
                    }
                    else
                    {
                        EmailErrorMsg = string.Empty;
                    }
                }

                (ApproveCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (RejectCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        private string _comment = string.Empty;
        public string Comment
        {
            get => _comment;
            set
            {
                SetProperty(ref _comment, value);
                (RejectCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

         // Error messages
        private string _emailErrorMsg = string.Empty;
        public string EmailErrorMsg
        {
            get => _emailErrorMsg;
            private set => SetProperty(ref _emailErrorMsg, value);
        }

        // Commands
        public ICommand RejectCommand { get; }
        public ICommand ApproveCommand { get; }

        // Constructor
        public ProcessRequestViewModel(IRequestService requestService, IEmployeeService employeeService, int requestId)
        {
            _requestService = requestService;
            _employeeService = employeeService;
            _requestId = requestId;

            RejectCommand = new RelayCommand(Reject, CanReject);
            ApproveCommand = new RelayCommand(Approve, CanApprove);

            var displayModel = _requestService.GetProcessRequestDisplayModel(requestId);
            if (displayModel != null)
            {
                TentativeAssigneeEmail = displayModel.TentativeAssigneeEmail;
                DeviceType = displayModel.DeviceType;
                OperatingSystem = displayModel.OperatingSystem;
                Location = displayModel.Location;
                RequestComment = displayModel.RequestComment;
                NeededByDate = displayModel.NeededByDate;
            }
        }

        // Email format validation
        private bool ValidateEmailFormat(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                EmailErrorMsg = "Email is required.";
                return false;
            }

            try
            {
                var addr = new MailAddress(email);
                bool isValid = addr.Address.Equals(email, StringComparison.OrdinalIgnoreCase);

                EmailErrorMsg = isValid ? string.Empty : "Email format is incorrect";
                return isValid;
            }
            catch (FormatException)
            {
                EmailErrorMsg = "Email format is incorrect";
                return false;
            }
        }

        // Approve and Reject methods
        private void Reject()
        {
            var approver = _employeeService.GetEmployeeByEmail(Approver);
            if (approver != null)
            {
                EmailErrorMsg = string.Empty;
                _requestService.RejectRequest(_requestId, approver.EmployeeID, Comment);
                CloseOverlay();
            }
        }

        private void Approve()
        {
            var approver = _employeeService.GetEmployeeByEmail(Approver);
            if (approver != null)
            {
                EmailErrorMsg = string.Empty;
                _requestService.ApproveRequest(_requestId, approver.EmployeeID, Comment);
                CloseOverlay();
            }
        }

        // CanExecute methods for commands
        private bool CanApprove() =>
            !string.IsNullOrWhiteSpace(Approver) && string.IsNullOrEmpty(EmailErrorMsg);

        private bool CanReject() =>
            !string.IsNullOrWhiteSpace(Approver) &&
            string.IsNullOrEmpty(EmailErrorMsg) &&
            !string.IsNullOrWhiteSpace(Comment);
    }
}
