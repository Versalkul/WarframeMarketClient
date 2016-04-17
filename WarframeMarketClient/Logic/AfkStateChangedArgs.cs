using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarframeMarketOnlineController
{
    public enum AfkState { Afk, Available }
    class AfkStateChangedArgs:EventArgs
    {
        public AfkState newAfkState;

        public AfkStateChangedArgs(AfkState newAfkState)
        {
            this.newAfkState = newAfkState;
        }

    }
}
