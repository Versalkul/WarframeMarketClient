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
        static Dictionary<string, string> nameTypeMap ;
        Timer onlineChecker;


        public MarketManager()
        {
           
            socket = new SocketManager();
            socket.recievedPM += new EventHandler<PmArgs>(AddNewChat);
            InitApplicationState();
            onlineChecker = new Timer();
            onlineChecker.Elapsed += new System.Timers.ElapsedEventHandler(forceUserState);
            onlineChecker.Interval = 60000;
            onlineChecker.AutoReset = true;
            onlineChecker.Enabled = true;
            onlineChecker.Start();
            forceUserState();
            if (nameTypeMap == null) nameTypeMap = getTypeMap();
        }

        // add coockie refresh

        #region ThreadStuff






        public void forceUserState()
        {
            (new Thread(() => forceUserState(null, null))).Start();
        }

        private void forceUserState(object o, EventArgs args)
        {

            if (getStatusOnSite(ApplicationState.getInstance().Username) != ApplicationState.getInstance().OnlineState)
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
            using (HttpWebResponse response = Webhelper.PostPage("http://warframe.market/api/check_status", $"users=[\"{username}\"]"))
            {
                if (response == null || response.StatusCode != HttpStatusCode.OK) return OnlineState.ERROR;
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    string json = reader.ReadToEnd();
                    OnlineInfo info = JsonConvert.DeserializeObject<JsonFrame<List<OnlineInfo>>>(json).response.First();

                    if (info.Ingame) return OnlineState.INGAME;
                    return info.Online ? OnlineState.ONLINE : OnlineState.OFFLINE;


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
            if (!nameTypeMap.ContainsKey(item.Name)) return false;
            string sellType = item.SellOffer ? "sell" : "buy";

            string postData = $"item_name={item.Name.Replace(' ', '+')}&item_type={nameTypeMap[item.Name]}&action_type={sellType}&item_quantity={item.Count}&platina={item.Price}";


            using (HttpWebResponse response = Webhelper.PostPage("http://warframe.market/api/place_order", postData))
            {

                if (response == null) return false;
                using (StreamReader reader = new StreamReader(response.GetResponseStream())) return reader.ReadToEnd().Contains("200");
            }

        }

        public static string GetCategory(string name)
        {
            if (String.IsNullOrWhiteSpace(name)) return "";
            if (nameTypeMap == null) nameTypeMap = getTypeMap();
            if (nameTypeMap.ContainsKey(name)) return nameTypeMap[name];
            return "";
        }

        #endregion


        public void InitApplicationState()
        {

            Parallel.Invoke(new Action[2] { new Action(InitListings),new Action(InitChats)});

            Console.WriteLine("Init Done");

        }

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


        private static Dictionary<string, string> getTypeMap()
        {
            Dictionary<string, string> map = new Dictionary<string, string>(1000);
            using (HttpWebResponse response = Webhelper.GetPage("http://warframe.market/api/get_all_items_v2"))
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {

                    List<ItemTypeMap> mapList = JsonConvert.DeserializeObject<List<ItemTypeMap>>(reader.ReadToEnd());

                    foreach (ItemTypeMap elem in mapList)
                    {
                        map.Add(elem.item_name, elem.item_type);
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
