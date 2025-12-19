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
        // Service
        private readonly IEmployeeService _employeeService;

        // Employee being terminated
        public EmployeeDisplayModel Employee { get; }

        // Selected termination date 
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

        // Command for confirming termination
        public ICommand ConfirmCommand { get; }

        // Constructor
        public TerminateEmployeeViewModel(EmployeeDisplayModel employee,
                                      IEmployeeService employeeService)
        {
            Employee = employee;
            _employeeService = employeeService;

            ConfirmCommand = new RelayCommand(Confirm, CanConfirm);
        }

        // Executes the termination by delegating to the service layer
        private void Confirm()
        {
            var dateOnly = DateOnly.FromDateTime(TerminationDate);
            _employeeService.TerminateEmployee(Employee.EmployeeID, dateOnly);
            CloseOverlay();
        }

        // Determines if the termination date is valid, used to enable or disable confirm command
        private bool CanConfirm()
        {
            return TerminationDate.Date >= DateTime.Today;
        }
    }
}
