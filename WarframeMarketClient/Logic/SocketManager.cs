﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarframeMarketClient.Model;
using WebSocket4Net;

namespace WarframeMarketClient.Logic
{
    class SocketManager:IDisposable
    {
        WebSocket socket;
        public event EventHandler<PmArgs> recievedPM;
        private Queue<string> jsonsToSend = new Queue<string>(5);
        private static SocketManager socketMgr;

        private SocketManager()
        {
            List<KeyValuePair<string, string>> cookies = new List<KeyValuePair<string, string>>(1);
            cookies.Add(new KeyValuePair<string, string>("session", ApplicationState.getInstance().SessionToken));
            socket = new WebSocket("wss://warframe.market/socket", "", cookies);
            socket.Opened += new EventHandler(onOpen);
            socket.Error += new EventHandler<SuperSocket.ClientEngine.ErrorEventArgs>(onError);
            socket.MessageReceived += new EventHandler<MessageReceivedEventArgs>(onMessage);
        }


        public static SocketManager getInstance()
        {
            if (socketMgr == null) socketMgr = new SocketManager();
            return socketMgr;
        }

        public static void Invalidate()
        {
            socketMgr.Dispose();
            socketMgr = null;
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

            if (recievedPM != null && args.Message.Contains("recive_message"))
            {
                SocketMessage msg = JsonConvert.DeserializeObject<SocketMessage>(args.Message);

                recievedPM.Invoke(this, new PmArgs(msg.data.text, msg.data.from));

            }
            Console.WriteLine(args.Message);
        }


        private void onOpen(object sender, EventArgs args)
        {

            if (jsonsToSend.Count != 0)
            {

                while (jsonsToSend.Count != 0)
                {
                    socket.Send(jsonsToSend.Dequeue());
                }

            }
            if (ApplicationState.getInstance().OnlineState==OnlineState.OFFLINE) socket.Close();

        }

        #endregion

        public void sendMessage(string to, string text)
        {
            sendJson("{\"destination\":\"user.send_message\",\"data\":{\"text\":\"" + text + "\",\"message_to\":\"" + to + "\"}}");
        }



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
            sendIngameJson();
        }

        public void setOnline()
        {
            sendJson("{\"destination\":\"user.set_status\",\"data\":{\"status\":\"online\"}}");
        }

        public void setOffline()
        {
            sendJson("{\"destination\":\"user.set_status\",\"data\":{\"status\":\"online\"}}");
            if (socket.State == WebSocketState.Open && jsonsToSend.Count == 0) socket.Close();
        }

        public void Dispose()
        {
            setOffline();
        }
    }
}