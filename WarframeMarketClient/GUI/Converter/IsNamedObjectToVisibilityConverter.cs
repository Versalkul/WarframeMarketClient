using System;
using System.Windows;
using System.Windows.Data;

namespace WarframeMarketClient.GUI.Converter
{
    public class IsNamedObjectVisibilityConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value.GetType().Name == "NamedObject")
            {
                return Visibility.Hidden;
            }
            else
            {
                return Visibility.Visible;
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
}
