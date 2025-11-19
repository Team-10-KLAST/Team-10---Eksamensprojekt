using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Application.Services;
using Data;
using Data.AdoNet;
using Presentation.Wpf.ViewModels;

namespace Presentation.Wpf.Views
{
    /// <summary>
    /// Interaction logic for EmployeeView.xaml
    /// </summary>
    public partial class EmployeeView : UserControl
    {
        public EmployeeView()
        {
            InitializeComponent();

            var db = DatabaseConnection.GetInstance();

            var employeeRepo = new EmployeeRepository(db);
            var departmentRepo = new DepartmentRepository(db);
            var roleRepo = new RoleRepository(db);

            var service = new EmployeeService(employeeRepo, departmentRepo, roleRepo);
            var viewModel = new EmployeeViewModel(service);

            DataContext = viewModel;
        }
    }
}
