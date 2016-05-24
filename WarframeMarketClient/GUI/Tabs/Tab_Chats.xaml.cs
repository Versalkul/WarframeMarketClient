using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using WarframeMarketClient.Model;
using WarframeMarketClient.ViewModel;
using System.Linq;

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
            set {
                chatChanged(GetValue(ChatsProperty), value);
                SetValue(ChatsProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for Chats.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ChatsProperty =
            DependencyProperty.Register(nameof(Chats), typeof(ObservableCollection<ChatViewModel>), typeof(Tab_Chats), new PropertyMetadata(new PropertyChangedCallback(
                (Obj, a) => {
                    DependencyPropertyChangedEventArgs? args = (a as DependencyPropertyChangedEventArgs?);
                    (Obj as Tab_Chats).chatChanged(
                        args.HasValue ? args.Value.OldValue : null,
                        args.HasValue ? args.Value.NewValue : null);
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


        protected ChatNewViewModel newChat;
        private Dispatcher _dispatcher;


        public Tab_Chats()
        {
            newChat = new ChatNewViewModel(this);
            InitializeComponent();
            _dispatcher = Dispatcher.CurrentDispatcher;
        }


        #region Chat Events
        /// <summary>
        /// Called when chat changes completely
        /// </summary>
        /// <param name="oldChat"></param>
        /// <param name="newChat"></param>
        private void chatChanged(object oldChat, object newChat) {
            ObservableCollection<ChatViewModel> oC = oldChat as ObservableCollection<ChatViewModel>,
                nC = newChat as ObservableCollection<ChatViewModel>;
            if (nC == oC)
                return;
            if (oC != null)
            {
                oC.CollectionChanged -= chatUpdated;
                foreach (ChatViewModel c in oC)
                    c.PropertyChanged -= chatHasInfo;
            }
            if (nC != null)
            {
                nC.CollectionChanged += chatUpdated;
                HasInfo = false;
                foreach (ChatViewModel v in nC)
                    HasInfo = HasInfo || v.HasInfo;
            }
            chatUpdated(null, null);
        }

        /// <summary>
        /// Called when chat content changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chatUpdated(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            _dispatcher.InvokeAsync(new Action(() =>
            {
                
                foreach (ChatViewModel c in Chats.ToList())
                {
                    c.PropertyChanged -= chatHasInfo;
                    c.PropertyChanged += chatHasInfo;
                }
                if (sender != null && e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                {
                    /*
                     * Switch to tab if:
                     *  + New chat content received (HasInfo = true) and in background
                     *  + User created Tab (HasInfo = false, NewStartingIndex = 0)
                     *  - Tab inserted while loading (NewStartingIndex != 0)
                     */
                    if (Chats[e.NewStartingIndex].HasInfo && !IsVisible ||
                        !Chats[e.NewStartingIndex].HasInfo && e.NewStartingIndex == 0)
                        chatTabs.SelectedIndex = e.NewStartingIndex + 1;
                    foreach (ChatViewModel v in e.NewItems)
                        HasInfo = HasInfo || v.HasInfo;
                }
                OnPropertyChanged(nameof(Tabs));
            }));
        }

        private void chatHasInfo(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "HasInfo")
                HasInfo = HasInfo || (sender as ChatViewModel).HasInfo;
            if(!IsVisible && (sender as ChatViewModel).HasInfo) // Only change Tab if not visible
                chatTabs.SelectedIndex = Chats.IndexOf((sender as ChatViewModel))+1;
        }
        #endregion


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
