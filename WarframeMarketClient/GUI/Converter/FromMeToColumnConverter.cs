using System;
using System.Globalization;
using System.Windows.Data;

namespace WarframeMarketClient.GUI.Converter
{
    class FromMeToColumnConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool? IsFromMe = value as bool?;
            if (IsFromMe != null && IsFromMe == true)
                return 2;
            return 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
