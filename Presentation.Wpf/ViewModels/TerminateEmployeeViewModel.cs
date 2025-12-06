using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Application.Interfaces;
using Application.Models;
using Application.Models.DisplayModels;
using Presentation.Wpf.Commands;

namespace Presentation.Wpf.ViewModels
{
    public class TerminateEmployeeViewModel : OverlayPanelViewModelBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeeDisplayModel Employee { get; }

        private DateTime _terminationDate = DateTime.Today;
        public DateTime TerminationDate
        {
            get => _terminationDate;
            set
            {
                _terminationDate = value;
                OnPropertyChanged();
                ((RelayCommand)ConfirmCommand).RaiseCanExecuteChanged();
            }
        }

        public ICommand ConfirmCommand { get; }

        public TerminateEmployeeViewModel(EmployeeDisplayModel employee,
                                      IEmployeeService employeeService)
        {
            Employee = employee;
            _employeeService = employeeService;

            ConfirmCommand = new RelayCommand(Confirm, CanConfirm);
        }

        private void Confirm()
        {
            var dateOnly = DateOnly.FromDateTime(TerminationDate);
            _employeeService.TerminateEmployee(Employee.EmployeeID, dateOnly);
            CloseOverlay();
        }

        private bool CanConfirm()
        {
            return TerminationDate.Date >= DateTime.Today;
        }
    }
}
