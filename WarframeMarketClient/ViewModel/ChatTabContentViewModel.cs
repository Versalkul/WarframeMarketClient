using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarframeMarketClient.Model;

namespace WarframeMarketClient.ViewModel
{
    public abstract class ChatTabContentViewModel
    {
        public abstract string DisplayName { get; }
        public virtual OnlineState? OnlineStateInfo { get; }
    }
}
