using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Presentation.Wpf.Helpers
{
    public class MultiplyDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is double d) || double.IsNaN(d) || double.IsInfinity(d))
            {
                return 320.0; // fallback hvis ActualWidth ikke er klar endnu
            }

            double factor = 0.5;
            if (parameter != null && double.TryParse(parameter.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double p))
                factor = p;

            var result = d * factor;
            if (double.IsNaN(result) || double.IsInfinity(result) || result <= 0)
                return 500.0;

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
