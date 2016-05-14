using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
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

        public ObservableCollection<ChatViewModel> Chats
        {
            get { return (ObservableCollection<ChatViewModel>)GetValue(ChatsProperty); }
            set { SetValue(ChatsProperty, value);
                 chatUpdated(null, null);
            }
        }

        // Using a DependencyProperty as the backing store for Chats.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ChatsProperty =
            DependencyProperty.Register("Chats", typeof(ObservableCollection<ChatViewModel>), typeof(Tab_Chats), new PropertyMetadata(new PropertyChangedCallback(
                (Obj, _) => {
                    (Obj as Tab_Chats).chatUpdated(null, null);
                }
                )));



        public ReadOnlyObservableCollection<ChatTabContentViewModel> Tabs
        {
            get {
                ObservableCollection<ChatTabContentViewModel> tmp =
                    Chats == null ?
                        new ObservableCollection<ChatTabContentViewModel>():
                        new ObservableCollection<ChatTabContentViewModel>(Chats);
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
                OnPropertyChanged(nameof(HasInfo));
            }
        }

        #endregion


        protected ChatNewViewModel newChat = new ChatNewViewModel();
        private Dispatcher _dispatcher;


        public Tab_Chats()
        {
            InitializeComponent();
            _dispatcher = Dispatcher.CurrentDispatcher;
        }
        

        private void chatUpdated(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (sender == null) // A new Chat object
            {
                Chats.CollectionChanged -= chatUpdated;
                Chats.CollectionChanged += chatUpdated;
            }

            _dispatcher.InvokeAsync(new Action(() =>
            {
                foreach (ChatViewModel c in Chats)
                {
                    c.PropertyChanged -= chatHasInfo;
                    c.PropertyChanged += chatHasInfo;
                }
                if (sender != null && e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                {
                    if (!Chats[e.NewStartingIndex].HasInfo || !IsVisible) // Switch to tab if in background or created by user (HasInfo = false)
                        chatTabs.SelectedIndex = e.NewStartingIndex + 1;
                    foreach (ChatViewModel v in e.NewItems)
                        HasInfo = HasInfo || v.HasInfo;
                }
            }));
            OnPropertyChanged(nameof(Tabs));
        }
        private void chatHasInfo(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "HasInfo")
                HasInfo = HasInfo || (sender as ChatViewModel).HasInfo;
            if(!IsVisible && (sender as ChatViewModel).HasInfo) // Only change Tab if not visible
                chatTabs.SelectedIndex = Chats.IndexOf((sender as ChatViewModel))+1;
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
