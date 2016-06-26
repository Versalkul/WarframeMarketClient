using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public SocketManager()
        {
            List<KeyValuePair<string, string>> cookies = new List<KeyValuePair<string, string>>(1);
            cookies.Add(new KeyValuePair<string, string>("session", ApplicationState.getInstance().SessionToken));
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

            ApplicationState.getInstance().Logger.Log("CLOSED SOCKET",false);
            SocketWasClosed = true;

        }


        private void onError(object caller, SuperSocket.ClientEngine.ErrorEventArgs args)
        {
            Console.WriteLine("Error from websocket:");
            Console.WriteLine(args.Exception.Message);
            Console.WriteLine(args.Exception);

        }

        public void onMessage(object caller, MessageReceivedEventArgs args)
        {
            if (recievedPM != null && args.Message.Contains("recive_message"))
            {
                SocketMessage msg = JsonConvert.DeserializeObject<SocketMessage>(args.Message);
                recievedPM.Invoke(this, new PmArgs(msg.data.text, msg.data.from));
            }
            Console.WriteLine(args.Message);
        }


        private void onOpen(object sender, EventArgs args)
        {
            SocketWasClosed = true;
            lock (socket)
            {

            if (jsonsToSend.Count != 0)
                while (jsonsToSend.Count > 0)
                    socket.Send(jsonsToSend.Dequeue());

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
            ApplicationState.getInstance().Logger.Log("Send Message SocketManager " +message+" to "+sendTo);

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
            sendJson("{\"destination\":\"user.set_status\",\"data\":{\"status\":\"in_game\"}}");
        }

        public void setIngame()
        {
            sendIngameJson();
        }

        public void setOnline()
        {
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
            ApplicationState.getInstance().Logger.Log("CLOSING SOCKET GOING OFFLINE");
        }

        public void Dispose()
        {
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
