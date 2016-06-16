using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using WarframeMarketClient.Model;
using WarframeMarketClient.ViewModel;
using Timer = System.Timers.Timer;
using WarframeMarketClient.Extensions;

namespace WarframeMarketClient.Logic
{
    public class MarketManager : IDisposable
    {

        private bool diposed = false;

        SocketManager socket;
        Timer onlineChecker;
        public static TimeSpan timeOffset;

        private Dictionary<string, Tuple<DateTime, OnlineState>> userStatusCache = new Dictionary<string, Tuple<DateTime, OnlineState>>(25);


        public MarketManager()
        {

            SaveMsg msgSave = new SaveMsg();
            
            msgSave.loadMessages().ForEach(x => ApplicationState.getInstance().Chats.Add(x));
            ViewModel.ChatViewModel[] result;
            List<WarframeItem> offers = null;
            ApplicationState appState = ApplicationState.getInstance();

            Action[] initActions = new Action[3]
            {
                ()=>
                {
                    socket = new SocketManager();
                    socket.recievedPM += new EventHandler<PmArgs>(AddNewChat);
                     ApplicationState.getInstance().ValidationProgress+=10;
                },
                ()=>
                {
                    if(ApplicationState.getInstance().Chats.Any())
                    {
                        CheckAndUpdateChats();
                        appState.ValidationProgress+=30;
                    }
                    else // faster because loading new chats parallel
                    {
                        appState.ValidationProgress+=10;
                        List<string> users = GetChatUser();
                        int valPerUser =30/(users.Count+1);
                        result = new ViewModel.ChatViewModel[users.Count];
                        Parallel.For(0, users.Count, (x) =>
                        {
                            List<ChatMessage> msg = GetMessages(users[x]);
                            result[x] = (new ViewModel.ChatViewModel(new User(users[x]), msg));
                             appState.ValidationProgress+=valPerUser;
                        });
                        foreach(ViewModel.ChatViewModel chat in result)
                        {
                            appState.Chats.Add(chat);
                        }
                    }
                    Task.Factory.StartNew(UpdateChatOnlineState);
                    msgSave.SaveMessages();
                },
                ()=>
                {
                     offers = getOffers();
                    ApplicationState.getInstance().ValidationProgress+=30;
                }

            };
            Parallel.Invoke(initActions);
            foreach (WarframeItem item in offers)
            {

                if (item.IsSellOffer) appState.SellItems.Add(item);
                else appState.BuyItems.Add(item);

            }
            

            Console.WriteLine("Done paralell init");

            onlineChecker = new Timer();
            onlineChecker.Elapsed += new System.Timers.ElapsedEventHandler(StateChecker); // new check regulary not just forceState
            onlineChecker.Interval = 35000;
            onlineChecker.AutoReset = true;
            onlineChecker.Enabled = true;
            onlineChecker.Start();

        }

        // add coockie refresh

        #region ThreadStuff

        private int timerCount = 0;
        private void StateChecker(object o, ElapsedEventArgs args)
        {
            timerCount++;
            if ((socket.SocketWasClosed||timerCount%5==3)&&ApplicationState.getInstance().OnlineState.IsOnline()) // check for messages every two min 
            {
                Task.Factory.StartNew(() => { CheckAndUpdateChats(); UpdateChatOnlineState(); });
                socket.SocketWasClosed = false;
            }
            Task.Factory.StartNew(ForceUserStateSynchronous);
            Task.Factory.StartNew(EnsureSocketState);
            if (timerCount % 5 == 4) {
                 Task.Factory.StartNew(UpdateListing);
                
            }
        }



        public void UpdateState() // change to parralell invoke
        {
            if (socket.SocketWasClosed&&ApplicationState.getInstance().OnlineState.IsOnline())
            {
                Task.Factory.StartNew(CheckAndUpdateChats);
                socket.SocketWasClosed = false;
            }
            Task.Factory.StartNew(ForceUserStateSynchronous);
            Task.Factory.StartNew(EnsureSocketState);
        }

        private void ForceUserStateSynchronous()
        {

            OnlineState actualState = getStatusOnSite(ApplicationState.getInstance().Username);
            if (!onlineChecker.Enabled&& ApplicationState.getInstance().OnlineState!=OnlineState.DISABLED) onlineChecker.Enabled = true;
            if (actualState != ApplicationState.getInstance().OnlineState)
            {
                switch (ApplicationState.getInstance().OnlineState)
                {
                    case OnlineState.OFFLINE:
                        setOffline();
                        break;
                    case OnlineState.ONLINE:
                        setOnline();
                        break;
                    case OnlineState.INGAME:
                        setIngame();
                        break;
                    case OnlineState.DISABLED:
                        onlineChecker.Enabled = false;
                        break;
                    default:
                        break;
                }
            }

        }



