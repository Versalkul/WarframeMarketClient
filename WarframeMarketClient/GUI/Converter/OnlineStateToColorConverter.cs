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
        private static Dictionary<String, Dictionary<OnlineState, Color>> _colors = new Dictionary<String, Dictionary<OnlineState, Color>>
        {
            { "normal", new Dictionary<OnlineState, Color>{
                { OnlineState.INGAME,   Color.FromRgb(204, 179, 255)},
                { OnlineState.ONLINE,   Color.FromRgb(179, 255, 179)},
                { OnlineState.OFFLINE,  Color.FromRgb(255, 179, 179)},
                { OnlineState.DISABLED, Color.FromRgb(204, 143, 143)},
            } },
            { "strong", new Dictionary<OnlineState, Color>{
                { OnlineState.INGAME,   Color.FromRgb(140,  84, 255)},
                { OnlineState.ONLINE,   Color.FromRgb(  0, 100,   0)},
                { OnlineState.OFFLINE,  Color.FromRgb(139,   0,   0)},
                { OnlineState.DISABLED, Color.FromRgb(130,  60,  60)},
            } }
        };


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            OnlineState? state = value as OnlineState?;
            String colorType = parameter == null ? "normal" : parameter as String;
            if (state != null && _colors.ContainsKey(colorType) && _colors[colorType].ContainsKey((OnlineState)state))
                return new SolidColorBrush(_colors[colorType][(OnlineState)state]);
            return new SolidColorBrush(Color.FromRgb(200,200,200));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
