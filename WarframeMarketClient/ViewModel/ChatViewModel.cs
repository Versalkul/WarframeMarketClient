using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Threading;
using WarframeMarketClient.Model;

namespace WarframeMarketClient.ViewModel
{
    public class ChatViewModel : ChatTabContentViewModel, IEquatable<ChatViewModel>
    {

        private ChatInfo CIOpen, CIClose;

        #region TabProperties
        public override string DisplayName { get { return User.Name; } }
        
        public override OnlineState? OnlineStateInfo { get { return User.State; }  }
        #endregion

        #region Properties

        private User user;
        public User User { get { return user; } private set {
                if (user != null)
                    user.PropertyChanged -= User_PropertyChanged;
                user = value;
                user.PropertyChanged += User_PropertyChanged;
            } }

        private void User_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(OnlineStateInfo));
        }

        private Boolean closed;

        public Boolean Closed
        {
            get { return closed; }
            set {
                // might get called from another dimension or so...
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    closed = value;
                    InfoClose.Remove(CIClose);
                    if (closed)
                        InfoClose.Add(CIClose);
                    OnPropertyChanged("Closed");
                }));
            }
        }


        public ObservableCollection<ChatMessage> ChatMessages { get; private set; }

        public ObservableCollection<ChatElement> OldChatElements { get; private set; }

        public ObservableCollection<ChatElement> InfoOpen { get; private set; } = new ObservableCollection<ChatElement>();
        public ObservableCollection<ChatElement> InfoClose { get; private set; } = new ObservableCollection<ChatElement>();


        private string newMessage;
        public string NewMessage
        {
            get { return newMessage; }
            set { newMessage = value; OnPropertyChanged("NewMessage"); }
        }
        #endregion

        #region Constructor

        public ChatViewModel(User u,IEnumerable<ChatMessage> messages)
        {
            User = u;
            ChatMessages = new ObservableCollection<ChatMessage>(messages);

            CIOpen = new ChatInfo("Chat opened with\n" + u.Name);
            CIClose = new ChatInfo("Chat was closed\nSend Message to reopen");

            InfoOpen.Add(CIOpen);

            InitLockCollection();
        }

        object chatLock = new object();
        private void InitLockCollection()
        {
            DispatcherOperation col = System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                BindingOperations.EnableCollectionSynchronization(ChatMessages, chatLock);
            }));
            col.Task.GetAwaiter().GetResult(); // you shall not deadlock ^^
        }

        #endregion

        public void SendMessage()
        {
            if (Closed)
            {
                Closed = false;
                ArchiveCurrentChat();
            }
            Task.Factory.StartNew(() => ApplicationState.getInstance().Market.SendMessage(NewMessage,User.Name));
            ChatMessages.Add(
                new ChatMessage() {
                    Message = NewMessage,
                    MessageFrom = ApplicationState.getInstance().Username,
                    SendHour = DateTime.Now.Hour.ToString(),
                    SendMinute = DateTime.Now.Minute.ToString()
                });
            ApplicationState.getInstance().Logger.Log("Send Message ChatViewModel to "+User.Name+" : "+NewMessage);
            NewMessage = "";
        }

        /// <summary>
        /// Move all ChatMessages to OldChatMessages and clear the current list
        /// </summary>
        public void ArchiveCurrentChat()
        {
            if (!(ApplicationState.getInstance().Settings.PerserveChats && ChatMessages.Any())) // Setting not chosen or list empty
                return;
            OldChatElements = new ObservableCollection<ChatElement>(OldChatElements.Concat(ChatMessages));
            OldChatElements.Insert(0, CIOpen);
            OldChatElements.Add(CIClose);

            OnPropertyChanged("OldChatElements");

            ChatMessages.Clear();
        }

        public void CloseChat()
        {
            Task.Factory.StartNew(()=>ApplicationState.getInstance().Market.CloseChat(User.Name));
            ApplicationState.getInstance().Chats.Remove(this);
        }


        public bool Equals(ChatViewModel chat)
        {
            return chat.User.Name == User.Name && chat.ChatMessages.SequenceEqual(ChatMessages);
        }
    }
}
