using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Presentation.Wpf.Commands;

namespace Presentation.Wpf.ViewModels
{
    // Base class for overlay panel view models. Provides close functionality.
    public class OverlayPanelViewModelBase : ViewModelBase
    {
        // Event triggered when the overlay requests to be closed.
        public event EventHandler? RequestClose;

        // Method to invoke the RequestClose event.
        protected void CloseOverlay()
        {
            RequestClose?.Invoke(this, EventArgs.Empty);
        }

        // Command to close the overlay.
        public ICommand CloseCommand {get; }

        // Constructor initializing the CloseCommand. Protected to allow inheritance and prevent direct instantiation.
        protected OverlayPanelViewModelBase()
        {
            CloseCommand = new RelayCommand(() =>
            {
                CloseOverlay();
            });
        }
    }
}
