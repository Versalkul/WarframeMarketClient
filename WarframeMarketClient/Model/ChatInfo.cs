﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarframeMarketClient.Model
{
    class ChatInfo : ChatElement
    {
        public String Text { get; set; }

        public ChatInfo(String text)
        {
            Text = text;
        }

    }
}
