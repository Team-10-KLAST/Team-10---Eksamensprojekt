using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Application.Models.DisplayModels
{
    public class StatusCardData
    {
        public string Title { get; set; }
        public string Icon { get; set; }
        public IEnumerable Items { get; set; }
        public int Count { get; set; }
        public ICommand Command { get; set; }

    }
}
