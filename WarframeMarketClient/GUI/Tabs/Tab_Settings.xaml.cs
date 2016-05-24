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

            ApplicationState.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(ApplPropChanged);
        }

        private void ApplPropChanged(object o, EventArgs args)
        {
            OnPropertyChanged(nameof(ApplicationState));
        }

        private void SetToken()
        {

            if (ApplicationState.getInstance().IsValid) ApplicationState.getInstance().Market.Dispose();
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
                SaveSettings();
            }
        }

        private void SaveSettings()
        {
            if (!ApplicationState.getInstance().HasUsername) return;
            SaveLoadFile saver = new SaveLoadFile();
            saver.autostart(Autostart);
            saver.saveBool("ToTray", ToTray);
            saver.saveBool("DefaultOnline",DefaultOnline);
            if(ApplicationState.getInstance().SessionToken.Length>10) saver.saveString("Token", ApplicationState.getInstance().SessionToken);
            saver.Save();
            
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
