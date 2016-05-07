using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using WarframeMarketClient.Model;

namespace WarframeMarketClient.GUI.Tabs
{
    /// <summary>
    /// Interaktionslogik für Tab_Items.xaml
    /// </summary>
    public partial class Tab_Items : UserControl
    {
        public ObservableCollection<WarframeItem> Items
        {
            get { return (ObservableCollection<WarframeItem>)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }


        // Using a DependencyProperty as the backing store for Items.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register("Items", typeof(ObservableCollection<WarframeItem>), typeof(Tab_Items), new PropertyMetadata(null));



        public Tab_Items()
        {
            InitializeComponent();
        }
        
    }

}
