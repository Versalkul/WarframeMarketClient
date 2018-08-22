using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WarframeMarketClient.Extensions;
using WarframeMarketClient.Model;
using WebSocketSharp;

namespace WarframeMarketClient.Logic
{
    class SocketManager : IDisposable
    {

        bool disposed = false;
        WebSocket socket;
        public event EventHandler<PmArgs> recievedPM;

        public bool SocketWasClosed { get; set; } = false;

        private Queue<string> jsonsToSend = new Queue<string>(5);

        public SocketManager() : this(ApplicationState.getInstance().SessionToken)
        {
        }
        public SocketManager(string cookie)
        {

            socket = new WebSocket("wss://warframe.market/socket");
            socket.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12;

            socket.SetCookie(new WebSocketSharp.Net.Cookie("JWT", cookie));
            socket.OnOpen += new EventHandler(onOpen);
            socket.OnError += new EventHandler<WebSocketSharp.ErrorEventArgs>(onError);
            socket.OnMessage += new EventHandler<MessageEventArgs>(onMessage);
            socket.OnClose += Socket_Closed;
            socket.Connect();
            Console.WriteLine("Open");
        }

        #region events

        private void Socket_Closed(object sender, EventArgs e)
        {

            ApplicationState.getInstance().Logger.Log("CLOSED SOCKET ", false);
            SocketWasClosed = true;
            if (ApplicationState.getInstance().OnlineState.IsOnline())
            {
                ApplicationState.getInstance().Market.EnsureSocketState(); // hacky solution for socket closing every 30 min (+-3 min)
                ApplicationState.getInstance().Logger.Log("REOPENING CLOSED SOCKET", false);

            }

        }


        private void onError(object caller, ErrorEventArgs args)
        {
            ApplicationState.getInstance().Logger.Log("Error from websocket:", false);
            ApplicationState.getInstance().Logger.Log(args.Exception.Message, false);


        }

        public void onMessage(object caller, MessageEventArgs args)
        {
            ApplicationState.getInstance().Logger.Log("Got Data from websocket", false);
            if (recievedPM != null && args.Data.Contains("recive_message"))
            {
                SocketMessage msg = JsonConvert.DeserializeObject<SocketMessage>(args.Data);
                recievedPM.Invoke(this, new PmArgs(msg.data.text, msg.data.from));
            }
            Console.WriteLine(args.Data);
        }


        private void onOpen(object sender, EventArgs args)
        {
            ApplicationState.getInstance().Logger.Log("Socket open", false);
            SocketWasClosed = true;
            lock (socket)
            {

                if (jsonsToSend.Count != 0)
                    while (jsonsToSend.Count > 0)
                        socket.Send(jsonsToSend.Dequeue());
                return; //Remove
                if (ApplicationState.getInstance().OnlineState == OnlineState.OFFLINE || ApplicationState.getInstance().OnlineState == OnlineState.DISABLED)
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
                if (!socket.IsAlive) socket.Connect();
            }
        }

        private void sendJson(string json)
        {
            ApplicationState.getInstance().Logger.Log("Sending json", false);
            lock (socket)
            {
                if (!socket.IsAlive)
                {
                    jsonsToSend.Enqueue(json);
                    socket.Connect();
                }
                else
                {
                    socket.Send(json);
                }

            }
        }

        private void sendIngameJson()
        {
            ApplicationState.getInstance().Logger.Log("Trying to go ingame", false);
            sendJson("{\"type\": \"@WS/USER/SET_STATUS\", \"payload\": \"ingame\"}");
        }

        public void setIngame()
        {
            sendIngameJson();
        }

        public void setOnline()
        {
            ApplicationState.getInstance().Logger.Log("Trying to go online", false);
            sendJson("{\"type\": \"@WS/USER/SET_STATUS\", \"payload\": \"online\"}");
        }

        public void setOffline()
        {
            sendJson("{\"type\": \"@WS/USER/SET_STATUS\", \"payload\": \"invisible\"}");
            ApplicationState.getInstance().Logger.Log("GOING OFFLINE", false);
        }

        public void Dispose()
        {
            ApplicationState.getInstance().Logger.Log("Disposing", false);
            if (disposed) return;
            disposed = true;
            setOffline();
            socket.Close();
        }

        ~SocketManager()
        {
            if (!disposed) Dispose();
        }
    }
}
