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

        private SaveLoadFile saver = new SaveLoadFile();

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

     



        private string choosenSoundFile="";

        public string ChoosenSoundFile
        {
            get { return choosenSoundFile; }
            set { choosenSoundFile= value; OnPropertyChanged(nameof(ChoosenSoundFile)); SaveSettings();  ApplicationState.Plimper?.UpdateSound(); if (loading) return; ApplicationState.Plimper.MessageReceived(); }
        }


        private List<string> availableSounds= new List<string>();
        public List<string> AvailableSounds { get { if (!availableSounds.Any())
                {
                    List<string> names = saver.GetSounds();
                    availableSounds = names;
                }
                    return availableSounds; } }

        private double volume=0.5;

        public double Volume
        {
            get { return volume; }
            set { volume = value;ApplicationState.getInstance().Plimper?.SetVolume(value); OnPropertyChanged(nameof(ChoosenSoundFile)); SaveSettings(); }
        }


        private bool updateAvailable;

        public bool UpdateAvailable
        {
            get { return updateAvailable; }
            set { updateAvailable = value; OnPropertyChanged(nameof(UpdateAvailable)); }
        }

        #endregion

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
            saver.saveString(nameof(ChoosenSoundFile),ChoosenSoundFile);
            saver.saveDouble(nameof(Volume), Volume);
            saver.Save();
        }

        public void LoadSettings()
        {
            loading = true;
            SaveLoadFile loader = new SaveLoadFile();
            if (loader.FileExists())
            {
                loader.Read();
                Autostart = loader.isAutostart();
                if (Autostart) loader.updateAutostart();
                ToTray = loader.loadBool(nameof(ToTray));
                LimitAutoComplete = loader.loadBool(nameof(LimitAutoComplete));
                DefaultOnline = loader.loadBool(nameof(DefaultOnline));
                ChoosenSoundFile = loader.loadString(nameof(ChoosenSoundFile));
                Volume = loader.loadDouble(nameof(Volume));
                ApplicationState.getInstance().SessionToken = loader.loadString("Token");
            }
            if (!AvailableSounds.Contains(ChoosenSoundFile) & AvailableSounds.Any()) ChoosenSoundFile = AvailableSounds.First();
            
            loading = false;

        }

        public bool ImportSound(string path)
        {



            if (saver.ImportSound(path))
            {
                List<string> names = saver.GetSounds();
                availableSounds = names;
                OnPropertyChanged(nameof(AvailableSounds));
                return true;  
            }
            return false;

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
