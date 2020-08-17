using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace robot_simulator.ViewModels.ValueConterters
{
    class QueueDisplayLimitConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var collection = value as IEnumerable<WarehouseItemViewModel>;
                var limit = int.Parse(parameter as string);
                return collection.Take(limit).ToList();
            }
            catch (Exception exc)
            {
                Log.Error(exc.Message);
            }
            return new List<WarehouseItemViewModel>();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
