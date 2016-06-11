using System;
using System.Collections.Generic;
using System.Linq;
using WarframeMarketClient.GUI.Tabs;
using WarframeMarketClient.Model;

namespace WarframeMarketClient.ViewModel
{
    public class ChatNewViewModel : ChatTabContentViewModel
    {

        private Tab_Chats parent;

        #region TabProperties
        public override string DisplayName { get { return "+";} }
        #endregion

        #region Properties
        private string status;

        public string Status
        {
            get { return status; }
            set { status = value; OnPropertyChanged(nameof(Status)); }
        }


        private string user;

        public string User
        {
            get { return user; }
            set { user = value; OnPropertyChanged(nameof(User)); }
        }
        
        #endregion

        public ChatNewViewModel(Tab_Chats parent)
        {
            this.parent = parent;
        }


        public void openChat()
        {
            Status = "Checking User";

            if (user == ApplicationState.getInstance().Username)
            {
                Status = "You can't chat with yourself";
                return;
            }

            IEnumerable<ChatViewModel> chatList = ApplicationState.getInstance().Chats.Where(chat => chat.User.Name == User);
            if (chatList.Any())
            {
                Status = "Chat does already exist";
                return;
            }


            if (ApplicationState.getInstance().Market.getStatusOnSite(User) != OnlineState.ERROR)
            {
                Status = "";
                ChatViewModel chat = new ChatViewModel(new Model.User(User), new List<ChatMessage>());
                ApplicationState.getInstance().Chats.Insert(0,chat);
            }
            else Status = "Username does not exist";
            Console.WriteLine("Open Chat with: "+User);
            User = "";
        }
    }
}
