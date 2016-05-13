using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarframeMarketClient.Model
{
    public class ChatMessage
    {

        #region Properties

        #region JSON

        private string message;
        private string messageFrom;
        private int sendHour;
        private int sendMinute;

        [JsonProperty(PropertyName = "message")]
        public string Message
        {
            get { return message; }
            set { message = value; }
        }

        [JsonProperty(PropertyName = "message_from")]
        public string MessageFrom
        {
            get { return messageFrom; }
            set { messageFrom = value; }
        }

        [JsonProperty(PropertyName = "send_hour")]
        public String SendHour
        {
            get { return ""+sendHour; }
            set { sendHour = Convert.ToInt32(value); }
        }

        [JsonProperty(PropertyName = "send_minute")]
        public string SendMinute
        {
            get { return ""+sendMinute; }
            set { sendMinute = Convert.ToInt32(value); }
        }

        #endregion

        #region Dynamic

        [JsonIgnoreAttribute]
        public bool IsFromMe
        {
            get
            {
                return MessageFrom == ApplicationState.getInstance().Username;
            }
        }


        [JsonIgnoreAttribute]
        public string TimeString
        {
            get
            {
                return String.Format("{0:D2} : {1:D2}", sendHour, sendMinute);
            }
        }

        #endregion

        #endregion
    }
}
