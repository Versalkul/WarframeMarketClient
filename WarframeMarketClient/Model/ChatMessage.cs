using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarframeMarketClient.Model
{
    public class ChatMessage: IEquatable<ChatMessage>
    {

        public ChatMessage()
        {
            Time = new DateTime(1000, 6, 12);
        }

        #region Properties

        #region JSON

        private string message;
        private string messageFrom;
        // private int sendHour;
        // private int sendMinute;



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
            get { return ""+Time.Hour; }
            set {Time = Time.AddHours(Convert.ToInt32(value));}
        }

        [JsonProperty(PropertyName = "send_minute")]
        public string SendMinute
        {
            get { return ""+Time.Minute; }
            set { Time = Time.AddMinutes(Convert.ToInt32(value)); }
        }

        #endregion

        #region Dynamic

        [JsonIgnoreAttribute]
        public DateTime Time { get; set; }

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
                return String.Format("{0:D2}:{1:D2}", Time.Hour, Time.Minute);
            }
        }


        #endregion

        #endregion

        public bool Equals(ChatMessage other)
        {
            return other.Message == Message && other.MessageFrom == MessageFrom && Math.Abs(Time.Minute - other.Time.Minute) <= 1 && Math.Abs(Time.Hour - other.Time.Hour) <= 1; // may not be working yet
        }


    }
}
