using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarframeMarketClient.Model;

namespace WarframeMarketClient.ViewModel
{
    public class ChatViewModel : ChatTabContentViewModel
    {
        #region TabProperties
        public override string DisplayName { get { return User.Name; } }
        public override OnlineState? OnlineStateInfo { get { return User.State; } }
        #endregion

        #region Properties
        public User User { get; private set; }

        public ObservableCollection<ChatMessage> ChatMessages { get; set; }

        
        private string newMessage;

        public string NewMessage
        {
            get { return newMessage; }
            set { newMessage = value; OnPropertyChanged("NewMessage"); }
        }
        #endregion
        

        public ChatViewModel(User u,IEnumerable<ChatMessage> messages)
        {
            User = u;
            ChatMessages = new ObservableCollection<ChatMessage>(messages);
            
        }


        public void sendMessage()
        {
            ApplicationState.getInstance().Market.SendMessage(User.Name, NewMessage);
            Console.WriteLine("Send Message to "+User.Name+" : "+NewMessage);
        }

        public void closeChat()
        {
            ApplicationState.getInstance().Market.CloseChat(User.Name);
            Console.WriteLine("Close Chat with "+User.Name);
        }
    }
}
