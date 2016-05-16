using System;
using System.Globalization;
using System.Windows.Data;

namespace WarframeMarketClient.GUI.Converter
{
    class ModRankToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value as int?) < 0 ? "" : ""+value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
