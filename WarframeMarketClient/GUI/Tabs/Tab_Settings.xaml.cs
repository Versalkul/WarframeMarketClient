using Hardcodet.Wpf.TaskbarNotification;
using System.Windows.Controls;
using System.Drawing;
using System;
using System.Windows;

namespace WarframeMarketClient.GUI.Tabs
{
    /// <summary>
    /// Interaktionslogik für Tab_Settings.xaml
    /// </summary>
    public partial class Tab_Settings : UserControl
    {

        public Tab_Settings()
        {
            InitializeComponent();
          
        }

        private void minimizeBox_Click(object sender, RoutedEventArgs e)
        {
            SharedValues.toTray = (bool)minimizeBox.IsChecked;
        }
    }
}