        #endregion

        #region onlineOffline

        public void EnsureSocketState()
        {
            OnlineState state = ApplicationState.getInstance().OnlineState;
            if (state.IsOnline()) socket.EnsureOpenSocket();
        }

        public Dictionary<string,OnlineState> getStatusOnSite(List<string> users)
        {

            lock (userStatusCache)
            {
                Dictionary<string, OnlineState> result = new Dictionary<string, OnlineState>(users.Count);
                users.ForEach(x => result.Add(x, OnlineState.ERROR));
                List<string> userCached = users.Where((username) => userStatusCache.ContainsKey(username) && (DateTime.Now - userStatusCache[username].Item1).Seconds < 30).ToList();
                List<string> userCheckSite = users.Except(userCached).ToList();

                userCached.ForEach(s => result[s] = userStatusCache[s].Item2);

                if (userCheckSite.Count == 0) return result;

                string param = JsonConvert.SerializeObject(users); // check every user always (better 1 request for 5 than 3 requests for 1 each)

                using (HttpWebResponse response = Webhelper.PostPage("http://warframe.market/api/check_status", $"users={param}"))
                {

                    if (response == null || response.StatusCode != HttpStatusCode.OK) return result;
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        string json = reader.ReadToEnd();
                        if (!json.Contains("200")) return result;
                        JsonFrame<List<OnlineInfo>> frame = JsonConvert.DeserializeObject<JsonFrame<List<OnlineInfo>>>(json);
                        if (!frame.response.Any()) return result;

                        foreach (OnlineInfo info in frame.response)
                        {

                            OnlineState state = info.Online ?
                                info.Ingame ? OnlineState.INGAME : OnlineState.ONLINE
                                : OnlineState.OFFLINE;
                            result[info.Username] = state;
                            if (!userStatusCache.ContainsKey(info.Username)) userStatusCache.Add(info.Username, new Tuple<DateTime, OnlineState>(DateTime.Now, state));
                            else userStatusCache[info.Username] = new Tuple<DateTime, OnlineState>(DateTime.Now, state);
                        }

                    }
                }
                return result;
            }

        }

        public OnlineState getStatusOnSite(string username)
        {

            return getStatusOnSite(new List<string>() { username }).Values.First();
           
        }

        public void setIngame()
        {
            socket.setIngame();
        }

        public void setOnline()
        {
            socket.setOnline();
        }

        public void setOffline()
        {
            socket.setOffline();
        }

        #endregion

        #region PM

        public void UpdateChatOnlineState()
        {
            List<string> names = ApplicationState.getInstance().Chats.Select(x => x.User.Name).ToList();
            

            Dictionary<string, OnlineState> stateInfo = getStatusOnSite(names);

            foreach (ChatViewModel chat in ApplicationState.getInstance().Chats)
            {
                if (stateInfo.ContainsKey(chat.User.Name)) chat.User.State = stateInfo[chat.User.Name];
             }

        }

        public void CheckAndUpdateChats() // if chat in app incorrect correct it (user send a msg via website)
        {

            List<string> users = GetChatUser();
            bool first = true;
            int elem = 0; // what elem am i looking at relevant for 2. foreach loop
            users.Reverse();// oldest usernames checked first because if i get the newest last i can just Insert at 0
            foreach(string user in users)
            {

                if (!ApplicationState.getInstance().Chats.ToList().Exists(x => x.User.Name == user))
                {
                    List<ChatMessage> msg = GetMessages(user); // the init with the chat messages is part of the forech below
                    ChatViewModel chat = new ChatViewModel(new User(user), msg);
                    chat.HasInfo = true;
                    ApplicationState.getInstance().Chats.Insert(0,chat);

                    ApplicationState.getInstance().Logger.Log(" Getting a new chat ");

                    ApplicationState.getInstance().InvokeNewMessage(this, chat.ChatMessages.ToList());
                    elem--;
                }

            }

            if (elem < 0) UpdateChatOnlineState();
            List<ChatViewModel> closedChats = new List<ChatViewModel>();

            foreach(ChatViewModel chatView  in ApplicationState.getInstance().Chats)
            {

                if (elem < 0) // added this one milisecond before i dont want to check it 
                {
                    elem++;
                    continue;
                }

                if (chatView.ChatMessages.Count == 0) continue;

                if (!users.Contains(chatView.User.Name))  // user closed chat
                {
                   closedChats.Add(chatView); // cant remove elements in a foreach loop will do that later 
                    continue;
                }

                if (first) // check for new chats
                {
                    List<ChatMessage> msg = GetMessages(chatView.User.Name);
                    if (chatView.ChatMessages.SequenceEqual(msg)) first = false; // nothing new
                    else // this is not really what i want ...
                    {
                        List<ChatMessage> newMsg = msg.Skip(msg.FindLastIndex(x => x.IsFromMe)+1 ).Except(chatView.ChatMessages).ToList(); // take last notFromMe and just if they werent in the chatView
                        chatView.ChatMessages.Clear(); // not nice but working 
                        msg.ForEach(x => chatView.ChatMessages.Add(x));
                        if (newMsg.Any()) 
                        {
                            chatView.HasInfo = true;
                            ApplicationState.getInstance().Logger.Log(" Getting a new chatmessage " + newMsg.Last().Time + "  "+ newMsg.Last().MessageFrom+": "+ newMsg.Last().Message);
                            ApplicationState.getInstance().InvokeNewMessage(this,newMsg);
                        }
                   }

                }

            }
           closedChats.ForEach(x => ApplicationState.getInstance().Chats.Remove(x));

        }

