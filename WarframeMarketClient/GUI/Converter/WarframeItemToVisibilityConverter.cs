using System;
using System.Windows;
using System.Windows.Data;
using WarframeMarketClient.Model;

namespace WarframeMarketClient.GUI.Converter
{
    public class WarframeItemToVisibilityConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string setting = parameter is string ? parameter as string : ""; // A: Add, E: Edit, P: Placeholder, N: Normal
            string status = value.GetType().Name == "NamedObject" ? "P" :
                value is WarframeItem &&  (String.IsNullOrWhiteSpace((value as WarframeItem).Id) || (value as WarframeItem).HasChanged) ? "A" :
                value is WarframeItem && ((value as WarframeItem).Editing) ? "E" :
                "N";
            if (setting.Contains(status) || status == "N" && setting == "") // Placeholder
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
}
