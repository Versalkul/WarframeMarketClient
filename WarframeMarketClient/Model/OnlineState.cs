﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarframeMarketClient.Model
{
    public enum OnlineState {
        ERROR,
        VALIDATING,
        OFFLINE,
        ONLINE,
        INGAME,
        DISABLED
    }
}
