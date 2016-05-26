using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarframeMarketClient.Model;

namespace WarframeMarketClient.Extensions
{
    static class OnlineStateFunctions
    {

        public static bool IsOnline(this OnlineState state)
        {
            return state == OnlineState.INGAME || state == OnlineState.ONLINE;
        }
    }
}
