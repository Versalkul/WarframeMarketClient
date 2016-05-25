using Hardcodet.Wpf.TaskbarNotification;
using System.Windows.Controls;
using System.Drawing;
using System;
using System.Windows;
using WarframeMarketClient.Model;
using System.Windows.Input;
using WarframeMarketClient.Logic;
using System.ComponentModel;

namespace WarframeMarketClient.GUI.Tabs
{
    /// <summary>
    /// Interaktionslogik für Tab_Settings.xaml
    /// </summary>
    public partial class Tab_Settings : UserControl,INotifyPropertyChanged
    {



        public string SessionTokenInput { get; set; }
        

        public ApplicationState ApplicationState { get { return ApplicationState.getInstance(); } }



        public Tab_Settings()
        {
            InitializeComponent();
            this.DataContext = ApplicationState.Settings;
            ApplicationState.Settings.LoadSettings();
            SessionTokenInput = ApplicationState.SessionToken;
            OnPropertyChanged(nameof(SessionTokenInput));

        }



        private void SetToken()
        {

            ApplicationState.SessionToken = SessionTokenInput;
            
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            SetToken();
            ApplicationState.Settings.SaveSettings();
        }

        private void tokenBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                SetToken();
                ApplicationState.Settings.SaveSettings();
            }
        }

 

        #region OnPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string property)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(property));

        }
        #endregion


    }
}
