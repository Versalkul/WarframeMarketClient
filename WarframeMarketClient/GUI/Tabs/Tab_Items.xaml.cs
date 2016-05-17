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



        private int AutoCItemCount = 0;
        public AutoCompleteFilterPredicate<Object> AutoCItemFilter { get
            {
                return new AutoCompleteFilterPredicate<object>((s, e) =>
                {
                    // returns only 5 items
                    return (e as string).ToLower().Contains(s.ToLower()) && AutoCItemCount++ < 5;
                });
            }
        }

        private void AutoCItemPopulating(object sender, RoutedEventArgs e)
        {
            AutoCItemCount = 0;
        }



        public Tab_Items()
        {
            InitializeComponent();
        }


        private void Add(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Add! : "+ (sender as Button).DataContext);
            if ((sender as Button).DataContext is WarframeItem)
            {
                ItemList.CommitEdit();
                ((sender as Button).DataContext as WarframeItem).CommitAdd();
            }
            else
            {
                ItemList.CurrentCell = new DataGridCellInfo(ItemList.CurrentCell.Item, ItemList.Columns[1]);
                ItemList.BeginEdit();
            }
        }
        private void Decrease(object sender, RoutedEventArgs e)
        {
            ItemList.CancelEdit();
            ((sender as Button).DataContext as WarframeItem).DecreaseCount();
        }
        private void Remove(object sender, RoutedEventArgs e)
        {
            ItemList.CancelEdit();
            ((sender as Button).DataContext as WarframeItem).RemoveItem();
        }

    }

}
