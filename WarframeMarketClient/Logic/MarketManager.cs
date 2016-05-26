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

namespace WarframeMarketClient.Logic
{
    public class MarketManager : IDisposable
    {

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

            Action[] initActions = new Action[4]
            {
                ()=>
                {
                    socket = new SocketManager();
                    socket.recievedPM += new EventHandler<PmArgs>(AddNewChat);
                     ApplicationState.getInstance().ValidationProgress+=10;
                },
                ()=>
                {
                    if (WarframeItem.itemInfoMap.Keys.Count < 100) WarframeItem.itemInfoMap = getTypeMap();
                     ApplicationState.getInstance().ValidationProgress+=15;
                },
                ()=>
                {
                    if(ApplicationState.getInstance().Chats.Any())
                    {
                        CheckAndUpdateChats();
                        appState.ValidationProgress+=20;
                    }
                    else // faster because loading new chats parallel
                    {
                        appState.ValidationProgress+=10;
                        List<string> users = GetChatUser();
                        int valPerUser =30/users.Count;
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
                    msgSave.SaveMessages();
                },
                ()=>
                {
                     offers = getOffers();
                    ApplicationState.getInstance().ValidationProgress+=20;
                }

            };
            Parallel.Invoke(initActions);
            foreach (WarframeItem item in offers)
            {

                if (item.SellOffer) appState.SellItems.Add(item);
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


        private void StateChecker(object o, ElapsedEventArgs args)
        {
            (new Thread(() => ForceUserStateSynchronous())).Start();
            (new Thread(() => EnsureSocketState())).Start();
            (new Thread(() => CheckAndUpdateChats())).Start(); // really ? maybe just 1/minute ?
            // update Listings
        }



        public void UpdateState() // change to parralell invoke
        {
            (new Thread(() => ForceUserStateSynchronous())).Start();
            (new Thread(() => CheckAndUpdateChats())).Start();
            (new Thread(() => EnsureSocketState())).Start();
        }

        private void ForceUserStateSynchronous()
        {

            OnlineState actualState = getStatusOnSite(ApplicationState.getInstance().Username);
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
                        setOffline(); // just maybe also stop timer
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
            if (state == OnlineState.ONLINE || state == OnlineState.INGAME) socket.EnsureOpenSocket();
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

                string param = JsonConvert.SerializeObject(userCheckSite);

                using (HttpWebResponse response = Webhelper.PostPage("http://warframe.market/api/check_status", $"users={param}"))
                {

                    if (response == null || response.StatusCode != HttpStatusCode.OK) return result;
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        string json = reader.ReadToEnd();
                        JsonFrame<List<OnlineInfo>> frame = JsonConvert.DeserializeObject<JsonFrame<List<OnlineInfo>>>(json);
                        if (!frame.response.Any()||frame.code!=200) return result;

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

        public void CheckAndUpdateChats() // if chat in app incorrect correct it (user send a msg via website)
        {

            if (ApplicationState.getInstance().Market==null) return;

            List<string> users = GetChatUser();
            bool first = true;
            int elem = 0; // what elem am i looking at relevant for 2. foreach loop
            users.Reverse();// oldest usernames checked first because if i get the newest last i can just Insert at 0
            foreach(string user in users)
            {

                if (!ApplicationState.getInstance().Chats.ToList().Exists(x => x.User.Name == user))
                {
                    List<ChatMessage> msg = GetMessages(user); // the init with the chat messages is part of the forech below
                    ApplicationState.getInstance().Chats.Insert(0, new ChatViewModel(new User(user), msg)); 
                    ApplicationState.getInstance().Chats.First().HasInfo = true;
                    elem--;
                }

            }

            List<ChatViewModel> closedChats = new List<ChatViewModel>();

            foreach(ChatViewModel chatView  in ApplicationState.getInstance().Chats)
            {

                if (elem < 0) // added this one milisecond before i dont want to check it 
                {
                    elem++;
                    continue;
                }

                if (chatView.ChatMessages.Count > 0) continue;

                if (!users.Contains(chatView.User.Name))  // user closed chat
                {
                   closedChats.Add(chatView); // cant remove elements in a foreach loop will do that later 
                    continue;
                }

                if (first) // check for new chats
                {
                    List<ChatMessage> msg = GetMessages(chatView.User.Name);
                    if (chatView.ChatMessages.SequenceEqual(msg)) first = false; // nothing new
                    else 
                    {

                        chatView.ChatMessages.Clear(); // not nice but working 
                        msg.ForEach(x => chatView.ChatMessages.Add(x));
                   }

                }

            }
           closedChats.ForEach(x => ApplicationState.getInstance().Chats.Remove(x));

        }

        private void AddNewChat(object o, PmArgs args) // checkAndUpdate ?
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
            }
            else
            {
                appState.Chats.Insert (0,new ViewModel.ChatViewModel(new User(args.fromUser), new List<ChatMessage>() { chatMsg }));
            }
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
                    if (!json.Contains("200")) return new List<ChatMessage>();
                    JsonFrame<List<ChatMessage>> result = JsonConvert.DeserializeObject<JsonFrame<List<ChatMessage>>>(json);
                    result.response.ForEach(x => x.Time = x.Time + timeOffset);

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
            socket.sendMessage(to, text);
        }

        public void CloseChat(string user)
        {
            using (HttpWebResponse response = Webhelper.GetPage("http://warframe.market/messages/close/" + user)) // no compiler dont remove that pls ^^
            {


            }
        }


        #endregion

        #region buy sell stuff

        public void UpdateListing() // may fail HORRIBLY if the cloning doesnt do clone
        {
            List<WarframeItem> items = getOffers();
            List<WarframeItem> itemsAll = items.ToList(); // cloning that list;
            ApplicationState.getInstance().BuyItems.ToList().ForEach(x => items.Remove(x)); // all identical items dont need to be looked at
            ApplicationState.getInstance().SellItems.ToList().ForEach(x => items.Remove(x));
            foreach (WarframeItem item in items)
            {
                string id = item.Id;
                item.Id = "";
                if (item.SellOffer)
                {
                    int itemIndex = ApplicationState.getInstance().SellItems.IndexOf(item);
                    if (itemIndex>0)
                    {
                        ApplicationState.getInstance().SellItems[itemIndex].Id = id;
                    }
                    else
                    {
                        item.Id = id;
                        ApplicationState.getInstance().SellItems.Add(item);
                    }

                }
                else
                {
                    int itemIndex = ApplicationState.getInstance().BuyItems.IndexOf(item);
                    if (itemIndex > 0)
                    {
                        ApplicationState.getInstance().BuyItems[itemIndex].Id = id;
                    }
                    else
                    {
                        item.Id = id;
                        ApplicationState.getInstance().SellItems.Add(item);
                    }

                }
            }
            foreach(WarframeItem item in ApplicationState.getInstance().BuyItems.ToList().Where(x=>x.Id!="")) // indirect clone here 
            {

                if (!itemsAll.Contains(item)) ApplicationState.getInstance().BuyItems.Remove(item);

            }
            foreach (WarframeItem item in ApplicationState.getInstance().SellItems.Where(x => x.Id != "")) // indirect clone here 
            {

                if (!itemsAll.Contains(item)) ApplicationState.getInstance().SellItems.ToList().Remove(item);

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
            using (HttpWebResponse response = Webhelper.PostPage("http://warframe.market/api/edit_order", $"id={item.Id}&new_count={item.Count}&new_price={item.Price}"))
            {
                if (response == null) return false;
                using (StreamReader reader = new StreamReader(response.GetResponseStream())) return reader.ReadToEnd().Contains("200");
            }
        }

        public bool AddItem(WarframeItem item)
        {
            if (!WarframeItem.itemInfoMap.ContainsKey(item.Name)) return false;
            string sellType = item.SellOffer ? "sell" : "buy";

            string postData = $"item_name={item.Name.Replace(' ', '+')}&item_type={WarframeItem.itemInfoMap[item.Name].Item1}&action_type={sellType}&item_quantity={item.Count}&platina={item.Price}";


            using (HttpWebResponse response = Webhelper.PostPage("http://warframe.market/api/place_order", postData))
            {

                if (response == null) return false;
                using (StreamReader reader = new StreamReader(response.GetResponseStream())) return reader.ReadToEnd().Contains("200");
            }

        }

        public static string GetCategory(string name)
        {
            if (String.IsNullOrWhiteSpace(name)) return "";
            if (WarframeItem.itemInfoMap == null|| WarframeItem.itemInfoMap.Count<50) WarframeItem.itemInfoMap = getTypeMap();
            if (WarframeItem.itemInfoMap.ContainsKey(name)) return WarframeItem.itemInfoMap[name].Item1;
            return "";
        }

        #endregion




        public static Dictionary<string, Tuple<string,int>> getTypeMap()
        {
            Dictionary<string, Tuple<string, int>> map = new Dictionary<string, Tuple<string, int>>(1000);
            using (HttpWebResponse response = Webhelper.GetPage("http://warframe.market/api/get_all_items_v2"))
            {
                if (response == null || response.StatusCode != HttpStatusCode.OK) return new Dictionary<string, Tuple<string, int>>();

                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {

                    List<ItemTypeMap> mapList = JsonConvert.DeserializeObject<List<ItemTypeMap>>(reader.ReadToEnd());

                    foreach (ItemTypeMap elem in mapList)
                    {
                        map.Add(elem.item_name,new Tuple<string, int>(elem.item_type,elem.mod_max_rank));
                    }
                }


            }
            return map;
        }

        public void Dispose()
        {
            SaveMsg saver = new SaveMsg();
            saver.SaveMessages();
            setOffline();
            onlineChecker.Dispose();
            socket.Dispose();
        }
    }
}
