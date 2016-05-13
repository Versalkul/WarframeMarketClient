using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarframeMarketClient.Model;

namespace WarframeMarketClient.ViewModel
{
    public abstract class ChatTabContentViewModel : TabInfoInterface, INotifyPropertyChanged
    {
        public abstract string DisplayName { get; }
        public virtual OnlineState? OnlineStateInfo { get; }


        protected bool hasInfo = true;

        public bool HasInfo
        {
            get { return hasInfo; }
            set
            {
                hasInfo = value;
                OnPropertyChanged("HasInfo");
            }
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
