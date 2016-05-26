using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarframeMarketClient.Logic;
using WarframeMarketClient.Model;

namespace WarframeMarketClient.ViewModel
{
    public class Settings: INotifyPropertyChanged
    {

        #region properties

        private bool loading = false;
        private bool init = true;

        private bool toTray;

        public bool ToTray
        {
            get { return toTray; }
            set { toTray = value; OnPropertyChanged(nameof(ToTray));SaveSettings(); }
        }

        private bool defaultOnline;

        public bool DefaultOnline
        {
            get { return defaultOnline; }
            set { defaultOnline = value; OnPropertyChanged(nameof(DefaultOnline)); SaveSettings(); ApplicationState.DefaultState = DefaultOnline ? OnlineState.ONLINE : OnlineState.OFFLINE; }
        }


        private bool autostart;

        public bool Autostart
        {
            get { return autostart; }
            set { autostart = value; OnPropertyChanged(nameof(Autostart)); SaveSettings(); }
        }


        private bool limitAutoComplete;

        public bool LimitAutoComplete
        {
            get { return limitAutoComplete; }
            set { limitAutoComplete = value; OnPropertyChanged(nameof(LimitAutoComplete)); SaveSettings(); }
        }

        public ApplicationState ApplicationState { get { return ApplicationState.getInstance(); } }

        #endregion

        private string choosenSoundFile="NewMessage.wav";

        public string ChoosenSoundFile
        {
            get { return choosenSoundFile; }
            set { choosenSoundFile = value; }
        }


        public void SaveSettings() // also SettingsChanged
        {
            if (init)ApplicationState.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(ApplPropChanged);
            init = false;
            if (loading) return;
            SaveLoadFile saver = new SaveLoadFile();
            saver.autostart(Autostart);
            saver.saveBool(nameof(ToTray),ToTray);
            saver.saveBool(nameof(LimitAutoComplete), LimitAutoComplete);
            saver.saveBool(nameof(DefaultOnline), DefaultOnline);
            saver.saveString("Token",ApplicationState.getInstance().SessionToken);
            saver.Save();
        }

        public void LoadSettings()
        {
            loading = true;
            SaveLoadFile loader = new SaveLoadFile();
            loader.Read();
            Autostart = loader.isAutostart();
            if (Autostart) loader.updateAutostart();
            ToTray = loader.loadBool(nameof(ToTray));
            LimitAutoComplete = loader.loadBool(nameof(LimitAutoComplete));
            DefaultOnline = loader.loadBool(nameof(DefaultOnline));
            ApplicationState.getInstance().SessionToken = loader.loadString("Token");
            loading = false;

        }


        #region OnPropertyChanged

        private void ApplPropChanged(object o, EventArgs args)
        {
            OnPropertyChanged(nameof(ApplicationState));
        }

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
