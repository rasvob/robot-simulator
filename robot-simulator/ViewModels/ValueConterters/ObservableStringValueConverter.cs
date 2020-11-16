using System;
using System.Globalization;
using System.Windows.Data;

namespace robot_simulator.ViewModels.ValueConterters
{
    public class ObservableStringValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var res = value as ObservableString;
            return (value as ObservableString)?.Value ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new ObservableString() { Value = value.ToString() };
        }
    }
}
