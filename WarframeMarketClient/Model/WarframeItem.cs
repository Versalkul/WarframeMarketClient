using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace WarframeMarketClient.Model
{
    public class WarframeItem : INotifyPropertyChanged, IDataErrorInfo // Implement IEditableObject 
    {
        public static Dictionary<string, Tuple<string, int>> itemInfoMap = new Dictionary<string, Tuple<string, int>>(1000); 


        #region Properties

        #region Native Properties
        private string name;

        public string Name
        {
            get { return name; }
            set {
                if (itemInfoMap.ContainsKey(value))
                {
                    name = value;
                    ModRank = Math.Min(0, itemInfoMap[name].Item2);
                }
                //name = value;
                // else name = ""; // Keep old name
                OnPropertyChanged(nameof(Category));
                OnPropertyChanged(nameof(MaxRank));
                OnPropertyChanged(nameof(ModRank));
                OnPropertyChanged(nameof(ModRanks));
            }
        }

        
        public int Price { get; set; }
        private int count;

        public int Count
        {
            get { return count; }
            set { count = value; }
        }

        public int ModRank { get; set; }
        public bool SellOffer { get; set; }
        public string Id { get; set; }
        #endregion

        #region Derivated Properties
        public int MaxRank { get { return itemInfoMap[Name].Item2; } }

        public IEnumerable<int> ModRanks { get {
                return Enumerable.Range(0, MaxRank+1);
            }
        }

        public string Category { get
            {
                return WarframeMarketClient.Logic.MarketManager.GetCategory(Name);
            }
        }
        
        public List<string> AllItemNames { get { return itemInfoMap.Keys.ToList(); } }

        #region IDataErrorInfo

        public string Error
        {
            get
            {
                Console.WriteLine(" Called Error");
                if (Name == "" || Count <= 0 || Price <= 0) return "Single DATA ERROR";
                return null;
            }
        }

        public string this[string columnName] // WTFH IS THIS SYNTAX O.o
        {
            get
            {
                // apply property level validation rules
                if (columnName == "Name")
                {
                    if (!itemInfoMap.ContainsKey(Name))
                        return "Wrong Name";
                }
                
                if (columnName == "Count")
                {
                    if (Count<=0)
                        return "Wrong Count";
                }
                if (columnName == "Price")
                {
                    if (Price <= 0)
                        return "Wrong Price";
                }
                return "";
            }
        }

        #endregion


        #endregion

        #endregion


        #region Constructors

        /// <summary>
        /// Needed for DataGrid AllowUserAddItem
        /// </summary>
        public WarframeItem()
        {
        }

        public WarframeItem(string name,int price,int count,bool sellOffer)
        {
            this.Name = name;
            this.Price = price;
            this.Count = count;
            this.SellOffer = sellOffer;
            ModRank = -1;
        }

        public WarframeItem(string name, int price, int count,int modRank, bool sellOffer):this(name, price, count, sellOffer)
        {
            this.ModRank = modRank;
        }

        public WarframeItem(string name, int price, int count, bool sellOffer,string id):this(name, price, count, sellOffer)
        {
            ModRank = -1;
            this.Id = id;
        }

        public WarframeItem(string name, int price, int count, int modRank, bool sellOffer,string id) : this(name, price, count, sellOffer, id)
        {
            this.ModRank = modRank;
        }

        #endregion


        #region Methods

        public void CommitAdd()
        {
            Console.WriteLine("Added!");
        }

        public void DecreaseCount()
        {
            ApplicationState.getInstance().Market.SoldItem(this);
            if (count > 1)
            {
                Count--;
                OnPropertyChanged(nameof(Count));
            }
            else
            {
                if (SellOffer) ApplicationState.getInstance().SellItems.Remove(this);
                else ApplicationState.getInstance().BuyItems.Remove(this);
            }
        }

        public void RemoveItem()
        {
            Console.WriteLine("Remove!");
            ApplicationState.getInstance().Market.RemoveItem(this);
            if (SellOffer) ApplicationState.getInstance().SellItems.Remove(this);
            else ApplicationState.getInstance().BuyItems.Remove(this);
        }

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
