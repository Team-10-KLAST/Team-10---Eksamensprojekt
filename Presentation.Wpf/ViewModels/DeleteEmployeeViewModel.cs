using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Presentation.Wpf.Commands;

namespace Presentation.Wpf.ViewModels;

public class DeleteEmployeeViewModel : INotifyPropertyChanged
{
    private DeleteEmployeeViewModel _selectedEmployee;
    private int _assignedDeviceCount;
    private string? _confirmationText;

    public DeleteEmployeeViewModel(Employee selectedEmployee, int assignedDeviceCount)
    {
        _selectedEmployee = selectedEmployee;
        _assignedDeviceCount = assignedDeviceCount;

        DeleteEmployeeCommand = new RelayCommand(
            DeleteEmployee,
            () => CanConfirmDelete);

        CancelCommand = new RelayCommand(
            Cancel);
    }


    /* public Employee SelectedEmployee
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
     }*/

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
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public bool CanConfirmDelete =>
    !HasDevices && ConfirmationText == "DELETE";

    public ICommand DeleteEmployeeCommand { get;  }
    public ICommand CancelCommand { get; }

    // Event som view kan lytte til for at lukke overlayet, skal måske ikke bruges pga arv fra base? 
    public event EventHandler? RequestClose;

    private void DeleteEmployee()
    {
        // TODO: kald på service for at slette mployee
        // _employeeService.DeleteEmployee(SelectedEmployee);

        // Når sletning er gennemfærtm luk overlay
        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    private void Cancel()
    {
        // Luk overlay
        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string name = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
