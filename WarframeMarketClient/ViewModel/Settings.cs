using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarframeMarketClient.ViewModel
{
    class Settings
    {

        #region properties

        private bool toTray;

        public bool ToTray
        {
            get { return toTray; }
            set { toTray = value; }
        }

        private bool autostart;

        public bool Autostart
        {
            get { return autostart; }
            set { autostart = value; }
        }

        private bool startMinimized;

        public bool StartMinimized
        {
            get { return startMinimized; }
            set { startMinimized = value; }
        }

        private bool limitAutoComplete;

        public bool LimitAutoComplete
        {
            get { return limitAutoComplete; }
            set { limitAutoComplete = value; }
        }

        private int maxAutoComplete;

        public int MaxAutoComplete
        {
            get { return maxAutoComplete; }
            set { maxAutoComplete = value; }
        }



        #endregion


    }
}
