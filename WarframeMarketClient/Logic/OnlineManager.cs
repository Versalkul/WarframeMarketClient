using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using WebSocket4Net;

namespace WarframeMarketClient.Logic
{


    class OnlineManager:IDisposable
    {

        WebSocket socket;
        status onlineStatus = status.offline;
        //bool onlineIngame = false;
        private readonly string session;
        private Queue<string> jsonsToSend = new Queue<string>(5);
        Dictionary<string, string> nameTypeMap = new Dictionary<string, string>(1000);

        public string username { get; private set; }

        Thread stateEnfore;

        public event EventHandler<PmArgs> recievedPM;

        enum status { offline,online,ingame}

        public OnlineManager(string session)
        {
            username = "";
            this.session = session;
            if (!verifyToken()) throw new ArgumentException("Session Token invalid");
            List<KeyValuePair<string, string>> cookies = new List<KeyValuePair<string, string>>(1);
            cookies.Add(new KeyValuePair<string, string>("session", session));
            socket = new WebSocket("wss://warframe.market/socket", "", cookies);
            socket.Opened += new EventHandler(onOpen);
            socket.Error += new EventHandler<SuperSocket.ClientEngine.ErrorEventArgs>(onError);
            socket.MessageReceived += new EventHandler<MessageReceivedEventArgs>(onMessage);
            socket.DataReceived += new EventHandler<DataReceivedEventArgs>(onData);
            nameTypeMap = getTypeMap();

            new Thread(new ThreadStart(()=>enhanceCookieLifetime())).Start();
            stateEnfore = new Thread(new ThreadStart(forceStatus));
            stateEnfore.Start();

          

        }

        public bool enhanceCookieLifetime()
        {

            using (HttpWebResponse response = Webhelper.getPage("http://warframe.market", session))
            {
                return response != null;
            }

        }

        private void forceStatus() // if the site changes the status i change it back
        {

            while (true)
            {
                Thread.Sleep(60000);
                if (username == "") continue;
                if (getStatusOnSite() != onlineStatus)
                {

                    switch (onlineStatus)
                    {

                        case status.ingame: setIngame();
                            break;
                        case status.online:setOnline();
                            break;
                        case status.offline: setOffline();
                            break;

                        default: break;
                    }

                }
            }
        }


        private Dictionary<string,string> getTypeMap()
        {
            Dictionary<string, string> map = new Dictionary<string, string>(1000);
            using (HttpWebResponse response = Webhelper.getPage("http://warframe.market/api/get_all_items_v2", session))
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {

                    List<ItemTypeMap> mapList = JsonConvert.DeserializeObject<List<ItemTypeMap>>(reader.ReadToEnd());

                   foreach(ItemTypeMap elem in mapList)
                    {
                        map.Add(elem.item_name, elem.item_type);
                    }
                }


             }
            return map;
        }

