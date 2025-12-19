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


        private string _justification = string.Empty;
        public string Justification
        {
            get => _justification;
            set => SetProperty(ref _justification, value);
        }

        //Validation of approver
        private string _approver = string.Empty;
        public string Approver
        {
            get => _approver;
            set
            {
                SetProperty(ref _approver, value);

                EmailErrorMsg = _employeeService.ValidateApproverEmail(_approver);

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
                Justification = displayModel.Justification;
                NeededByDate = displayModel.NeededByDate;
            }
        }

        // Approve and Reject methods
        private void Reject()
        {
            var approver = _employeeService.GetEmployeeByEmail(Approver);
            if (approver != null)
            {
                _requestService.RejectRequest(_requestId, approver.EmployeeID, Comment);
                CloseOverlay();
            }
        }

        private void Approve()
        {
            var approver = _employeeService.GetEmployeeByEmail(Approver);
            if (approver != null)
            {
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
