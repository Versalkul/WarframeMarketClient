using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using WarframeMarketClient.Model;

namespace WarframeMarketClient.ViewModel
{
    public class SoundViewModel
    {
        private MediaPlayer PlimPlayer = new MediaPlayer();
        private string folderPath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WarframeMarketClient"), "Sounds");

        public SoundViewModel()
        {
            PlimPlayer.Open(new Uri(Path.Combine(folderPath, ApplicationState.getInstance().Settings.ChoosenSoundFile)));
            PlimPlayer.Volume = ApplicationState.getInstance().Settings.Volume; 
        }

        public void MessageReceived()
        {
            PlimPlayer.Position = new TimeSpan(0);
            PlimPlayer.Play();
        }

        public void UpdateSound()
        {
            if (!PlimPlayer.Source.OriginalString.Contains(ApplicationState.getInstance().Settings.ChoosenSoundFile)) PlimPlayer.Open(new Uri(Path.Combine(folderPath, ApplicationState.getInstance().Settings.ChoosenSoundFile)));
        }

        public void SetVolume(double vol)
        {
            PlimPlayer.Volume = vol;
        }

    }
}
