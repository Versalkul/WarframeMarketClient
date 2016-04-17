using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarframeMarketOnlineController
{
    class PmArgs:EventArgs
    {
        public string message;
        public string fromUser;
        public PmArgs(string message,string from)
        {
            this.message = message;
            this.fromUser = from;
        }
    }
}
