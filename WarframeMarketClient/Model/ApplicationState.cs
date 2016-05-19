using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
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

            OnlineState = OnlineState.ERROR;
            BuyItems = new ObservableCollection<WarframeItem>();
            SellItems = new ObservableCollection<WarframeItem>();
            Chats = new ObservableCollection<ChatViewModel>();
            BindingOperations.EnableCollectionSynchronization(sellItems, sellItemsLock);
            BindingOperations.EnableCollectionSynchronization(buyItems, buyItemsLock);
            BindingOperations.EnableCollectionSynchronization(chats, chatLock);
            asynchRun(() => WarframeItem.itemInfoMap=MarketManager.getTypeMap()); // inits the webapi and gets an usefull result


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
                if (value.Length < 10) return;
                #region clean old
                if(Market!=null)Market.Dispose();
                Market = null;
                if (Market != null) OnlineChecker.Dispose();
                OnlineChecker = null;
                Chats.Clear();
                SellItems.Clear();
                BuyItems.Clear();

                #endregion
                Username = "Temp";
                sessionToken = value;
                ValidationProgress = 0;
                asynchRun(() =>
                {
                    
                    OnlineState = OnlineState.VALIDATING;
                    Tuple<bool, string> verification = HtmlParser.verifyToken();
                    ValidationProgress += 10;
                    if (!verification.Item1)
                    {
                        Username = "";
                        OnlineState= OnlineState.ERROR;
                        return;
                    }
                    Username = verification.Item2;
                    Console.WriteLine("Logged in as " + Username);
                    if (OnlineChecker != null) OnlineChecker.Dispose();

                    Thread.Sleep(5000);

                    Market = new MarketManager();
                    ValidationProgress = 100;
                    OnlineChecker = new RunsGameChecker(); 
                    OnlineState = DefaultState;
                    OnPropertyChanged(nameof(IsValid));

                });
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
                if (onlineState == value) return;
                onlineState = value;
                if (Market != null) Market.forceUserState();
                OnPropertyChanged(nameof(OnlineState));
                }
        }

        public OnlineState DefaultState { get; set; } = OnlineState.OFFLINE;



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
        #region Validating etc props

        public static bool HasInstance { get { return instance != null; } }
        public static bool HasValidInstance { get { return HasInstance && instance.HasUsername; } }

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
        public RunsGameChecker OnlineChecker { get; private set; }


        #endregion


        public  void asynchRun(Action function)
        {
            (new Thread(() =>
            {
                // add try catch
                function.Invoke();
            }
            )).Start();

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
