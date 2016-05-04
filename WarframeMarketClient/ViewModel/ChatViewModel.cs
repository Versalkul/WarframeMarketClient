using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarframeMarketClient.Model;

namespace WarframeMarketClient.ViewModel
{
    public class ChatViewModel : ChatTabContentViewModel
    {
        public User user { get; private set; }

        public ObservableCollection<ChatMessage> ChatMessages { get; set; }


        public void sendMessage()
        {

        }

        public void closeChat()
        {

        }
    }
}
