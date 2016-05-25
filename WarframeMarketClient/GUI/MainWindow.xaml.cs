using System;
using System.Windows;
using MahApps.Metro.Controls;
using WarframeMarketClient.Model;

namespace WarframeMarketClient.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            this.DataContext = ApplicationState.getInstance();
            InitializeComponent();
        }


        private void MainButtonClick(object sender, RoutedEventArgs args)
        {
            ApplicationState.getInstance().OnlineChecker.changeRunning();
        }


        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized && this.IsMouseOver && ApplicationState.getInstance().Settings.ToTray)
            {
                Hide();
                TrayIcon.Visibility = Visibility.Visible;
            }
            else
                TrayIcon.Visibility = Visibility.Hidden;
        }

        private void onTrayClick(object o, RoutedEventArgs args)
        {
            Show();
            Activate();
            WindowState = WindowState.Normal;
        }
    }
}
