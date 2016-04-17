using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarframeMarketClient.Logic
{

    class PmRequest
    {
        public int code;
        public List<PmJson> response;
    }

    class PmJson
    {
        public string message;
        public string message_from;
        public string send_hour;
        public string send_minute;
    }

    public class PmData
    {
        public string message;
        public string time;
        public bool fromMe;
    }
    
}
