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
    public class ChatViewModel : ChatTabContentViewModel, INotifyPropertyChanged
    {
        public User User { get; private set; }

        public ObservableCollection<ChatMessage> ChatMessages { get; set; }

        public override string DisplayName { get { return User.Name; } }
        
        public override OnlineState? OnlineStateInfo { get { return User.State; } }


        private string newMessage;

        public string NewMessage
        {
            get { return newMessage; }
            set { newMessage = value; OnPropertyChanged("NewMessage"); }
        }




        public ChatViewModel(User u)
        {
            User = u;
            ChatMessages = new ObservableCollection<ChatMessage>
            {
                new ChatMessage() { Message = "Test", SendHour="20", SendMinute="15", MessageFrom="A" },
                new ChatMessage() { Message = "Passt", SendHour="20", SendMinute="16", MessageFrom="B" },
                new ChatMessage() { Message = "Nice!", SendHour="20", SendMinute="17", MessageFrom="A" },
                new ChatMessage() { Message = "Test", SendHour="20", SendMinute="15", MessageFrom="A" },
                new ChatMessage() { Message = "Passt", SendHour="20", SendMinute="16", MessageFrom="B" },
                new ChatMessage() { Message = "Nice!", SendHour="20", SendMinute="17", MessageFrom="A" },
                new ChatMessage() { Message = "Test", SendHour="20", SendMinute="15", MessageFrom="A" },
                new ChatMessage() { Message = "Passt", SendHour="20", SendMinute="16", MessageFrom="B" },
                new ChatMessage() { Message = "Nice!", SendHour="20", SendMinute="17", MessageFrom="A" },
                new ChatMessage() { Message = "Test", SendHour="20", SendMinute="15", MessageFrom="A" },
                new ChatMessage() { Message = "Passt", SendHour="20", SendMinute="16", MessageFrom="B" },
                new ChatMessage() { Message = "Nice!", SendHour="20", SendMinute="17", MessageFrom="A" },
            };
        }


        public void sendMessage()
        {
            Console.WriteLine("Send Message to "+User.Name+" : "+NewMessage);
        }

        public void closeChat()
        {
            Console.WriteLine("Close Chat with "+User.Name);
        }



        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string property)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(property));
        }
    }
}
