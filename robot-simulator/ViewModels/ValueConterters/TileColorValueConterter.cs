using OptimizationLogic.DTO;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace robot_simulator.ViewModels.ValueConterters
{
    public class TileColorValueConterter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string state = value as string;

            return state switch
            {
                "Forbidden" => new SolidColorBrush(Color.FromRgb(217, 32, 39)),
                "Empty" => new SolidColorBrush(Color.FromRgb(221, 221, 221)),
                "MEB" => new SolidColorBrush(Color.FromRgb(255, 192, 0)),
                "MQB" => new SolidColorBrush(Color.FromRgb(230, 230, 0)),
                "Shuttle" => new SolidColorBrush(Color.FromRgb(128, 128, 128)),
                _ => new SolidColorBrush(Color.FromRgb(213, 64, 98))
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Empty;
        }
    }

    public class TileColorEnumValueConterter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ItemState state = (ItemState)value;

            return state switch
            {
                ItemState.Forbidden => new SolidColorBrush(Color.FromRgb(217, 32, 39)),
                ItemState.Empty => new SolidColorBrush(Color.FromRgb(221, 221, 221)),
                ItemState.MEB => new SolidColorBrush(Color.FromRgb(53, 208, 186)),
                ItemState.MQB => new SolidColorBrush(Color.FromRgb(255, 205, 60)),
                _ => new SolidColorBrush(Color.FromRgb(213, 64, 98))
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ItemState.Empty;
        }
    }

    public class TileEnabledValueConterter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string state = value as string;

            return state switch
            {
                "Empty" => false,
                _ => true
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return true;
        }
    }
}
