using Application.Services;  
using Data;                    
using Data.AdoNet;
using Presentation.Wpf.ViewModels;  
using System.Windows.Controls;


namespace Presentation.Wpf.Views
{
    public partial class DeviceView : UserControl
    {
        public DeviceView()
        {
            InitializeComponent();

            var db = DatabaseConnection.GetInstance();

            var deviceRepo = new DeviceRepository(db);

            var deviceService = new DeviceService(deviceRepo);

            var viewModel = new DeviceViewModel(deviceService);

            DataContext = viewModel;
        }
    }
}
