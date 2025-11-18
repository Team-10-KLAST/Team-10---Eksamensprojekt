using System;
using System.Windows.Input;
using Application.Models;
using Application.Interfaces;
using Presentation.Wpf.Commands;

namespace Presentation.Wpf.ViewModels;

public class DeleteEmployeeViewModel : OverlayPanelViewModelBase
{
    private readonly IEmployeeService _employeeService;

    private Employee _selectedEmployee;
    private int _assignedDeviceCount;
    private string? _confirmationText;

    public DeleteEmployeeViewModel(Employee selectedEmployee, int assignedDeviceCount, IEmployeeService employeeService)
    {
        _selectedEmployee = selectedEmployee;
        _assignedDeviceCount = assignedDeviceCount;
        _employeeService = employeeService;

        DeleteEmployeeCommand = new RelayCommand(DeleteEmployee, () => CanConfirmDelete);
        CancelCommand = new RelayCommand(Cancel);
    }

     public Employee SelectedEmployee
     {
         get => _selectedEmployee;
         set
         {
             if (_selectedEmployee != value)
             {
                 _selectedEmployee = value;
                 OnPropertyChanged();
             }
         }
     }

    public string FullName => $"{SelectedEmployee.FirstName} {SelectedEmployee.LastName}";
    public string Email => SelectedEmployee.Email;
   // public string Department => SelectedEmployee.Department;

    public int AssignedDeviceCount
    {
        get => _assignedDeviceCount;
        set
        {
            if (_assignedDeviceCount != value)
            {
                _assignedDeviceCount = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasDevices));
                OnPropertyChanged(nameof(DeviceStatusText));
                OnPropertyChanged(nameof(CanConfirmDelete));
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public bool HasDevices => AssignedDeviceCount > 0;

    public string DeviceStatusText =>
        HasDevices
        ? $"Employee has {AssignedDeviceCount} assigned devices. Deletion is not possible."
        : "Employee has 0 assigned devices. Deletion is possible.";

    public string? ConfirmationText
    {
        get => _confirmationText;
        set
        {
            if (_confirmationText != value)
            {
                _confirmationText = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanConfirmDelete));
                OnPropertyChanged(nameof(ShowDeleteError));
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public bool CanConfirmDelete =>
    !HasDevices && ConfirmationText == "DELETE";

    public bool ShowDeleteError =>
        !HasDevices &&
        !string.IsNullOrWhiteSpace(ConfirmationText) &&
        ConfirmationText != "DELETE";


    public ICommand DeleteEmployeeCommand { get;  }
    public ICommand CancelCommand { get; }


    private void DeleteEmployee()
    {
        // _employeeService.DeleteEmployee(SelectedEmployee.EmployeeId);
        CloseOverlay();
    }

    private void Cancel()
    { 
        CloseOverlay();
    }

    

    
}
