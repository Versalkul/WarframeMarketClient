using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarframeMarketClient.Model
{
    public class SendMessageJson
    {
        public readonly string destination = "user.send_message";
        public SendMessageData data;

        public SendMessageJson(string message,string sendTo)
        {
            data = new SendMessageData(message,sendTo);
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

    }

    public class SendMessageData
    {
       public string text;
       public string message_to;

       public SendMessageData(string text, string to)
        {
            this.text = text.Replace(System.Environment.NewLine, "<br>");
            message_to = to;
        }


    }
}
