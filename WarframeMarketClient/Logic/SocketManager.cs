using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarframeMarketClient.Extensions;
using WarframeMarketClient.Model;
using WebSocket4Net;

namespace WarframeMarketClient.Logic
{
    class SocketManager : IDisposable
    {

        bool disposed = false;
        WebSocket socket;
        public event EventHandler<PmArgs> recievedPM;

        public bool SocketWasClosed { get; set; } = false;

        private Queue<string> jsonsToSend = new Queue<string>(5);

        public SocketManager():this(ApplicationState.getInstance().SessionToken)
        {
        }
        public SocketManager(string cookie)
        {
            List<KeyValuePair<string, string>> cookies = new List<KeyValuePair<string, string>>(1);
            cookies.Add(new KeyValuePair<string, string>("session", cookie));
            socket = new WebSocket("wss://warframe.market/socket", "", cookies);
            socket.Opened += new EventHandler(onOpen);
            socket.Error += new EventHandler<SuperSocket.ClientEngine.ErrorEventArgs>(onError);
            socket.MessageReceived += new EventHandler<MessageReceivedEventArgs>(onMessage);
            socket.Closed += Socket_Closed;
            socket.EnableAutoSendPing = false;
        }

        #region events

        private void Socket_Closed(object sender, EventArgs e)
        {

            ApplicationState.getInstance().Logger.Log("CLOSED SOCKET ",false);
            SocketWasClosed = true;
            if (ApplicationState.getInstance().OnlineState.IsOnline())
            {
                ApplicationState.getInstance().Market.EnsureSocketState(); // hacky solution for socket closing every 30 min (+-3 min)
                ApplicationState.getInstance().Logger.Log("REOPENING CLOSED SOCKET", false);

            }

        }


        private void onError(object caller, SuperSocket.ClientEngine.ErrorEventArgs args)
        {
            ApplicationState.getInstance().Logger.Log("Error from websocket:",false);
            ApplicationState.getInstance().Logger.Log(args.Exception.Message,false);
            

        }

        public void onMessage(object caller, MessageReceivedEventArgs args)
        {
            ApplicationState.getInstance().Logger.Log("Got Data from websocket", false);
            if (recievedPM != null && args.Message.Contains("recive_message"))
            {
                SocketMessage msg = JsonConvert.DeserializeObject<SocketMessage>(args.Message);
                recievedPM.Invoke(this, new PmArgs(msg.data.text, msg.data.from));
            }
            Console.WriteLine(args.Message);
        }


        private void onOpen(object sender, EventArgs args)
        {
            ApplicationState.getInstance().Logger.Log("Socket open",false);
            SocketWasClosed = true;
            lock (socket)
            {

            if (jsonsToSend.Count != 0)
                while (jsonsToSend.Count > 0)
                    socket.Send(jsonsToSend.Dequeue());
                return; //Remove
            if (ApplicationState.getInstance().OnlineState == OnlineState.OFFLINE|| ApplicationState.getInstance().OnlineState==OnlineState.DISABLED)
            {
                    ApplicationState.getInstance().Logger.Log("CLOSING SOCKET after opening", false);
                    socket.Close();
                }
            }
        }

        #endregion


        public void sendMessage(string message, string sendTo)
        {
            string send = new SendMessageJson(message, sendTo).ToString();
            sendJson(send);

        }

        public void EnsureOpenSocket()
        {
            lock (socket)
            {
                if (socket.State == WebSocketState.Closed||socket.State==WebSocketState.None) socket.Open();
            }
        }

        private void sendJson(string json)
        {
            ApplicationState.getInstance().Logger.Log("Sending json",false);
            lock (socket)
            {
                if (socket.State == WebSocketState.Closed || socket.State == WebSocketState.None|| socket.State == WebSocketState.Connecting)
                {
                    jsonsToSend.Enqueue(json);
                    if(socket.State != WebSocketState.Connecting) socket.Open();
                }
                else
                {
                    socket.Send(json);
                }
                    
            }
        }

        private void sendIngameJson()
        {
            ApplicationState.getInstance().Logger.Log("Trying to go ingame",false);
            sendJson("{\"destination\":\"user.set_status\",\"data\":{\"status\":\"in_game\"}}");
        }

        public void setIngame()
        {
            sendIngameJson();
        }

        public void setOnline()
        {
            ApplicationState.getInstance().Logger.Log("Trying to go online",false);
            sendJson("{\"destination\":\"user.set_status\",\"data\":{\"status\":\"online\"}}");
        }

        public void setOffline()
        { 
            sendJson("{\"destination\":\"user.set_status\",\"data\":{\"status\":\"online\"}}");
            lock (socket)
            {
                if (socket.State == WebSocketState.Open && jsonsToSend.Count == 0)
                    socket.Close();
            }
            ApplicationState.getInstance().Logger.Log("CLOSING SOCKET GOING OFFLINE",false);
        }

        public void Dispose()
        {
            ApplicationState.getInstance().Logger.Log("Disposing", false);
            if (disposed) return;
            disposed = true;
            setOffline();
            socket.Dispose();
        }

        ~SocketManager()
        {
           if(!disposed) Dispose();
        }
    }
}
