using System;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.ComponentModel;


namespace System.Windows.Controls
{
    /// <summary>
    /// Source: http://www.codeproject.com/Articles/362940/Persist-the-Visual-Tree-when-switching-tabs-in-the
    /// </summary>
    public class TabItemGeneratorBehavior : INotifyPropertyChanged
    {
        //------------------------------------------------------------------- 
        //  Fields
        //-------------------------------------------------------------------

        #region Fields

        /// <summary>
        /// Holds reference to currently selected TabItem
        /// </summary>
        private TabItem _innerSelection;

        /// <summary>
        /// Holds reference to TabControl
        /// </summary>
        private TabControl _tabControl;

        /// <summary>
        /// Refers to Source
        /// </summary>
        private IEnumerable _itemsSource;

        #endregion Fields

        //------------------------------------------------------------------- 
        //  Constructors
        //-------------------------------------------------------------------

        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="tab">Reference to parent <see cref="TabControl"/>.</param>
        private TabItemGeneratorBehavior(TabControl tab)
        {
            if (null == tab)
                throw new ArgumentNullException("Only hosts of type TabControl are supported.");

            _tabControl = tab;
            _tabControl.Loaded += OnTabLoaded;
            _tabControl.SetBinding(TabControl.SelectedItemProperty,
                                        new Binding("SelectedTabItem") { Source = this });
        }

        #endregion Constructors

        //------------------------------------------------------------------- 
        //  Dependency properties
        //-------------------------------------------------------------------

        #region DependencyProperties

        /// <summary>
        /// ItemsSource property
        /// </summary>
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.RegisterAttached("ItemsSource",
                                                typeof(IEnumerable),
                                                typeof(TabItemGeneratorBehavior),
                                                new UIPropertyMetadata(null,
                                                                       OnItemsSourcePropertyChanged));
        /// <summary>
        /// SelectedItem property
        /// </summary>
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.RegisterAttached("SelectedItem",
                                                typeof(object),
                                                typeof(TabItemGeneratorBehavior),
                                                new FrameworkPropertyMetadata(null,
                                                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                                                    OnSelectedItemPropertyChanged));

        #region Dependency Property Getters and Setters

        /// <summary>
        /// Sets value of ItemsSource Dependency property
        /// </summary>
        public static void SetItemsSource(DependencyObject parent, IEnumerable source)
        {
            parent.SetValue(ItemsSourceProperty, source);
        }

        /// <summary>
        /// Gets value of ItemsSource Dependency property
        /// </summary>
        public static object GetItemsSource(DependencyObject tab)
        {
            return tab.GetValue(ItemsSourceProperty);
        }

        /// <summary>
        /// Sets value of SelectedItem Dependency property
        /// </summary>
        public static void SetSelectedItem(DependencyObject tab, object source)
        {
            tab.SetValue(SelectedItemProperty, source);
        }

        /// <summary>
        /// Gets value of SelectedItem Dependency property
        /// </summary>
        public static object GetSelectedItem(DependencyObject tab)
        {
            return tab.GetValue(SelectedItemProperty);
        }

        #endregion

        #endregion DependencyProperties

        //------------------------------------------------------------------- 
        //  Properties
        //-------------------------------------------------------------------

        #region Properties

        /// <summary>
        /// Currently selecter TabItem control
        /// </summary>
        public TabItem SelectedTabItem
        {
            get { return _innerSelection; }
            set
            {
                if (ReferenceEquals(value, _innerSelection))
                    return;

                _innerSelection = value;

                _tabControl.SetValue(TabItemGeneratorBehavior.SelectedItemProperty,
                                          (null == _innerSelection) ? null : _innerSelection.DataContext);
            }
        }

        #endregion  Properties

        //------------------------------------------------------------------- 
        //   Notification Delegates
        //-------------------------------------------------------------------

        #region Dependency Property Notification Delegates

        /// <summary>
        /// Called when ItemsSource had changed
        /// </summary>
        /// <param name="parent"><see cref="TabControl"/> where this behavior is attached to</param>
        /// <param name="e">Change notification</param>
        private static void OnItemsSourcePropertyChanged(DependencyObject parent, DependencyPropertyChangedEventArgs e)
        {
            TabItemGeneratorBehavior instance = GetHandler(parent as TabControl);

            IEnumerable value = e.NewValue as IEnumerable;

            if (ReferenceEquals(instance._itemsSource, value))
                return;

            // Unregister from previous source
            if (null != instance._itemsSource)
            {
                ((INotifyCollectionChanged)instance._itemsSource).CollectionChanged -= instance.OnSourceCollectionChanged;
                //instance._tabControl.Items.Clear();
                instance._itemsSource = null;
            }

            // Check if source exists
            if (null == value)
                return;

            // Register new source
            INotifyCollectionChanged notifyCollectionChanged = value as INotifyCollectionChanged;
            if (null == notifyCollectionChanged)
                return;

            instance._itemsSource = value;
            notifyCollectionChanged.CollectionChanged += instance.OnSourceCollectionChanged;
            instance.SyncItems();
        }

        /// <summary>
        /// Called when SelectedItem had changed
        /// </summary>
        /// <param name="parent"><see cref="TabControl"/> where this behavior is attached to</param>
        /// <param name="e">Change notification</param>
        private static void OnSelectedItemPropertyChanged(DependencyObject parent, DependencyPropertyChangedEventArgs e)
        {
            TabItemGeneratorBehavior instance = GetHandler(parent as TabControl);

            instance._innerSelection = (null == e.NewValue) ? null
                            : instance._tabControl.Items.Cast<TabItem>().FirstOrDefault(t => e.NewValue.Equals(t.DataContext));

            instance.PropertyChanged(instance, new PropertyChangedEventArgs("SelectedTabItem"));
        }

