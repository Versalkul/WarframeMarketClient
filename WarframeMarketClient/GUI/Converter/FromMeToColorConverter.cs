using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace WarframeMarketClient.GUI.Converter
{
    class FromMeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool? IsFromMe = value as bool?;
            if (IsFromMe != null && IsFromMe == true)
                return new SolidColorBrush(Color.FromRgb(230, 245, 255));
            return new SolidColorBrush(Color.FromRgb(240, 240, 240));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
