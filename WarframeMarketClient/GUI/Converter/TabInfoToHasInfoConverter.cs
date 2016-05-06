using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using WarframeMarketClient.Model;

namespace WarframeMarketClient.GUI.Converter
{
    class TabInfoToHasInfoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Console.WriteLine("Convert Has Info");
            if (value == null || !(value is TabInfoInterface))
                return false;
            else
                return (value as TabInfoInterface).HasInfo;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
