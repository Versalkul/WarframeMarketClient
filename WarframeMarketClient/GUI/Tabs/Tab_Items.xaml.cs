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


        public string DecreaseItemText
        {
            get { return (string)GetValue(DecreaseItemTextProperty); }
            set { SetValue(DecreaseItemTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DecreaseItemText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DecreaseItemTextProperty =
            DependencyProperty.Register("DecreaseItemText", typeof(string), typeof(Tab_Items), new PropertyMetadata(null));





        public Tab_Items()
        {
            InitializeComponent();
        }


        private void Decrease(object sender, RoutedEventArgs e)
        {
            ((sender as Button).DataContext as WarframeItem).DecreaseCount();
        }
        private void Remove(object sender, RoutedEventArgs e)
        {
            ((sender as Button).DataContext as WarframeItem).RemoveItem();
        }

    }

}
