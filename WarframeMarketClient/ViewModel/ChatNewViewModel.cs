using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarframeMarketClient.Model;

namespace WarframeMarketClient.ViewModel
{
    public class ChatNewViewModel : ChatTabContentViewModel
    {
        #region TabProperties
        public override string DisplayName { get { return "+";} }
        #endregion

        #region Properties
        public string Status { get; set; }

        private string user;

        public string User
        {
            get { return user; }
            set { user = value; OnPropertyChanged("User"); }
        }
        #endregion

        public void openChat()
        {
            if (ApplicationState.getInstance().Market.getStatusOnSite(User) != OnlineState.ERROR) ApplicationState.getInstance().Chats.Add(new ChatViewModel(new Model.User(User), new List<ChatMessage>()));
            Console.WriteLine("Open Chat with: "+User);
        }
    }
}
