using Hardcodet.Wpf.TaskbarNotification;
using System.Windows.Controls;
using System.Drawing;
using System;
using System.Windows;
using WarframeMarketClient.Model;
using System.Windows.Input;
using WarframeMarketClient.Logic;

namespace WarframeMarketClient.GUI.Tabs
{
    /// <summary>
    /// Interaktionslogik für Tab_Settings.xaml
    /// </summary>
    public partial class Tab_Settings : UserControl
    {



        public string SessionTokenInput { get; set; }
        private bool autostart;

        public bool Autostart
        {
            get { return autostart; }
            set { autostart = value;SaveSettings();
            }
        }

        private bool toTray;

        public bool ToTray
        {
            get { return toTray; }
            set { toTray = value;SaveSettings(); }
        }



        public bool DefaultOnline {
            get {
                return ApplicationState.getInstance().DefaultState == OnlineState.ONLINE;
            }
            set {
                ApplicationState.getInstance().DefaultState = value ? OnlineState.ONLINE : OnlineState.OFFLINE;
                SaveSettings();
                if (value)
                {
                    if (ApplicationState.getInstance().OnlineState == OnlineState.OFFLINE)
                        ApplicationState.getInstance().OnlineState = OnlineState.ONLINE;
                }

                else
                {
                    if (ApplicationState.getInstance().OnlineState == OnlineState.ONLINE)
                        ApplicationState.getInstance().OnlineState = OnlineState.OFFLINE;
                }

            }
        }



        public ApplicationState ApplicationState { get { return ApplicationState.getInstance(); } }



        public Tab_Settings()
        {
            InitializeComponent();
            this.DataContext = this;
            SaveLoadFile loader = new SaveLoadFile();
            if (loader.FileExists())
            {
                loader.Read();
                Autostart = loader.isAutostart();
                DefaultOnline = loader.loadBool("DefaultOnline");
                ToTray = loader.loadBool("ToTray");
                SessionTokenInput = loader.loadString("Token");
                SetToken();
            }
            
            // minimize if toTray

        }

        private void SetToken()
        {

            if (ApplicationState.getInstance().IsValid) ApplicationState.getInstance().Market.Dispose();
            Console.WriteLine($"new token is {SessionTokenInput}");
            ApplicationState.getInstance().SessionToken = SessionTokenInput;
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            SetToken();
            SaveSettings();
        }

        private void tokenBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                SetToken();

            }
        }

        private void SaveSettings()
        {
            if (!ApplicationState.HasValidInstance) return;
            SaveLoadFile saver = new SaveLoadFile();
            saver.saveString("Token", ApplicationState.getInstance().SessionToken);
            saver.autostart(Autostart);
            saver.saveBool("ToTray", ToTray);
            saver.saveBool("DefaultOnline",DefaultOnline);
            saver.Save();
            
        }


    }
}
