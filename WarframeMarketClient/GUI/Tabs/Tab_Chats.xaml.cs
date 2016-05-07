using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using WarframeMarketClient.Model;
using WarframeMarketClient.ViewModel;

namespace WarframeMarketClient.GUI.Tabs
{
    /// <summary>
    /// Interaktionslogik für Tab_Chats.xaml
    /// </summary>
    public partial class Tab_Chats : UserControl, TabInfoInterface, INotifyPropertyChanged
    {

        #region Properties


        private ObservableCollection<ChatViewModel> chats;

        public ObservableCollection<ChatViewModel> Chats
        {
            get { return (ObservableCollection<ChatViewModel>)GetValue(ChatsProperty); }
            set { SetValue(ChatsProperty, value);
                value.CollectionChanged += chatUpdated;
                OnPropertyChanged("Tabs");
            }
        }

        // Using a DependencyProperty as the backing store for Chats.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ChatsProperty =
            DependencyProperty.Register("Chats", typeof(ObservableCollection<ChatViewModel>), typeof(Tab_Chats), new PropertyMetadata(null));



        public ReadOnlyObservableCollection<ChatTabContentViewModel> Tabs
        {
            get {
                ObservableCollection<ChatTabContentViewModel> tmp =
                    chats == null ?
                        new ObservableCollection<ChatTabContentViewModel>():
                        new ObservableCollection<ChatTabContentViewModel>(chats);
                tmp.Insert(0, newChat);
                return new ReadOnlyObservableCollection<ChatTabContentViewModel>(tmp);
            }
        }

        protected bool hasInfo = false;

        public bool HasInfo
        {
            get { return hasInfo; }
            private set
            {
                hasInfo = value;
                OnPropertyChanged("HasInfo");
            }
        }

        #endregion


        protected ChatNewViewModel newChat = new ChatNewViewModel();


        public Tab_Chats()
        {
            InitializeComponent();
            this.DataContext = this;
        }


        private void chatUpdated(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("Tabs");
            foreach(ChatViewModel c in Chats)
            {
                c.PropertyChanged -= chatHasInfo;
                c.PropertyChanged += chatHasInfo;
            }
        }
        private void chatHasInfo(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "HasInfo")
                HasInfo = HasInfo || (sender as ChatViewModel).HasInfo;
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
