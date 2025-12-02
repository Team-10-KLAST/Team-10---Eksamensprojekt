using System.Windows.Controls;
using Application.Models;
using Application.Services;
using Data;
using Data.AdoNet;
using Presentation.Wpf.ViewModels;

namespace Presentation.Wpf.Views
{
    // Code-behind for the dashboard view. Creates repositories, services and the view model.
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();

            var db = DatabaseConnection.GetInstance();

            // Creates repositories used by the dashboard
            var requestRepository = new RequestRepository(db);
            var deviceRepository = new DeviceRepository(db);
            var loanRepository = new LoanRepository(db);
            var employeeRepository = new EmployeeRepository(db);
            var deviceDescriptionRepository = new DeviceDescriptionRepository(db);

            // Creates services used by the view model
            var requestService = new RequestService(requestRepository);
            var deviceService = new DeviceService(deviceRepository);

            // Creates the dashboard view model and assigns it as DataContext
            var viewModel = new DashboardViewModel(
                requestService,
                deviceService,
                loanRepository,
                employeeRepository,
                deviceDescriptionRepository);

            DataContext = viewModel;
        }
    }
}
