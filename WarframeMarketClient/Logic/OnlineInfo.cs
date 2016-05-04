using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WarframeMarketClient.Logic
{
    class OnlineInfo
    {

        private bool online;
        private bool ingame;
        private string username;
        [JsonProperty(PropertyName = "online_status")]
        public bool Online
        {
            get { return online; }
            set { online = value; }
        }



        [JsonProperty(PropertyName = "online_ingame")]
        public bool Ingame
        {
            get { return ingame; }
            set { ingame = value; }
        }

        [JsonProperty(PropertyName = "ingame_name")]
        public string Username
        {
            get { return username; }
            set { username = value; }
        }
    }
}
