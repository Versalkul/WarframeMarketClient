using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarframeMarketClient.Model
{
    public class WarframeItem : INotifyPropertyChanged
    {

        public static Dictionary<string, Tuple<string, int>> itemInfoMap = new Dictionary<string, Tuple<string, int>>(1000); 

        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; OnPropertyChanged(nameof(Category)); }
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

        public int? ModRankDisplay { get { return ModRank < 0 ? null as int? : ModRank;  } }

        public ObservableCollection<int> ModRanks { get {
                // TODO: Find ModMaxRank
                return new ObservableCollection<int>() { 0, 1, 2, 3 };
            }
        }

        public string Category { get
            {
                return WarframeMarketClient.Logic.MarketManager.GetCategory(Name);
            }
        }

        /// <summary>
        /// Needed for DataGrid AllowUserAddItem
        /// </summary>
        public WarframeItem()
        {
            Console.WriteLine("New Item!");
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



        public void DecreaseCount()
        {
            ApplicationState.getInstance().Market.SoldItem(this);
            if (count > 1) Count--;
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
