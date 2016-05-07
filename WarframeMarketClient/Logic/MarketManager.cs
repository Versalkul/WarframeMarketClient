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
    class MarketManager :IDisposable
    {

        SocketManager socket;
        Dictionary<string, string> nameTypeMap = new Dictionary<string, string>(1000);
        Timer onlineChecker;


        public MarketManager()
        {
            nameTypeMap = getTypeMap();
            (new Thread(refreshCoockie)).Start();
            onlineChecker = new Timer();
            onlineChecker.Elapsed += new System.Timers.ElapsedEventHandler(forceUserState);
            onlineChecker.Interval = 60000;
            onlineChecker.AutoReset = true;
            onlineChecker.Start();


        }

        // add coockie refresh

        #region ThreadStuff

        public OnlineState getStatusOnSite(string username)
        {
            using (HttpWebResponse response = Webhelper.PostPage("http://warframe.market/api/check_status", $"[\"username\"]"))
            {
                if (response == null) return OnlineState.OFFLINE;
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    
                    OnlineInfo info = JsonConvert.DeserializeObject<JsonFrame<OnlineInfo>>(reader.ReadToEnd()).response;

                    if (info.Ingame) return OnlineState.INGAME;
                    return info.Online?OnlineState.ONLINE:OnlineState.OFFLINE;


                }


            }
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


        private void refreshCoockie()
        {
            using (HttpWebResponse response = Webhelper.GetPage("http://warframe.market/"))
            {
            }

        }

        #endregion

        #region onlineOffline

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

        public List<ChatMessage> GetMessages(string user)
        {
            using (HttpWebResponse response = Webhelper.GetPage("http://warframe.market/api/messages/" + user))
            {

                if (response == null) return new List<ChatMessage>();
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    string json = reader.ReadToEnd();
                    if (!json.Contains("200")) return new List<ChatMessage>();
                    return JsonConvert.DeserializeObject<List<ChatMessage>>(json);

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

        public static List<WarframeItem> getOffers()
        {
            return HtmlParser.getOffers();
        }

        public bool RemoveItem(WarframeItem item)
        {

            using (HttpWebResponse response = Webhelper.PostPage("http://warframe.market/api/remove_order", $"id={item.id}&count={item.count}"))
            {
                if (response == null) return false;
                using (StreamReader reader = new StreamReader(response.GetResponseStream())) return reader.ReadToEnd().Contains("200");
            }

        }

        public bool SoldItem(WarframeItem item)
        {
            using (HttpWebResponse response = Webhelper.PostPage("http://warframe.market/api/bs_order", $"id={item.id}&count={item.count}"))
            {
                if (response == null) return false;
                using (StreamReader reader = new StreamReader(response.GetResponseStream())) return reader.ReadToEnd().Contains("200");
            }
        }

        public bool EditItem(WarframeItem item)
        {
            using (HttpWebResponse response = Webhelper.PostPage("http://warframe.market/api/edit_order", $"id={item.id}&new_count={item.count}&new_price={item.price}"))
            {
                if (response == null) return false;
                using (StreamReader reader = new StreamReader(response.GetResponseStream())) return reader.ReadToEnd().Contains("200");
            }
        }

        public bool AddItem(WarframeItem item)
        {
            if (!nameTypeMap.ContainsKey(item.name)) return false;
            string sellType = item.sellOffer ? "sell" : "buy";

            string postData = $"item_name={item.name.Replace(' ', '+')}&item_type={nameTypeMap[item.name]}&action_type={sellType}&item_quantity={item.count}&platina={item.price}";


            using (HttpWebResponse response = Webhelper.PostPage("http://warframe.market/api/place_order", postData))
            {

                if (response == null) return false;
                using (StreamReader reader = new StreamReader(response.GetResponseStream())) return reader.ReadToEnd().Contains("200");
            }

        }

        #endregion


        private Dictionary<string, string> getTypeMap()
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
            onlineChecker.Dispose();
            socket.Dispose();
        }
    }
}