        private status getStatusOnSite()
        {
            using (HttpWebResponse response = Webhelper.getPage("http://warframe.market/person/" + username, session))
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    return reader.ReadToEnd().Contains("Online in Game") ? status.ingame : status.offline;

                }


                }
        }

        private bool verifyToken()
        {
                using (HttpWebResponse response = Webhelper.getPage("http://warframe.market/account",session))
                {
                    
                    if (response==null||((HttpWebResponse)response).StatusCode != HttpStatusCode.OK) return false;
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        while (!reader.EndOfStream)
                        {
                        
                            string s = reader.ReadLine();
                            if(s.Contains("waiting for your message ...")) // dirty hack for getting the username (semi-parsing the html)
                            {
                                s = s.Substring(s.IndexOf("value")+7);
                                username =s.Substring(0, s.IndexOf('"'));
                                Console.WriteLine("Got username " +username);
                                return true;
                            }

                        }
                    }
                }
            return false;
   

        }

        #region events

        public void onError(object caller, SuperSocket.ClientEngine.ErrorEventArgs args)
        {
            Console.WriteLine("Error from websocket:");
            Console.WriteLine(args.Exception.Message);
            Console.WriteLine(args.Exception);
           
        }

        public void onMessage(object caller, MessageReceivedEventArgs args)
        {
            // if message raiseEvent

            if (recievedPM != null && args.Message.Contains("recive_message"))
            {
                SocketMessage msg = JsonConvert.DeserializeObject<SocketMessage>(args.Message);

                recievedPM.Invoke(this, new PmArgs(msg.data.text, msg.data.from));

            }
            Console.WriteLine(args.Message);
        }


        public void onData(object caller, DataReceivedEventArgs args)
        {
            Console.WriteLine(args.Data);


        }

        private void onOpen(object sender, EventArgs args)
        {
            
            if(onlineStatus==status.ingame) sendIngameJson() ;

            if (jsonsToSend.Count != 0)
            {

                while (jsonsToSend.Count != 0)
                {
                    socket.Send(jsonsToSend.Dequeue());
                }

            }
            if (onlineStatus == status.offline) socket.Close();

        }

        #endregion

        #region onlineOffline

        private void sendJson(string json)
        {
            if (socket.State == WebSocketState.Closed || socket.State == WebSocketState.None)
            {
                jsonsToSend.Enqueue(json);
                socket.Open();
            }
            else
            {
                socket.Send(json);
            }
        }

        private void sendIngameJson()
        {
            sendJson("{\"destination\":\"user.set_status\",\"data\":{\"status\":\"in_game\"}}");
        }

        public void setIngame()
        {
            onlineStatus = status.ingame;
            sendIngameJson();
        }

        public void setOnline()
        {

            onlineStatus = status.online;
            sendJson("{\"destination\":\"user.set_status\",\"data\":{\"status\":\"online\"}}");

        }

        public void setOffline()
        {
            onlineStatus = status.offline;

            sendJson("{\"destination\":\"user.set_status\",\"data\":{\"status\":\"online\"}}");
            if(socket.State==WebSocketState.Open&&jsonsToSend.Count==0) socket.Close(); 
        }

        #endregion

        #region PM

        public void sendMessage(string to, string text)
        {
            sendJson("{\"destination\":\"user.send_message\",\"data\":{\"text\":\"" + text + "\",\"message_to\":\"" + to + "\"}}");
        }

        public List<PmJson> getMessages(string user)
        {
            using (HttpWebResponse response = Webhelper.getPage("http://warframe.market/api/messages/" + user, session))
            {

                string json = (new StreamReader(response.GetResponseStream())).ReadToEnd();

                if(!json.Contains("200")) return new List<PmJson>();

                return JsonConvert.DeserializeObject<PmRequest>(json).response;

            }

        }
        public void closeChat(string user)
        {
            using (HttpWebResponse response = Webhelper.getPage("http://warframe.market/messages/close/" + user, session)) // no compiler dont remove that pls ^^
            {


            }
        }


        public List<string> getChatUser()
        {
            using (HttpWebResponse response = Webhelper.getPage("http://warframe.market/messages", session))
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {

                    List<string> user= new List<string>();
                    bool ready = false;
                    while (!reader.EndOfStream)
                    {

                        string line = reader.ReadLine();
                        if(line.Contains("List of users.") && !ready)
                        {
                            ready = true;
                            continue;
                        }
                        if (!ready) continue;
                        if(line.Contains("<li data-name=") && !line.Contains("ingame_name"))
                        {
                            user.Add(line.Split()[1]);
                        }

                        if (line.Contains("</div>")) return user;

                    }
                    return user;

               }

            }
        }


        #endregion

        #region buy sell stuff
        public List<WarframeItem> getOffers()
        {
            using (HttpWebResponse response = Webhelper.getPage("http://warframe.market/orders", session))
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {

                    bool sell = true;
                    bool ready = false;
                    string name = "";
                    int price = 0;
                    int count = 0;
                    int modrank = -1;
                    int stepcount = 0;
                    string id="";
                    List<WarframeItem> offers = new List<WarframeItem>(50);

                    while (!reader.EndOfStream)
                    {
                        if (!ready)
                        {
                            if (reader.ReadLine().Contains("Sell orders")) ready = true; // first want to sell stuff
                            continue;
                        }
                        string line = reader.ReadLine();
                        if (line.Contains("Buy orders")) // after that want to buy 
                        {
                            sell = false;
                            continue;
                        }

                        if(line.Contains("<tr data-id="))
                        {
                            stepcount++;
                            id = line.Replace("<tr data-id=\"", "").Replace("\">", "").Trim();
                        }

                        else if (line.Contains("<td>") && line.Contains("</td>")) // got name info step=>2
                        {
                            stepcount++;
                            name = line.Replace("<td>", "").Replace("</td>", "").Trim();
                           
                        }
                        else if(line.Contains("<span>") && line.Contains("</span>")) //step=> 3-5
                        {
                            line = line.Replace("<span>", "").Replace("</span>", "").Trim();
                            stepcount++;
                            switch (stepcount)
                            {
                                case 3:
                                    price = Convert.ToInt32(line);
                                    break;
                                case 4:
                                    count = Convert.ToInt32(line);

                                    break;
                                case 5:
                                    if (line.Length < 1) modrank = -1;
                                    else modrank = Convert.ToInt32(line);
                                    stepcount = 0;
                                    offers.Add(new WarframeItem(name, price, count, modrank, sell,id));
                                    break;

                                default:
                                    break;
                            }
                        }

                    }
                    return offers;

                }


            }

        }

        public bool removeItem(WarframeItem item)
        {

            using (HttpWebResponse response = Webhelper.postPage("http://warframe.market/api/remove_order",session,$"id={item.id}&count={item.count}"))
            {

                return (new StreamReader(response.GetResponseStream())).ReadToEnd().Contains("200");
            }

        }

        public bool soldItem(WarframeItem item)
        {
            using (HttpWebResponse response = Webhelper.postPage("http://warframe.market/api/bs_order", session, $"id={item.id}&count={item.count}"))
            {

                return (new StreamReader(response.GetResponseStream())).ReadToEnd().Contains("200");
            }
        }
        
        public bool editItem(WarframeItem item)
        {
            using (HttpWebResponse response = Webhelper.postPage("http://warframe.market/api/edit_order", session, $"id={item.id}&new_count={item.count}&new_price={item.price}"))
            {

                return (new StreamReader(response.GetResponseStream())).ReadToEnd().Contains("200");
            }
        }

        public bool addItem(WarframeItem item)
        {
            if (!nameTypeMap.ContainsKey(item.name)) return false;
            string sellType = item.sellOffer ? "sell" : "buy";

            string postData = $"item_name={item.name.Replace(' ', '+')}&item_type={nameTypeMap[item.name]}&action_type={sellType}&item_quantity={item.count}&platina={item.price}";

            
            using (HttpWebResponse response = Webhelper.postPage("http://warframe.market/api/place_order", session, postData))
            {

                return (new StreamReader(response.GetResponseStream())).ReadToEnd().Contains("200");
            }

        }

        #endregion


        public void test()
        {
           

        }


        public void Dispose()
        {
            try
            {
                stateEnfore.Abort();
            }
            catch(Exception ex)
            {

            }

            setOffline();
        }
    }
}
