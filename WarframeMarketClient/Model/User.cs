using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarframeMarketClient.Model
{
    public class User: INotifyPropertyChanged
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
            get { if((DateTime.Now - lastCheck).Minutes<1)ApplicationState.getInstance().asynchAssign(() => State = ApplicationState.getInstance().Market.getStatusOnSite(Name)); lastCheck = DateTime.Now; return state; }
            set { OnlineState oldstate = state; ; state = value; if(oldstate!=state) OnPropertyChanged(nameof(State)); }
        }



        #region OnPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string property)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(property));

        }
        #endregion

    }
}
