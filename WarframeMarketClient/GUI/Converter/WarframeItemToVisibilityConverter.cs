using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using WarframeMarketClient.Model;

namespace WarframeMarketClient.GUI.Converter
{
    /// <summary>
    /// Changes the Visibility According to the given Parameter ( any combination of the chars below )
    /// P: Placeholder; Visible on Placeholder
    /// A: Add;         Visible during add-editing of an item (editing of a not yet placed offer)
    /// E: Edit;        Visible during editing of the item
    /// N: Normal;      Visible in normal state
    /// </summary>
    public class WarframeItemToVisibilityConverter : IMultiValueConverter
    {

        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            object val = value[0];

            string setting = parameter is string ? parameter as string : ""; // A: Add, E: Edit, P: Placeholder, N: Normal
            setting = setting == "" ? "N" : setting; // Default value

            string status = val.GetType().Name == "NamedObject" ? "P" :
                val is WarframeItem &&  (String.IsNullOrWhiteSpace((val as WarframeItem).Id) ) ? "A" :
                val is WarframeItem && ((val as WarframeItem).IsEditing || (val as WarframeItem).HasChanged) ? "E" :
                "N";

            if (setting.Contains(status))
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
