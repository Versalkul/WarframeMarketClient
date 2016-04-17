using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarframeMarketClient.Logic
{
    class MarketController : IDisposable
    {
        public OnlineManager setter;
        RunsGameChecker checker;
        bool running = false;
        public event EventHandler<GameStateChangedArgs> changedStatus;
        public event EventHandler<PmArgs> pmRecieved { add { setter.recievedPM += value; }  remove { setter.recievedPM -= value; } }
        bool ingame;
        public bool stayOnline = false;

        public MarketController(string session)
        {
            setter = new OnlineManager(session);
            checker = new RunsGameChecker();
            checker.Start();
        }

        public void enableControler()
        {
            if (running) return;
            running = true;
            checker.gameChanged += new EventHandler<GameStateChangedArgs>(gameStateChanged);
            checker.afkChanged += new EventHandler<AfkStateChangedArgs>(afkStateChenged);
        }

        public void sendMessage(string message,string user)
        {
            setter.sendMessage(user, message);
        }
        public void closeChat(string user)
        {
            setter.closeChat(user);
        }

        public List<PmData> getMsg(string user)
        {

            return setter.getMessages(user).Select(x=> new PmData() { message = x.message, time = x.send_hour.ToString()+':'+x.send_minute, fromMe = setter.username==user }).ToList();

        }

        public void disableController()
        {
            if (!running) return;
            running = false;
            checker.gameChanged -= new EventHandler<GameStateChangedArgs>(gameStateChanged);
            checker.afkChanged -= new EventHandler<AfkStateChangedArgs>(afkStateChenged);
        }

        private void gameStateChanged(object caller,GameStateChangedArgs args)
        {

            ingame = args.newGamestate == GameState.running;

            if (ingame)
            {
                setter.setIngame();
            }
            else
            {
                if (stayOnline) setter.setOnline();
                else setter.setOffline();
            }

            if (changedStatus != null) changedStatus.Invoke(this, new GameStateChangedArgs(args.newGamestate));
        }


        private void afkStateChenged(object caller, AfkStateChangedArgs args)
        {

            if (args.newAfkState == AfkState.Afk) setter.setOffline();
            else if (checker.GameOnline) setter.setIngame();
        }
        public void Dispose()
        {
            Console.WriteLine("Disposing objects");
            setter.setOffline();
            checker.Dispose();
            setter.Dispose();
        }
    }
}