        private void AddNewChat(object o, PmArgs args) 
        {

            ApplicationState appState = ApplicationState.getInstance();
            IEnumerable<ChatViewModel> chatList = appState.Chats.Where(chat => chat.User.Name == args.fromUser);
            ChatMessage chatMsg = new ChatMessage()
            {
                Message = args.message,
                MessageFrom = args.fromUser,
                SendMinute = DateTime.Now.Minute.ToString(),
                SendHour = DateTime.Now.Hour.ToString()
            };

            
            if (chatList.Any())
            {
                chatList.First().ChatMessages.Add(chatMsg);
                chatList.First().HasInfo = true;
            }
            else
            {
                ChatViewModel chat = new ViewModel.ChatViewModel(new User(args.fromUser), new List<ChatMessage>() { chatMsg });
                chat.HasInfo = true;
                appState.Chats.Insert (0,chat);
                Task.Factory.StartNew(UpdateChatOnlineState);
            }
                ApplicationState.getInstance().InvokeNewMessage(this, new List<ChatMessage>() { chatMsg });
            // set Has Info
        }

        public List<ChatMessage> GetMessages(string user)
        {
            using (HttpWebResponse response = Webhelper.GetPage("http://warframe.market/api/messages/" + user))
            {

                if (response == null) return new List<ChatMessage>();
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    string json = reader.ReadToEnd();
                    if (!json.Contains("200")) return new List<ChatMessage>(); // need to check befor the Deserialize Method (else crash in Deserialize)
                    JsonFrame<List<ChatMessage>> result = JsonConvert.DeserializeObject<JsonFrame<List<ChatMessage>>>(json);
                    result.response.ForEach(x => { // fixing Time and newlines
                        x.Time = x.Time + timeOffset;
                        x.Message = x.Message.Replace("{{br}}", System.Environment.NewLine);
                    });

                    return result.response;

                }

            }

        }

        public List<string> GetChatUser()
        {
            return HtmlParser.getChatUser();
        }

        public void SendMessage(string to, string text)
        {
            socket.sendMessage(to, text.Replace(System.Environment.NewLine,"<br>"));
        }

        public void CloseChat(string user)
        {
            using (HttpWebResponse response = Webhelper.GetPage("http://warframe.market/messages/close/" + user))
            {


            }
        }


        #endregion

        #region buy sell stuff

        public void UpdateListing() 
        {
            List<WarframeItem> items = getOffers();
            List<WarframeItem> itemsAll = items.ToList(); // cloning that list;
            ApplicationState.getInstance().BuyItems.ToList().ForEach(x => items.Remove(x)); // all identical items dont need to be looked at
            ApplicationState.getInstance().SellItems.ToList().ForEach(x => items.Remove(x));
            foreach (WarframeItem it in items)
            {
                WarframeItem item = new WarframeItem(it); // creating a clone of the object so i dont edit it in the list
                string id = item.Id;
                item.Id = "";
                if (item.IsSellOffer) 
                {
                    int itemIndex = ApplicationState.getInstance().SellItems.IndexOf(item);
                    if (itemIndex>=0) // i just added the item i just need to add its id
                    {
                        ApplicationState.getInstance().SellItems[itemIndex].Id = id;
                    }
                    else
                    {

                        IEnumerable<WarframeItem> sameItem = ApplicationState.getInstance().SellItems.Where(x => x.Id == id);
                        if (sameItem.Any()) // do i know the itemid => item changed
                        {
                            WarframeItem updateItem = sameItem.First();
                            if(updateItem.Price != item.Price) updateItem.Price = item.Price;
                            if(updateItem.Count != item.Count) updateItem.Count = item.Count;
                            if (updateItem.ModRank != item.ModRank) updateItem.ModRank = item.ModRank;
                            if (updateItem.Name != item.Name) updateItem.Name = item.Name; // not sure if this case can happen 
                        }
                        else
                        {
                        item.Id = id;
                        ApplicationState.getInstance().SellItems.Add(item);

                        }
                    }

                }
                else
                {
                    int itemIndex = ApplicationState.getInstance().BuyItems.IndexOf(item);

                    if (itemIndex >= 0)
                    {
                        ApplicationState.getInstance().BuyItems[itemIndex].Id = id;
                    }
                    else
                    {
                        IEnumerable<WarframeItem> sameItem = ApplicationState.getInstance().BuyItems.Where(x => x.Id == id);
                        if (sameItem.Any()) // do i know the itemid => item changed
                        {
                            WarframeItem updateItem = sameItem.First();
                            if (updateItem.Price != item.Price) updateItem.Price = item.Price;
                            if (updateItem.Count != item.Count) updateItem.Count = item.Count;
                            if (updateItem.ModRank != item.ModRank) updateItem.ModRank = item.ModRank;
                            if (updateItem.Name != item.Name) updateItem.Name = item.Name; // not sure if this case can happen 
                        }
                        else
                        {
                            item.Id = id;
                            ApplicationState.getInstance().BuyItems.Add(item);

                        }
                    }

                }
            }

            foreach (WarframeItem item in ApplicationState.getInstance().BuyItems.Where(x => x.Id != ""))
            {

                if (!itemsAll.Contains(item)) ApplicationState.getInstance().BuyItems.Remove(item);

            }
            foreach (WarframeItem item in ApplicationState.getInstance().SellItems.Where(x => x.Id != "")) 
            {

                if (!itemsAll.Contains(item)) ApplicationState.getInstance().SellItems.Remove(item);

            }

        }

        public List<WarframeItem> getOffers()
        {
            return HtmlParser.getOffers();
        }

        public bool RemoveItem(WarframeItem item)
        {

            using (HttpWebResponse response = Webhelper.PostPage("http://warframe.market/api/remove_order", $"id={item.Id}&count={item.Count}"))
            {
                if (response == null) return false;
                using (StreamReader reader = new StreamReader(response.GetResponseStream())) return reader.ReadToEnd().Contains("200");
            }

        }

        public bool SoldItem(WarframeItem item)
        {
            using (HttpWebResponse response = Webhelper.PostPage("http://warframe.market/api/bs_order", $"id={item.Id}&count=1"))
            {
                if (response == null) return false;
                using (StreamReader reader = new StreamReader(response.GetResponseStream())) return reader.ReadToEnd().Contains("200");
            }
        }

        public bool EditItem(WarframeItem item)
        {
            string modRank = item.ModRank >= 0 ? $"new_mod_rank={item.ModRank}&" : "";
            using (HttpWebResponse response = Webhelper.PostPage("http://warframe.market/api/edit_order", modRank+ $"id={item.Id}&new_count={item.Count}&new_price={item.Price}"))
            {
                if (response == null) return false;
                using (StreamReader reader = new StreamReader(response.GetResponseStream())) return reader.ReadToEnd().Contains("200");
            }
        }

        public bool AddItem(WarframeItem item)
        {
            if (!ItemMap.IsValidItemName(item.Name)) return false;
            string sellType = item.IsSellOffer ? "sell" : "buy";
            string postData = $"item_name={item.Name.Replace(' ', '+')}&item_type={item.Category}&action_type={sellType}&item_quantity={item.Count}&platina={item.Price}";


            using (HttpWebResponse response = Webhelper.PostPage("http://warframe.market/api/place_order", postData))
            {

                if (response == null) return false;
                using (StreamReader reader = new StreamReader(response.GetResponseStream())) return reader.ReadToEnd().Contains("200");
            }

        }



        #endregion


        public void Dispose()
        {
            if (diposed) return;
            diposed = true;
            SaveMsg saver = new SaveMsg();
            saver.SaveMessages();
            onlineChecker.Dispose();
            socket.Dispose();
        }


        ~MarketManager() // finalizer Garbage Collector calls it when collecting the old object
        {
           if(!this.diposed) Dispose();
        }
    }
}
