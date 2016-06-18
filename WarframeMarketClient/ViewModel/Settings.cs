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
            saver.Autostart(Autostart);
            saver.SaveBool(nameof(ToTray),ToTray);
            saver.SaveBool(nameof(LimitAutoComplete), LimitAutoComplete);
            saver.SaveBool(nameof(DefaultOnline), DefaultOnline);
            saver.SaveString("Token",ApplicationState.getInstance().SessionToken);
            saver.SaveString(nameof(ChoosenSoundFile),ChoosenSoundFile);
            saver.SaveDouble(nameof(Volume), Volume);
            saver.Save();
        }

        public void LoadSettings()
        {
            loading = true;
            SaveLoadFile loader = new SaveLoadFile();
            if (loader.FileExists())
            {
                loader.Read();
                Autostart = loader.IsAutostart();
                if (Autostart) loader.UpdateAutostart();
                ToTray = loader.LoadBool(nameof(ToTray));
                LimitAutoComplete = loader.LoadBool(nameof(LimitAutoComplete));
                DefaultOnline = loader.LoadBool(nameof(DefaultOnline));
                ChoosenSoundFile = loader.LoadString(nameof(ChoosenSoundFile));
                Volume = loader.LoadDouble(nameof(Volume));
                ApplicationState.getInstance().SessionToken = loader.LoadString("Token");
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
