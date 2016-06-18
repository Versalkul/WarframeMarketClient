using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WarframeMarketClient.GUI.Converter
{

    /// <summary>
    /// Changes the Visibility According to the given Parameter ( any combination of the chars below )
    /// I: Inverted;    bool => !bool
    /// H: Hidden;      false => Visibility.Hidden instead of Visibility.Collapsed
    /// </summary>
    class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string setting = parameter is string ? parameter as string : ""; // I=Inverted
            return ((bool)value) ^ setting.Contains("I")  ? Visibility.Visible : setting.Contains("H") ? Visibility.Hidden : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
