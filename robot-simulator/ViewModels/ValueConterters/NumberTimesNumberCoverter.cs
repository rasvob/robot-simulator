using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace robot_simulator.ViewModels.ValueConterters
{
    public class NumberTimesNumberCoverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int num1 = (int)value;
            int num2 = int.Parse(parameter.ToString());
            return num1 * num2;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int num1 = (int)value;
            int num2 = (int)parameter;
            return num1 / num2;
        }
    }
}
