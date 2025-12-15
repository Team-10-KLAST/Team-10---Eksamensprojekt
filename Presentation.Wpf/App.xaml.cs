using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using Application.Services;
using Data;
using Data.AdoNet;
using Microsoft.Extensions.Configuration;
using Presentation.Wpf.ViewModels;

namespace Presentation.Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var connectionString = config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            
            DatabaseConnection.Initialize(connectionString);

            // Hent en instans af DatabaseConnection
            var dbConnection = DatabaseConnection.GetInstance();

            // 1. Opret repositories med dbConnection
            var loanRepository = new LoanRepository(dbConnection);
            var employeeRepository = new EmployeeRepository(dbConnection);
            var deviceDescriptionRepository = new DeviceDescriptionRepository(dbConnection);
            var deviceRepository = new DeviceRepository(dbConnection);
            var requestRepository = new RequestRepository(dbConnection);
            var decisionRepository = new DecisionRepository(dbConnection);
            var departmentRepository = new DepartmentRepository(dbConnection);
            var roleRepository = new RoleRepository(dbConnection);

            // 2. Opret services med repositories            
            var deviceDescriptionService = new DeviceDescriptionService(deviceDescriptionRepository);
            var deviceService = new DeviceService(deviceRepository, deviceDescriptionService);
            var employeeService = new EmployeeService(employeeRepository, departmentRepository, roleRepository, loanRepository);
            var loanService = new LoanService(loanRepository, deviceService);
            var requestService = new RequestService(requestRepository, employeeRepository, deviceRepository,
                deviceDescriptionRepository, loanRepository, decisionRepository, deviceService, loanService, employeeService);
            var dashboardService = new DashboardService(requestService, deviceService, loanRepository, employeeRepository, deviceDescriptionRepository);

            // 3. Opret MainWindowViewModel med services og repos
            var mainWindowVm = new MainWindowViewModel(
                requestService,
                deviceDescriptionService,
                employeeService,
                deviceService,
                loanService,
                dashboardService
            );

            var mainWindow = new MainWindow
            {
                DataContext = mainWindowVm
            };

            mainWindow.Show();
        }

    }

}
