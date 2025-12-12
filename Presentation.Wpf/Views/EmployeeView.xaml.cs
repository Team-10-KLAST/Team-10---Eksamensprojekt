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
    public partial class EmployeeView : UserControl
    {
        public EmployeeView()
        {
            InitializeComponent();
        }

        private void OverlayHost_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
