using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarframeMarketClient.Model
{
    public class User
    {

        DateTime lastCheck = DateTime.MinValue;

        public User(string name)
        {
            Name = name;
            //State = OnlineState.OFFLINE;
        }

        public String Name { get; set; }

        private OnlineState state;

        public OnlineState State
        {
            get { if((DateTime.Now - lastCheck).Minutes<1)ApplicationState.getInstance().asynchAssign(() => State = ApplicationState.getInstance().Market.getStatusOnSite(Name)); return state; }
            set { state = value; }
        }

        
    }
}
