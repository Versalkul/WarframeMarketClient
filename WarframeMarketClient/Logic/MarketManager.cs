using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WarframeMarketClient.Model;
using Timer=System.Timers.Timer;

namespace WarframeMarketClient.Logic
{
    class MarketManager : IDisposable
    {

        SocketManager socket;
        Timer onlineChecker;
        public static TimeSpan timeOffset;

        private Dictionary<string, Tuple<DateTime, OnlineState>> userStatusCache = new Dictionary<string, Tuple<DateTime, OnlineState>>(25);

        public MarketManager()
        {

            socket = new SocketManager();
            socket.recievedPM += new EventHandler<PmArgs>(AddNewChat);
            InitChats(); InitListings(); 


            

            onlineChecker = new Timer();
            onlineChecker.Elapsed += new System.Timers.ElapsedEventHandler(forceUserState);
            onlineChecker.Interval = 5000;//60000;
            onlineChecker.AutoReset = true;
            onlineChecker.Enabled = true;
            onlineChecker.Start();
            forceUserState();
        }

        // add coockie refresh

        #region ThreadStuff






        public void forceUserState()
        {
            (new Thread(() => forceUserState(null, null))).Start();
        }

        private void forceUserState(object o, EventArgs args)
        {
            OnlineState actualState = getStatusOnSite(ApplicationState.getInstance().Username);
            Console.WriteLine("State on Site is: "+actualState);
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
                    default:
                        break;
                }
            }

        }

        #endregion

        #region onlineOffline

        public OnlineState getStatusOnSite(string username)
        {
            lock (userStatusCache)
            {

                if (userStatusCache.ContainsKey(username) && (DateTime.Now - userStatusCache[username].Item1).Seconds < 3)
                {
                    Console.WriteLine("Chached");
                    return userStatusCache[username].Item2;
                }

            using (HttpWebResponse response = Webhelper.PostPage("http://warframe.market/api/check_status", $"users=[\"{username}\"]"))
            {
                if (response == null || response.StatusCode != HttpStatusCode.OK) return OnlineState.ERROR;
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    string json = reader.ReadToEnd();
                    OnlineInfo info = JsonConvert.DeserializeObject<JsonFrame<List<OnlineInfo>>>(json).response.First();

                    OnlineState ret;

                    ret= info.Online ?
                            info.Ingame ? OnlineState.INGAME : OnlineState.ONLINE
                            : OnlineState.OFFLINE;

                    if (!userStatusCache.ContainsKey(username)) userStatusCache.Add(username, new Tuple<DateTime, OnlineState>(DateTime.Now, ret));
                    else userStatusCache[username] = new Tuple<DateTime, OnlineState>(DateTime.Now, ret);
                    return ret;


                }

            }

            }
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


        private void AddNewChat(object o, PmArgs args)
        {

            ApplicationState appState = ApplicationState.getInstance();
            var chatList = appState.Chats.Where(chat => chat.User.Name == args.fromUser);
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
                appState.Chats.Add(new ViewModel.ChatViewModel(new User(args.fromUser), new List<ChatMessage>() { chatMsg }));
            }

        }

        public List<ChatMessage> GetMessages(string user)
        {
            using (HttpWebResponse response = Webhelper.GetPage("http://warframe.market/api/messages/" + user))
            {

                if (response == null) return new List<ChatMessage>();
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    string json = reader.ReadToEnd();
                    JsonFrame<List<ChatMessage>> result = JsonConvert.DeserializeObject<JsonFrame<List<ChatMessage>>>(json);
                    if (result.code != 200) return new List<ChatMessage>();
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
            using (HttpWebResponse response = Webhelper.PostPage("http://warframe.market/api/bs_order", $"id={item.Id}&count={item.Count}"))
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
            if (WarframeItem.itemInfoMap == null) WarframeItem.itemInfoMap = getTypeMap();
            if (WarframeItem.itemInfoMap.ContainsKey(name)) return WarframeItem.itemInfoMap[name].Item1;
            return "";
        }

        #endregion


        private void InitListings()
        {
            List<WarframeItem> offers = getOffers();
            ApplicationState appState = ApplicationState.getInstance();


                appState.BuyItems.Clear();
                appState.SellItems.Clear();

                foreach (WarframeItem item in offers)
                {

                    if (item.SellOffer) appState.SellItems.Add(item);
                    else appState.BuyItems.Add(item);

                }
        }

        private void InitChats()
        {

            ApplicationState appState = ApplicationState.getInstance();
            appState.Chats.Clear();

            Parallel.ForEach(GetChatUser(), (user) =>
            {
                List<ChatMessage> msg = GetMessages(user);
                appState.Chats.Add(new ViewModel.ChatViewModel(new User(user), msg));

            });
        }


        public static Dictionary<string, Tuple<string,int>> getTypeMap()
        {
            Dictionary<string, Tuple<string, int>> map = new Dictionary<string, Tuple<string, int>>(1000);
            using (HttpWebResponse response = Webhelper.GetPage("http://warframe.market/api/get_all_items_v2"))
            {
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
            setOffline();
            onlineChecker.Dispose();
            socket.Dispose();
        }
    }
}
