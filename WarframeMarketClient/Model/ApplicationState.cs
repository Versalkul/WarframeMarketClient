using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarframeMarketClient.Logic;

namespace WarframeMarketClient.Model
{
    class ApplicationState
    {
        #region Singleton

        private static ApplicationState instance;
        public MarketManager Market { get; private set; }

        public static ApplicationState getInstance()
        {
            if (instance == null)
                instance = new ApplicationState();
            return instance;
        }

        private ApplicationState()
        {
            Username = "";
            SessionToken = "";
            OnlineState = OnlineState.OFFLINE;
        }

        #endregion



        #region Properties

        public string sessionToken;

        public string SessionToken { get
            {
                
                return sessionToken;
            }
            set
            {
                if (Market != null) Market.Dispose();
                if (value == "") return;
                sessionToken = value;
                Market = new MarketManager();

            }
        }

        public string Username { get; set; }

        public OnlineState OnlineState { get; set; }

        public OnlineState AfkState { get; set; }
        
        public OnlineState DefaultState { get; set; }


        #endregion

    }
}
