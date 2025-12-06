using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.DisplayModels
{
    public class EmployeeDisplayModel
    {
        public int EmployeeID { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string DepartmentName { get; set; } = "";
        public string RoleName { get; set; } = "";
        public DateOnly? TerminationDate { get; set; }
        public int DeviceCount { get; set; }
    }
}
