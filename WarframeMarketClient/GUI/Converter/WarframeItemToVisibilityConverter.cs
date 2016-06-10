using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using WarframeMarketClient.Model;

namespace WarframeMarketClient.GUI.Converter
{
    public class WarframeItemToVisibilityConverter : IMultiValueConverter
    {

        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            object val = value[0];
            string setting = parameter is string ? parameter as string : ""; // A: Add, E: Edit, P: Placeholder, N: Normal
            string status = val.GetType().Name == "NamedObject" ? "P" :
                val is WarframeItem &&  (String.IsNullOrWhiteSpace((val as WarframeItem).Id) || (val as WarframeItem).HasChanged) ? "A" :
                val is WarframeItem && ((val as WarframeItem).IsEditing) ? "E" :
                "N";
            if (setting.Contains(status) || status == "N" && setting == "") // Placeholder
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }
        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
}
