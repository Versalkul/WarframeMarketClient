using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WarframeMarketClient.Model;

namespace WarframeMarketClient.Logic
{
    class DebugFunctions
    {
        const string token = "0a70510e-d509-4546-92b0-346214fe047f._LT-kGrXImEcgtRvoZRXnWxiA8U";
        static SocketManager man = new SocketManager(token);

        public static void TryMessageWuerstchen(string message)
        {

            man.sendMessage(message, "WuerstchenHD");
        }

        public static void OnlineOfflineCheck()
        {
            Thread.Sleep(10000);
            for(int i = 1; i < 10; i++)
            {

                 man.setIngame();
                Thread.Sleep(10000);
                OnlineState myState = ApplicationState.getInstance().Market.getStatusOnSite("Versalkul",false);
                if  (myState!= OnlineState.INGAME)
                {
                    ApplicationState.getInstance().Logger.Log("Tried to set Versalkul Ingame It failed "+myState,false);
                   TryMessageWuerstchen("Failed to set onlinestate ingame" +DateTime.Now);
                }
                else
                {
                    ApplicationState.getInstance().Logger.Log("Set Versalkul to ingame",false);
                }
                man.setOnline();
                Thread.Sleep(10000);
                myState = ApplicationState.getInstance().Market.getStatusOnSite("Versalkul", false);
                if (myState != OnlineState.ONLINE)
                {
                    ApplicationState.getInstance().Logger.Log("Tried to set Versalkul online It failed " +myState,false);
                    TryMessageWuerstchen("Failed to set onlinestate online" +DateTime.Now);
                }
                else
                {
                    ApplicationState.getInstance().Logger.Log("Set Versalkul to online",false);
                }
            }
        }

        

    }
}
