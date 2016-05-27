using System;
using System.Collections.ObjectModel;
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
        #region Properties
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

        public string TabName { get; set; }


        #endregion

        #region Autocompletion Behaviour
        private int AutoCItemCount = 0;
        public AutoCompleteFilterPredicate<Object> AutoCItemFilter { get
            {
                return new AutoCompleteFilterPredicate<object>((s, e) =>
                {
                    // returns only 5 items
                    return (e as string).ToLower().Contains(s.ToLower()) && (AutoCItemCount++ < 7||!ApplicationState.getInstance().Settings.LimitAutoComplete);
                });
            }
        }

        private void AutoCItemPopulating(object sender, RoutedEventArgs e)
        {
            AutoCItemCount = 0;
        }
        #endregion

        #region View Behaviour
        private void EditView_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is UIElement)
                (sender as UIElement).Focus();

            if (sender is AutoCompleteBox) // Yes, that's necessary :(
            {
                TextBox textbox = (sender as AutoCompleteBox).Template.FindName("Text", sender as AutoCompleteBox) as TextBox;
                if (textbox != null) textbox.Focus();
            }
        }
        #endregion


        public Tab_Items()
        {
            InitializeComponent();
            Items = new ObservableCollection<WarframeItem>() {
                new WarframeItem("Test", 1, 2, true)
            };
        }


        #region Button Events
        private void Add(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Add! : "+ (sender as Button).DataContext);
            if ((sender as Button).DataContext is WarframeItem)
            {
                WarframeItem item = ((sender as Button).DataContext as WarframeItem);
                item.SellOffer = TabName == "Sell";
                ItemList.CommitEdit();
                item.CommitAdd();
            }
            else
            {
                ItemList.CurrentCell = new DataGridCellInfo(ItemList.CurrentCell.Item, ItemList.Columns[1]);
                //ItemList.BeginEdit();
            }
        }
        private void Decrease(object sender, RoutedEventArgs e)
        {
            //ItemList.CancelEdit();
            ((sender as Button).DataContext as WarframeItem).DecreaseCount();
        }
        private void Remove(object sender, RoutedEventArgs e)
        {
            ItemList.CancelEdit();
            ((sender as Button).DataContext as WarframeItem).RemoveItem();
        }
        #endregion
    }

}
