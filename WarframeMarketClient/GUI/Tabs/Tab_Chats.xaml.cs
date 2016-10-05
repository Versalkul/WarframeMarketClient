using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using WarframeMarketClient.Model;
using WarframeMarketClient.ViewModel;
using System.Linq;
using System.Windows.Input;

namespace WarframeMarketClient.GUI.Tabs
{
    /// <summary>
    /// Code behind for Tab_Chats.xaml
    /// </summary>
    public partial class Tab_Chats : UserControl, TabInfoInterface, INotifyPropertyChanged
    {

        #region Properties
        
        /// <summary>
        /// This contains all Chats as ChatViewModel (not the new Chat Tab!)
        /// </summary>
        public ObservableCollection<ChatViewModel> Chats
        {
            get { return (ObservableCollection<ChatViewModel>)GetValue(ChatsProperty); }
            set {
                ChatChanged(GetValue(ChatsProperty), value);
                SetValue(ChatsProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for Chats.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ChatsProperty =
            DependencyProperty.Register(nameof(Chats), typeof(ObservableCollection<ChatViewModel>), typeof(Tab_Chats), new PropertyMetadata(new PropertyChangedCallback(
                (Obj, a) => {
                    DependencyPropertyChangedEventArgs? args = (a as DependencyPropertyChangedEventArgs?);
                    (Obj as Tab_Chats).ChatChanged(
                        args.HasValue ? args.Value.OldValue : null,
                        args.HasValue ? args.Value.NewValue : null);
                }
                )));

        
        /// <summary>
        /// This contains all Chats PLUS the new Chat Tab in the first place
        /// </summary>
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


        public ChatViewModel SelectedChat
        {
            get
            {
                return (Chats.Count == 0 | chatTabs.SelectedIndex < 1) ? null : Chats[chatTabs.SelectedIndex - 1];
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
            chatTabs.SelectionChanged += TabChanged;
            IsVisibleChanged += Tab_Chats_IsVisibleChanged;
        }


        public void test(object sender, KeyEventArgs e)
        {
            Console.WriteLine("Typed");
        }

        #region Tab Events
        private void Tab_Chats_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(IsVisible)
                TabChanged(null, null);
        }
        /// <summary>
        /// Called when the visibility of the tabs changes (Also when the whole Chat Tab becomes visible)
        /// </summary>
        private void TabChanged(object sender, SelectionChangedEventArgs e)
        {
            if (chatTabs.SelectedIndex > 0)
                Chats[chatTabs.SelectedIndex - 1].HasInfo = false;
            // Make sure HasInfo is correct
            HasInfo = false;
            foreach (ChatViewModel v in Chats)
                HasInfo = HasInfo || v.HasInfo;
        }
        #endregion


        #region Chat Events
        /// <summary>
        /// Called when chat changes completely
        /// Updates all the Event Listeners
        /// </summary>
        private void ChatChanged(object oldChat, object newChat) {
            ObservableCollection<ChatViewModel> oC = oldChat as ObservableCollection<ChatViewModel>,
                nC = newChat as ObservableCollection<ChatViewModel>;
            if (nC == oC)
                return;
            if (oC != null)
            {
                oC.CollectionChanged -= ChatUpdated;
                foreach (ChatViewModel c in oC)
                    c.PropertyChanged -= ChatHasInfo;
            }
            if (nC != null)
            {
                nC.CollectionChanged += ChatUpdated;
                HasInfo = false;
                foreach (ChatViewModel v in nC)
                    HasInfo = HasInfo || v.HasInfo;
            }
            ChatUpdated(null, null);
        }

        /// <summary>
        /// Called when chat content changes
        /// Checks HasInfo and switches to tab if applicable
        /// </summary>
        private void ChatUpdated(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Dispatcher neccessary because Chats is owned by the GUI thread
            _dispatcher.Invoke(new Action(() =>
            {
                foreach (ChatViewModel c in Chats.ToList())
                {
                    c.PropertyChanged -= ChatHasInfo;
                    c.PropertyChanged += ChatHasInfo;
                }

                // Reload Tabs before switching to new Tab
                OnPropertyChanged(nameof(Tabs));

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
            }));
        }

        /// <summary>
        /// Called when HasInfo of a Chat changes
        /// Checks HasInfo and switches to tab if applicable
        /// </summary>
        private void ChatHasInfo(object sender, PropertyChangedEventArgs args)
        {
            // Dispatcher neccessary because Chats is owned by the GUI thread
            _dispatcher.Invoke(new Action(() =>
            {
                if (args.PropertyName == "HasInfo" && sender is ChatViewModel && (sender as ChatViewModel).HasInfo)
                {
                    int tIndex = Chats.IndexOf((sender as ChatViewModel)) + 1;
                    if (chatTabs.SelectedIndex == tIndex && IsVisible) // if currently in foreground
                    {
                        (sender as ChatViewModel).HasInfo = false;
                        return;
                    }
                    HasInfo = HasInfo || (sender as ChatViewModel).HasInfo;
                    if (!IsVisible && (sender as ChatViewModel).HasInfo) // Only change Tab if not visible
                        chatTabs.SelectedIndex = tIndex;
                }
            }));
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
