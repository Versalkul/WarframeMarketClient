﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarframeMarketClient.Model
{
    public class ApplicationState
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
            Username = "";
            SessionToken = "";
            OnlineState = OnlineState.OFFLINE;
        }

        #endregion


        #region Properties


        public string SessionToken { get; set; }

        public string Username { get; set; }

        public OnlineState OnlineState { get; set; }

        public OnlineState AfkState { get; set; }
        
        public OnlineState DefaultState { get; set; }
        #endregion

    }
}
