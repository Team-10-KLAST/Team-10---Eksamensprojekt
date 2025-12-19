using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Presentation.Wpf.Commands;

namespace Presentation.Wpf.ViewModels
{
    // ViewModel for managing overlay panels in the application. It handles displaying and closing overlays.
    public class OverlayHostViewModel : ViewModelBase
    {
        // The currently displayed overlay panel.
        private object? _currentOverlay;
        public object? CurrentOverlay
        {
            get => _currentOverlay;
            set
            {
                if (SetProperty(ref _currentOverlay, value))
                    IsOverlayOpen = value != null;
            }
        }

        //Used for controlling overlay visibility.
        private bool _isOverlayOpen;
        public bool IsOverlayOpen
        {
            get => _isOverlayOpen;
            set => SetProperty(ref _isOverlayOpen, value);
        }

        // Command to close the currently displayed overlay panel.
        public ICommand CloseOverlayCommand { get; }

        // Constructor initializing the CloseOverlayCommand.
        public OverlayHostViewModel()
        {
            CloseOverlayCommand = new RelayCommand(() => CurrentOverlay = null);
        }

        // Method to show a new overlay panel. Subscribes to the overlay's RequestClose event, so it can be closed from within.
        public void ShowOverlay(OverlayPanelViewModelBase overlay)
        {
            overlay.RequestClose += (sender, eventArgs) => CurrentOverlay = null;
            CurrentOverlay = overlay;
        }
    }
}
