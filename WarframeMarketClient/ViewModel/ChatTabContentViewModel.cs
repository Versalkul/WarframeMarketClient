using System.ComponentModel;
using WarframeMarketClient.Model;

namespace WarframeMarketClient.ViewModel
{
    public abstract class ChatTabContentViewModel : TabInfoInterface, INotifyPropertyChanged
    {
        public abstract string DisplayName { get; }
        public virtual OnlineState? OnlineStateInfo { get; }


        protected bool hasInfo = false;
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
