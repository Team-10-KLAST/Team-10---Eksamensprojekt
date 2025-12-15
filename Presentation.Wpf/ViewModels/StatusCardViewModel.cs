using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Presentation.Wpf.ViewModels
{
    public class StatusCardViewModel
    {
        public string Title { get; set; } = null!;
        public string Icon { get; set; } = null!;
        public IEnumerable Items { get; set; } = null!;
        public int Count { get; set; }
        public ICommand Command { get; set; } = null!;

    }
}
