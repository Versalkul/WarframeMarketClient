using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using WarframeMarketClient.Model;

namespace WarframeMarketClient.GUI.Converter
{
    class TabInfoToHasInfoConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (object value in values)
            {
                if (value == null)
                    continue;
                if ((value is TabInfoInterface) && (value as TabInfoInterface).HasInfo)
                    return true;
                if (value is bool && (value as bool?) == true)
                    return true;
            }
            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
