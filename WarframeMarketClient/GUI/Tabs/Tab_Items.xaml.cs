using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WarframeMarketClient.Model;
using DataGridCellInfo = Microsoft.Windows.Controls.DataGridCellInfo;

namespace WarframeMarketClient.GUI.Tabs
{
    /// <summary>
    /// Code behind for Tab_Items.xaml
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

        /// <summary>
        /// Text shown on the Button that decreases the item count ( Sold / Bought )
        /// </summary>
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
        private static readonly int AutoCItemMax = 7;

        private int autoCItemCount = 0;

        /// <summary>
        /// Filters only first <AutoCItemMax> Items that fit the input
        /// </summary>
        public AutoCompleteFilterPredicate<Object> AutoCItemFilter { get
            {
                return new AutoCompleteFilterPredicate<object>((s, e) =>
                {
                    return (e as string).ToLower().Contains(s.ToLower()) && (
                        !ApplicationState.getInstance().Settings.LimitAutoComplete
                        || autoCItemCount++ < AutoCItemMax);
                });
            }
        }

        private void AutoCItemPopulating(object sender, RoutedEventArgs e)
        {
            autoCItemCount = 0;
        }
        #endregion

        #region View Behaviour
        /// <summary>
        /// Sets the focus to the correct element on start edit
        /// </summary>
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

        /// <summary>
        /// Start Editing on Enter ( for Keyboard Navigability )
        /// </summary>
        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && ItemList.CurrentCell != null && ItemList.CurrentCell.Column != null) // Block all Enter keys; User has to use Arrow Keys for navigation
            {
                if (!ItemList.CurrentCell.Column.IsReadOnly) // Only start Editing in editable columns
                    ItemList.BeginEdit();
                e.Handled = true;
            }
        }
        #endregion


        public Tab_Items()
        {
            InitializeComponent();
            Items = new ObservableCollection<WarframeItem>();
        }


        #region Button Events
        private void Add(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Add! : "+ (sender as Button).DataContext);
            if ((sender as Button).DataContext is WarframeItem) // Add Button should save edits
            {
                WarframeItem item = ((sender as Button).DataContext as WarframeItem);
                item.IsSellOffer = TabName == "Sell";
                ItemList.CommitEdit();
                item.CommitAdd();
            }
            else // Add Button should start editing
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


        private void Save(object sender, RoutedEventArgs e)
        {
            ItemList.CommitEdit();
            ((sender as Button).DataContext as WarframeItem).CommitEdit();
        }

        #endregion


    }

}
