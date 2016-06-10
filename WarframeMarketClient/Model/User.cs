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
        

        public User(string name)
        {
            Name = name;
        }

        public string Name { get; set; }

        private OnlineState state=OnlineState.OFFLINE;

        public OnlineState State
        {
            get {  return state; }
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
