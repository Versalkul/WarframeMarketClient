using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WarframeMarketClient.GUI.AttachedProperties
{
    /// <summary>
    /// This class contains a few useful extenders for the ListBox
    /// </summary>
    /// <remarks>
    /// Using Code from https://michlg.wordpress.com/2010/01/16/listbox-automatically-scroll-currentitem-into-view/
    /// and http://stackoverflow.com/a/24241299/603268
    /// </remarks>
    public class ListBoxExtenders : DependencyObject
    {
        #region Properties

        public static readonly DependencyProperty AutoScrollToSelectedItemProperty = DependencyProperty.RegisterAttached("AutoScrollToSelectedItem", typeof(bool), typeof(ListBoxExtenders), new UIPropertyMetadata(default(bool), OnAutoScrollToSelectedItemChanged));

        /// <summary>
        /// Returns the value of the AutoScrollToSelectedItemProperty
        /// </summary>
        /// <param name="obj">The dependency-object whichs value should be returned</param>
        /// <returns>The value of the given property</returns>
        public static bool GetAutoScrollToSelectedItem(DependencyObject obj)
        {
            return (bool)obj.GetValue(AutoScrollToSelectedItemProperty);
        }

        /// <summary>
        /// Sets the value of the AutoScrollToSelectedItemProperty
        /// </summary>
        /// <param name="obj">The dependency-object whichs value should be set</param>
        /// <param name="value">The value which should be assigned to the AutoScrollToSelectedItemProperty</param>
        public static void SetAutoScrollToSelectedItem(DependencyObject obj, bool value)
        {
            obj.SetValue(AutoScrollToSelectedItemProperty, value);
        }

        #endregion

        #region Events

        /// <summary>
        /// This method will be called when the AutoScrollToSelectedItem
        /// property was changed
        /// </summary>
        /// <param name="s">The sender (the ListBox)</param>
        /// <param name="e">Some additional information</param>
        public static void OnAutoScrollToSelectedItemChanged(DependencyObject s, DependencyPropertyChangedEventArgs e)
        {
            var listBox = s as ListBox;
            if (listBox != null)
            {
                var newValue = (bool)e.NewValue;
                
                if (newValue)
                    listBox.SelectionChanged += OnAutoScrollToSelectedItem;
                else
                    listBox.SelectionChanged -= OnAutoScrollToSelectedItem;
            }
        }


        /// <summary>
        /// This method will be called when the ListBox should
        /// be scrolled to the given index
        /// </summary>
        /// <param name="listBox">The ListBox which should be scrolled</param>
        /// <param name="index">The index of the item to which it should be scrolled</param>
        public static void OnAutoScrollToSelectedItem(object sender, SelectionChangedEventArgs e)
        {
            ListBox listBox = sender as ListBox;
            
            if (listBox != null && listBox.Items != null && listBox.Items.Count > listBox.SelectedIndex && listBox.SelectedIndex >= 0)
                listBox.ScrollIntoView(listBox.Items[listBox.SelectedIndex]);
        }

        #endregion
    }
}
