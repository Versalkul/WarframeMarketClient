using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarframeMarketClient.Model
{
    public class User
    {

        public User(string name)
        {
            Name = name;
            State = OnlineState.OFFLINE;
        }

        public String Name { get; set; }

        public OnlineState State { get; set; }
    }
}
