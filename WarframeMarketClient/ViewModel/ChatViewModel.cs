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
    public class ChatViewModel : ChatTabContentViewModel,IEquatable<ChatViewModel>
    {
        #region TabProperties

        object chatLock = new object(); // seems not to work if not static 


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

        public ObservableCollection<ChatMessage> ChatMessages { get; private set; }

        
        private string newMessage;

        public string NewMessage
        {
            get { return newMessage; }
            set { newMessage = value; OnPropertyChanged("NewMessage"); }
        }
        #endregion

        public ChatViewModel()
        {
            ChatMessages = new ObservableCollection<ChatMessage>();
            InitLockCollection();
        }

        public ChatViewModel(User u,IEnumerable<ChatMessage> messages)
        {
            User = u;
            ChatMessages = new ObservableCollection<ChatMessage>(messages);
            InitLockCollection();
        }

        private void InitLockCollection()
        {
            DispatcherOperation col = System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                BindingOperations.EnableCollectionSynchronization(ChatMessages, chatLock);
            }));
            col.Task.GetAwaiter().GetResult(); // you shall not deadlock ^^
        }

        public void sendMessage()
        {
            Task.Factory.StartNew(()=> ApplicationState.getInstance().Market.SendMessage(User.Name, NewMessage));
            ApplicationState.getInstance().Chats.Where(user => user.User.Name == User.Name).First().ChatMessages.Add(new ChatMessage() { Message = NewMessage, MessageFrom = ApplicationState.getInstance().Username, SendHour = DateTime.Now.Hour.ToString(), SendMinute = DateTime.Now.Minute.ToString() }); // wir brauchen eine Chatmsg.GetMsg die automatisch sich um die zeit + differenz kümmert das hier ist nur ein workaround
            Console.WriteLine("Send Message to "+User.Name+" : "+NewMessage);
            NewMessage = "";
        }

        public void closeChat()
        {
            Task.Factory.StartNew(()=>ApplicationState.getInstance().Market.CloseChat(User.Name));
            ApplicationState.getInstance().Chats.Remove(this);
            Console.WriteLine("Close Chat with "+User.Name);
        }


        public bool Equals(ChatViewModel chat)
        {
            return chat.User.Name == User.Name && chat.ChatMessages.SequenceEqual(ChatMessages);
        }
    }
}