        #endregion  Dependency Property Notification Delegates

        #region TabControl Notification Delegates

        /// <summary>
        /// This handler is being called when <see cref="TabControl"/> has
        /// been loaded.
        /// </summary>
        /// <param name="sender"><see cref="TabControl"/> we are attached to.</param>
        /// <param name="e">Arguments</param>
        protected virtual void OnTabLoaded(object sender, RoutedEventArgs e)
        {
            Debug.Assert(null == _tabControl.ContentTemplate,
                "ContentTemplate should not be assigned" +
                " on TabControl if non virtualizing UI is required.");

            Debug.Assert(null == _tabControl.ItemsSource,
                "ItemsSource should not be assigned" +
                " on TabControl if non virtualizing UI is required.");

            SyncItems();
        }

        #endregion  TabControl Notification Delegates

        #region Source Collection Notification Delegates

        /// <summary>
        /// Source collection change events handler.
        /// </summary>
        /// <param name="sender">Source collection</param>
        /// <param name="e">Change event arguments</param>
        protected virtual void OnSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems)
                        AddTabItem(item);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems)
                        RemoveTabItem(item);
                    break;

                case NotifyCollectionChangedAction.Replace:
                    ReplaceItems(e.NewItems, e.OldItems);
                    break;

                case NotifyCollectionChangedAction.Reset:
                    _tabControl.Items.Clear();
                    break;

                default:
                    // TODO: add other cases
                    throw new NotImplementedException("This operation is not implemented yet.");
            }
        }

        #endregion  Source Collection Notification Delegates

        //------------------------------------------------------------------- 
        //   Methods
        //-------------------------------------------------------------------

        #region Methods

        /// <summary>
        /// Retreives instance  of this class from attached object or creates new.
        /// </summary>
        /// <param name="parent"><see cref="TabControl"/> where we attaching to.</param>
        /// <returns>Returns instance of this class</returns>
        private static TabItemGeneratorBehavior GetHandler(TabControl parent)
        {
            var binding = parent.GetBindingExpression(TabControl.SelectedItemProperty);

            return (null == binding) ? new TabItemGeneratorBehavior(parent)
                                     : binding.DataItem as TabItemGeneratorBehavior;
        }

        /// <summary>
        /// Synchronizes the order of the items, adds missing and removes superfluous
        /// </summary>
        /// <remarks>Modified from the original</remarks>
        private void SyncItems()
        {
            if(_itemsSource == null)
            {
                _tabControl.Items.Clear();
                return;
            }
            Collections.Generic.IEnumerable<Object> iSource = _itemsSource.Cast<Object>();
            Collections.Generic.IEnumerable<TabItem> iTabs = _tabControl.Items.Cast<TabItem>();
            for (int i = 0; i < _tabControl.Items.Count; i++)
            {
                if (!iSource.Contains((_tabControl.Items[i] as TabItem).DataContext))
                {
                    _tabControl.Items.Remove(i);
                    i--;
                }
            }
            foreach (var item in _itemsSource)
            {
                TabItem tab = iTabs.FirstOrDefault(t => t.DataContext == item);
                if(tab == null)
                    AddTabItem(item);
                else
                {
                    _tabControl.Items.Remove(tab);
                    _tabControl.Items.Add(tab);
                }
            }
        }

        /// <summary>
        /// Replaces data in Tabs
        /// </summary>
        /// <param name="newItems">List of new data items.</param>
        /// <param name="oldItems">List of items to be replaced</param>
        private void ReplaceItems(IEnumerable newItems, IEnumerable oldItems)
        {
            IEnumerator newEnum = newItems.GetEnumerator();
            IEnumerator oldEnum = oldItems.GetEnumerator();

            while (newEnum.MoveNext() && oldEnum.MoveNext())
            {
                TabItem tab = _tabControl.Items.Cast<TabItem>().FirstOrDefault(t => t.DataContext == oldEnum.Current);
                if (null == tab)
                    continue;

                tab.DataContext = newEnum.Current;
            }
        }

        /// <summary>
        /// Removes <see cref="TabItem"/> associated with the item
        /// </summary>
        /// <param name="item">Content of the <see cref="TabItems"/></param>
        private void RemoveTabItem(object item)
        {
            TabItem foundItem = _tabControl.Items.Cast<TabItem>().FirstOrDefault(t => t.DataContext == item);

            if (foundItem != null)
                _tabControl.Items.Remove(foundItem);
        }

        /// <summary>
        /// Adds <see cref="TabItem"/> for the content object
        /// </summary>
        /// <param name="item">Content of the <see cref="TabItems"/></param>
        private void AddTabItem(object item)
        {
            ContentControl contentControl = new ContentControl();
            TabItem tab = new TabItem
            {
                DataContext = item,
                Content = contentControl,
                HeaderTemplate = _tabControl.ItemTemplate
            };

            contentControl.SetBinding(ContentControl.ContentProperty, new Binding());
            tab.SetBinding(TabItem.HeaderProperty, new Binding());

            _tabControl.Items.Add(tab);
        }

        #endregion Methods

        //------------------------------------------------------------------- 
        //   INotifyPropertyChanged
        //-------------------------------------------------------------------

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Implementation of <see cref="INotifyPropertyChanged"/>
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        #endregion
    }
}