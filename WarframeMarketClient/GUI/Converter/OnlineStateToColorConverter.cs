using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using WarframeMarketClient.Model;

namespace WarframeMarketClient.GUI.Converter
{
    class OnlineStateToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            OnlineState? state = value as OnlineState?;
            if (state != null)
                switch (state) {
                    case OnlineState.INGAME: return new SolidColorBrush(Color.FromRgb(170, 255, 170));
                    case OnlineState.ONLINE: return new SolidColorBrush(Color.FromRgb(255, 255, 170));
                    case OnlineState.OFFLINE: return new SolidColorBrush(Color.FromRgb(255, 170, 170));
                }
            return new SolidColorBrush(Color.FromRgb(200,200,200));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
