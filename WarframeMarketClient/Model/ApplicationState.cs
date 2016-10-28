using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Data;
using WarframeMarketClient.Logic;
using WarframeMarketClient.ViewModel;

namespace WarframeMarketClient.Model
{
    public class ApplicationState : INotifyPropertyChanged 
    {
        #region Singleton

        private static ApplicationState instance;
        private static object buyItemsLock = new object();
        private static object sellItemsLock = new object();
        private static object chatLock = new object();
        private static bool init=false;
        public static ApplicationState getInstance()
        {

            if (!init)
            {

                if (instance == null)
                {
                    init = true;
                    instance = new ApplicationState();
                    instance.Initialize();
                }
            }

            return instance;
        }

        private ApplicationState()
        {

            OnlineState = OnlineState.ERROR;
            BuyItems = new ObservableCollection<WarframeItem>();
            SellItems = new ObservableCollection<WarframeItem>();
            Chats = new ObservableCollection<ChatViewModel>();
            BindingOperations.EnableCollectionSynchronization(sellItems, sellItemsLock);
            BindingOperations.EnableCollectionSynchronization(buyItems, buyItemsLock);
            BindingOperations.EnableCollectionSynchronization(chats, chatLock);
        }

        private void Initialize() // Initializes that need the ApplicationState
        {
            if (Logger != null) return; // just to be sure
            Logger = new Logger();
            ItemMap.LoadMap();
            (new SaveLoadFile()).ExtractStandartSounds();
            Settings = new Settings();
            Settings.LoadSettings();
            Task.Factory.StartNew(() => { ItemMap.getTypeMap();ItemMap.SaveMap(); }); // inits the webapi and gets an usefull result
            Plimper = new SoundViewModel();
            Task.Factory.StartNew(() => LatestVersion = Webhelper.CheckUpdate());
        }

        #endregion


        #region Properties

        #region SessionToken
        private string sessionToken="";

        public string SessionToken
        {
            get
            {

                return sessionToken;
            }
            set
            {
                if (value==null||value.Length < 10) return;
                #region clean old
                if(Market!=null)Market.Dispose();
                Market = null;
                if (OnlineChecker != null) OnlineChecker.Dispose();
                OnlineChecker = null;
                Chats.Clear();
                SellItems.Clear();
                BuyItems.Clear();

                #endregion
                Username = "Temp";
                OnPropertyChanged(nameof(IsValid));
                sessionToken = value.Replace(";", "").Replace(",", "").Replace("\"","").Trim();
                ValidationProgress = 5;
                OnlineState = OnlineState.VALIDATING;
                Task.Factory.StartNew(() =>
                {
                    Tuple<bool, string> verification = HtmlParser.verifyToken();
                    ValidationProgress += 35;
                    if (!verification.Item1)
                    {
                        Username = "";
                        OnlineState= OnlineState.ERROR;
                        return;
                    }
                    Username = verification.Item2;
                    Console.WriteLine("Logged in as " + Username);
                    if (OnlineChecker != null) OnlineChecker.Dispose();
                    
                    Market = new MarketManager();
                    ValidationProgress = 100;
                    OnlineChecker = new RunsGameChecker(); 
                    OnlineState = DefaultState;
                    OnPropertyChanged(nameof(IsValid));

                });
            }
        }
        #endregion

        public string Username { get; set; } = "";

        #region OnlineState
        private OnlineState onlineState;
        public OnlineState OnlineState { get
            {
                return onlineState ;
                    }
            set {
                if (onlineState == value) return;
                onlineState = value;
                if (Market != null) Market.UpdateState();
                OnPropertyChanged(nameof(OnlineState));
                }
        }

        private OnlineState defaultState = OnlineState.OFFLINE;

        public OnlineState DefaultState
        {
            get { return defaultState; }
            set { defaultState = value; OnPropertyChanged(nameof(DefaultState)); if (OnlineState == OnlineState.ONLINE || OnlineState == OnlineState.OFFLINE) OnlineState = DefaultState; }
        }
        #endregion

        #region Items n Chats

        private event EventHandler<List<ChatMessage>> newMessage;

        public event EventHandler<List<ChatMessage>> NewMessage { add { newMessage += value; } remove { newMessage -= value; } }


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
        #endregion

        #region Validating etc props

        public bool HasUsername { get { return !String.IsNullOrWhiteSpace(instance.Username); } }

        public bool IsValid { get { return HasUsername&&Username!="Temp"&&OnlineChecker!=null; } }

        public bool IsValidating { get { return !String.IsNullOrWhiteSpace(instance.Username) && (instance.Username == "Temp" || OnlineChecker == null); } }

        private int validationProgress=0;

        public int ValidationProgress
        {
            get { return validationProgress; }
            set { validationProgress = value;OnPropertyChanged(nameof(ValidationProgress)); }
        }

        #endregion

        #region Version
        public Version Version { get { return System.Reflection.Assembly.GetEntryAssembly().GetName().Version; } }

        private Version latestVersion;
        public Version LatestVersion
        {
            get { return latestVersion; }
            set {
                latestVersion = value;
                OnPropertyChanged("LatestVersion");
                OnPropertyChanged("UpdateAvailable");
            }
        }

        public bool UpdateAvailable
        {
            get
            {
                return LatestVersion != null &&
                    (Version.Major < LatestVersion.Major || Version.Minor < LatestVersion.Minor || Version.Build < LatestVersion.Build);
            }
        }

        #endregion

        public MarketManager Market { get; private set; }

        public RunsGameChecker OnlineChecker { get; private set; }

        public Settings Settings { get; set; } 

        public SoundViewModel Plimper { get; set; }

        public Logger Logger { get; private set; }

        #endregion


        public void InvokeNewMessage(object o,List<ChatMessage> msg)
        {
            newMessage?.Invoke(o, msg);
        }


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
