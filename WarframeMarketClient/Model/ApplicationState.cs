using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarframeMarketClient.Model
{
    class ApplicationState
    {
        #region Singleton

        private static ApplicationState instance;

        public static ApplicationState getInstance()
        {
            if (instance == null)
                instance = new ApplicationState();
            return instance;
        }

        private ApplicationState()
        {

        }

        #endregion


        #region Properties


        public string SessionToken { get; set; }

        public string Username { get { return "A"; } set { } }

        public OnlineState OnlineState { get; set; }


        #endregion

    }
}
