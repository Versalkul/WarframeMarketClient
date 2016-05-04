using System.Windows;
using System.Windows.Controls;

namespace WarframeMarketClient.GUI
{
    /// <summary>
    /// Extension for a BelowTabsContent
    /// </summary>

    public class FancyTabControl : TabControl
    {
        static FancyTabControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(FancyTabControl),
                new FrameworkPropertyMetadata(typeof(FancyTabControl)));
        }


        public object BelowTabsContent
        {
            get { return (object)GetValue(BelowTabsContentProperty); }
            set { SetValue(BelowTabsContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for belowTabContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BelowTabsContentProperty =
            DependencyProperty.Register("BelowTabsContent", typeof(object), typeof(FancyTabControl), new PropertyMetadata(null));



    }
}
