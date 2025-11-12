using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Data;

namespace Presentation.Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var connectionString = config.GetConnectionString("DefaultConnection");
            DatabaseConnection.Instance(connectionString);

            var mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }

}
