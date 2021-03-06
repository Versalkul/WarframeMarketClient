﻿using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Data;
using System.Collections.Generic;

namespace WarframeMarketClient.GUI.Converter
{
    class FromMeToColumnConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int[] val = (parameter is string) ?
                (new List<string>((parameter as string).Split(','))).Select(int.Parse).ToArray()
                : new int[] {1,2};
            bool? IsFromMe = value as bool?;
            if (IsFromMe != null && IsFromMe == true)
                return val[1];
            return val[0];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
