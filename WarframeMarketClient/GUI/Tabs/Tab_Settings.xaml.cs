using Hardcodet.Wpf.TaskbarNotification;
using System.Windows.Controls;
using System.Drawing;
using System;
using System.Windows;
using WarframeMarketClient.Model;
using System.Windows.Input;
using WarframeMarketClient.Logic;
using System.ComponentModel;
using Microsoft.Win32;
using System.Windows.Media;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using WarframeMarketClient.Extensions;

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
            soundBox.SelectedIndex = ApplicationState.Settings.AvailableSounds.IndexOf(ApplicationState.Settings.ChoosenSoundFile);
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

        private void importSound_Click(object sender, RoutedEventArgs e)
        {


                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Multiselect = false;
                bool? dialogResult = dialog.ShowDialog();
                if (dialogResult == true && ApplicationState.Settings.ImportSound(dialog.FileName))
                {
                    Console.WriteLine("New Sound Loaded");
                }
               
        }

        private void updateButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/Versalkul/WarframeMarketClient/releases/latest");

        }

        private void openFolder_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(AppDomain.CurrentDomain.BaseDirectory);
        }

        private async void removeProgramm_Click(object sender, RoutedEventArgs e)
        {
            MetroWindow window = (MetroWindow)Application.Current.MainWindow;
            MessageDialogResult result = await window.ShowMessageAsync("Remove Files", "This will remove all the files the program saved in Appdata\\Roaming and close the program.\nIf you want the Application itself removed just delete the exe afterwards.\nDo you really want to remove all traces?", MessageDialogStyle.AffirmativeAndNegative);

            if (result == MessageDialogResult.Affirmative)
            {

                MessageDialogResult invalidResult = await window.ShowMessageAsync("Invalidate session cookie", "Do you also want to invalidate your session cookie ?\nThis may also log you out in the browser you used to get your session cookie.", MessageDialogStyle.AffirmativeAndNegative);

                if (invalidResult == MessageDialogResult.Affirmative) ApplicationState.Market?.InvalidateCookie();
                ApplicationState.SessionToken = "";
                ApplicationState.Market?.Dispose();
                ApplicationState.Plimper.Dispose();
                ApplicationState.Logger.Dispose();
                new SaveLoadFile().RemoveRegEntry();
                (new System.IO.DirectoryInfo(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WarframeMarketClient"))).Remove();
                Application.Current.Shutdown();
            }
        }
    }
}
