using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarframeMarketClient.Logic;
using WarframeMarketClient.ViewModel;

namespace WarframeMarketClient.Model
{
    class ApplicationState : INotifyPropertyChanged
    {
        #region Singleton

        private static ApplicationState instance;


        public static ApplicationState getInstance()
        {
            if (instance == null)
            {
                instance = new ApplicationState();
            }

            return instance;
        }

        private ApplicationState()
        {
            OnlineState = OnlineState.ONLINE;
            BuyItems = new ObservableCollection<WarframeItem>();
            SellItems = new ObservableCollection<WarframeItem>() {
                new WarframeItem("Nyx Prime Systems",5,3,true),
                new WarframeItem("Loki Prime Systems",150,1,true),
                new WarframeItem("Buzz Kill",150,1,5,true),
            };
            Chats = new ObservableCollection<ChatViewModel>()
                 {
                 new ChatViewModel(
                    new User("RandomGuy") {State=OnlineState.ONLINE },
                     new ObservableCollection<ChatMessage>() {
                        new ChatMessage() {
                            MessageFrom = "ME",
                            SendHour = "12",
                            SendMinute = "10",
                            Message ="BLablabla"
                        },
                        new ChatMessage() { Message = "Test", SendHour="20", SendMinute="15", MessageFrom="ME" },
                        new ChatMessage() { Message = "Passt", SendHour="20", SendMinute="16", MessageFrom="B" },
                        new ChatMessage() { Message = "Test", SendHour="20", SendMinute="15", MessageFrom="ME" },
                        new ChatMessage() { Message = "Passt", SendHour="20", SendMinute="16", MessageFrom="B" },
                        new ChatMessage() { Message = "Nice!", SendHour="20", SendMinute="17", MessageFrom="ME" },
                        new ChatMessage() { Message = "Test", SendHour="20", SendMinute="15", MessageFrom="ME" },
                        new ChatMessage() { Message = "Passt", SendHour="20", SendMinute="16", MessageFrom="B" },
                        new ChatMessage() { Message = "Nice!", SendHour="20", SendMinute="17", MessageFrom="ME" },
                        new ChatMessage() { Message = "Test", SendHour="20", SendMinute="15", MessageFrom="ME" },
                        new ChatMessage() { Message = "Passt", SendHour="20", SendMinute="16", MessageFrom="B" },
                        new ChatMessage() { Message = "Nice!", SendHour="20", SendMinute="17", MessageFrom="ME" },
                        new ChatMessage() { Message = "Test", SendHour="20", SendMinute="15", MessageFrom="ME" },
                        new ChatMessage() { Message = "Passt", SendHour="20", SendMinute="16", MessageFrom="B" }
                    }
                ),
                new ChatViewModel(
                    new User("RandomGuy2") {State=OnlineState.INGAME },
                     new ObservableCollection<ChatMessage>() {
                        new ChatMessage() {
                            MessageFrom = "ME",
                            SendHour = "12",
                            SendMinute = "10",
                            Message ="BLablabla"
                        },
                        new ChatMessage() { Message = "Test", SendHour="20", SendMinute="15", MessageFrom="ME" },
                        new ChatMessage() { Message = "Passt", SendHour="20", SendMinute="16", MessageFrom="B" },
                        new ChatMessage() { Message = "Nice!", SendHour="20", SendMinute="17", MessageFrom="ME" },
                        new ChatMessage() { Message = "Test", SendHour="20", SendMinute="15", MessageFrom="ME" },
                        new ChatMessage() { Message = "Passt", SendHour="20", SendMinute="16", MessageFrom="B" },
                        new ChatMessage() { Message = "Nice!", SendHour="20", SendMinute="17", MessageFrom="ME" },
                        new ChatMessage() { Message = "Test", SendHour="20", SendMinute="15", MessageFrom="ME" },
                        new ChatMessage() { Message = "Passt", SendHour="20", SendMinute="16", MessageFrom="B" },
                        new ChatMessage() { Message = "Nice!", SendHour="20", SendMinute="17", MessageFrom="ME" },
                        new ChatMessage() { Message = "Test", SendHour="20", SendMinute="15", MessageFrom="ME" },
                        new ChatMessage() { Message = "Passt", SendHour="20", SendMinute="16", MessageFrom="B" },
                    }
                )
            };
            Username = "ME";


        }

        #endregion


        #region Properties

        public string sessionToken="";

        public string SessionToken
        {
            get
            {

                return sessionToken;
            }
            set
            {
                Username = "Temp";
                sessionToken = value;
                Tuple<bool, string> verification = HtmlParser.verifyToken();
                if (!verification.Item1)
                {
                    Username = "";
                    return;
                }
                Username = verification.Item2;
                OnlineState= OnlineState.OFFLINE;
                Console.WriteLine("logged in as " + Username);
                if (Market != null) Market.Dispose();
                if (OnlineChecker != null) OnlineChecker.Dispose();
                Market = new MarketManager();
                Console.WriteLine("Token set");
                OnlineChecker = new RunsGameChecker();
                OnPropertyChanged(nameof(IsValid));
            }
        } 

        
        public MarketManager Market { get; private set; }

        public string Username { get; set; } = "";


        private OnlineState onlineState;
        public OnlineState OnlineState { get
            {
                return onlineState ;
                    }
            set {
                onlineState = value;
                if (Market != null) Market.forceUserState();
                }
        }
        
        public OnlineState DefaultState { get; set; }



        private ObservableCollection<WarframeItem> sellItems;
        public ObservableCollection<WarframeItem> SellItems
        {
            get { return sellItems; }
            set {
                sellItems = value;
                OnPropertyChanged(nameof(SellItems));
            }
        }


        private ObservableCollection<WarframeItem> buyItems;
        public ObservableCollection<WarframeItem> BuyItems
        {
            get { return buyItems; }
            set
            {
                buyItems = value;
                OnPropertyChanged(nameof(BuyItems));
            }
        }


        private ObservableCollection<ChatViewModel> chats;
        public ObservableCollection<ChatViewModel> Chats
        {
            get { return chats; }
            set {
                chats = value;
                OnPropertyChanged(nameof(Chats));
            }
        }

        public TimeSpan TimeOffset { get; set; }

        public static bool HasInstance { get { return instance != null; } }
        public static bool HasValidInstance { get { return HasInstance && instance.IsValid; } }

        public bool IsValid { get { return !String.IsNullOrWhiteSpace(instance.Username); } }
        public RunsGameChecker OnlineChecker { get; private set; }

        #endregion



        #region OnPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string property)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(property));
        }
        #endregion
    }
}
