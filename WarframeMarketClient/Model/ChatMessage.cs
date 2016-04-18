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
        private string sendHour;
        private string sendMinute;

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
        public string SendHour
        {
            get { return sendHour; }
            set { sendHour = value; }
        }

        [JsonProperty(PropertyName = "send_minute")]
        public string SendMinute
        {
            get { return sendMinute; }
            set { sendMinute = value; }
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
                return SendHour + " : " + SendMinute;
            }
        }

        #endregion

        #endregion
    }
}
