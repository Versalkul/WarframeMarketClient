using System.Windows.Controls;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Data;

namespace WarframeMarketClient.GUI.Converter
{
    class ErrorPropertiesAndNameToHasErrorConverter : IMultiValueConverter
    {
        /// <summary>
        /// Checks if the Propertie corresponding to the column of the given cell has an error
        /// </summary>
        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value[0] is ObservableCollection<string>) && (value[1] is DataGridCell) &&
                ((value[1] as DataGridCell).Column != null) && ((value[1] as DataGridCell).Column.SortMemberPath != null) &&
                (value[0] as ObservableCollection<string>).Contains((value[1] as DataGridCell).Column.SortMemberPath);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
