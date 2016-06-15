using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro.Controls.Dialogs;
using MahApps.Metro.Controls;
using WarframeMarketClient.Logic;

namespace WarframeMarketClient.Model
{
    public class WarframeItem : INotifyPropertyChanged, IDataErrorInfo, IEditableObject, IEquatable<WarframeItem>
    {
        
        private WarframeItem backUp;

        /// <summary>
        /// Backup for Editing
        /// </summary>
        public WarframeItem BackUp
        {
            get { return backUp; }
            set { backUp = value;OnPropertyChanged(nameof(IsEditing)); }
        }


        #region Properties

        #region Native Properties
        private string name = "";
        /// <summary>
        /// Name of the Item
        /// Setter Updates all the other Properties according to ItemMap Info
        /// </summary>
        public string Name
        {
            get { return name; }
            set {
                if (String.IsNullOrWhiteSpace(value)) return;

                if (!ItemMap.initialized && !ItemMap.IsValidItemName(value))
                {
                    Task.Factory.StartNew(() => ItemMap.getTypeMap());
                    // if MapNotLoaded and NameUnknown=> wait till i can be sure about it
                    while (!ItemMap.initialized) System.Threading.Thread.Yield(); // may Block the gui
                }

                if (ItemMap.IsValidItemName(value))
                {
                    name = value;
                    ModRank = Math.Min(0, ItemMap.GetMaxModRank(value));
                }
                OnPropertyChanged(nameof(Category));
                OnPropertyChanged(nameof(MaxRank));
                OnPropertyChanged(nameof(ModRank));
                OnPropertyChanged(nameof(ModRanks));
            }
        }

        private int price;
        public int Price {
            get { return price; }
            set { price = value; OnPropertyChanged(nameof(Price)); } }

        private int count;
        public int Count
        {
            get { return count; }
            set { count = value;OnPropertyChanged(nameof(Count)); }
        }

        private int modRank = -1;
        public int ModRank
        {
            get { return modRank; }
            set { modRank = value; OnPropertyChanged(nameof(ModRank)); }
        }

        public bool IsSellOffer { get; set; }

        /// <summary>
        /// Offer Id from Website
        /// Null or Empty if Item only local ( not yet synced )
        /// </summary>
        public string Id { get; set; }
        #endregion

        #region Derivated Properties
        public int MaxRank { get { return (Name==null || !ItemMap.IsValidItemName(name)) ? -1 : ItemMap.GetMaxModRank(name); } }

        public IEnumerable<int> ModRanks { get {
                return Enumerable.Range(0, MaxRank+1);
            }
        }

        public string Category { get
            {
                return ItemMap.GetCategory(Name);
            }
        }
        
        public List<string> AllItemNames { get { return ItemMap.GetValidItemNames(); } }

        public bool IsEditing { get { return BackUp != null; } }

        public bool HasChanged { get { return IsEditing && !Equals(BackUp); } }


        private bool isUpdating;

        public bool IsUpdating
        {
            get { return isUpdating; }
            set { isUpdating = value;OnPropertyChanged(nameof(IsUpdating)); }
        }


        #endregion

        #region IDataErrorInfo

        /// <summary>
        /// List of all Properties that currently have validation errors
        /// </summary>
        public ObservableCollection<string> ErrorProperties { get; } = new ObservableCollection<string>();


        public string Error
        {
            get
            {
                if (Name == "" || Count <= 0 || Price <= 0) return "Single DATA ERROR";
                return null;
            }
        }

        public string this[string columnName]
        {
            get
            {
                string error = "";

                // apply property level validation rules
                if (columnName == "Name")
                    if (Name==null || !ItemMap.IsValidItemName(Name))
                        error = "Wrong Name";
                
                if (columnName == "Count")
                    if (Count<=0)
                        error = "Wrong Count";

                if (columnName == "Price")
                    if (Price <= 0)
                        error = "Wrong Price";

                ErrorProperties.Remove(columnName); // Keep only once
                if (error != "")
                    ErrorProperties.Add(columnName);

                return error;
            }
        }

        #endregion

        
        #endregion


        #region Constructors

        /// <summary>
        /// Needed for DataGrid AllowUserAddItem
        /// </summary>
        public WarframeItem()
        {
            Name = "";
            ErrorProperties.CollectionChanged += (_, __) => { OnPropertyChanged(nameof(ErrorProperties)); };
        }

        public WarframeItem(string name,int price,int count,bool sellOffer):this()
        {
            this.Name = name;
            this.Price = price;
            this.Count = count;
            this.IsSellOffer = sellOffer;
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

        public WarframeItem(WarframeItem item):this(item.Name,item.Price,item.Count,item.ModRank,item.IsSellOffer,item.Id)
        {

        }

        #endregion


        #region Methods

        #region IEditableObject

        public void BeginEdit()
        {
            if (BackUp != null) return;
            BackUp = new WarframeItem(Name, Price, Count, ModRank, IsSellOffer, Id);
            Console.WriteLine("Begin Edit");
        }

        public void EndEdit()
        {
            if (!HasChanged)
            {
                BackUp = null;
            }
            Console.WriteLine("Commit Edit");
        }

        public void CancelEdit()
        {
            if (BackUp == null) return;
            lock (BackUp)
            {
                if (BackUp == null) return;
                Name = BackUp.Name;
                Price = BackUp.Price;
                Count = BackUp.Count;
                ModRank = BackUp.ModRank;
                BackUp = null;
            }
            Console.WriteLine("Cancel Edit");

        }

        #endregion

        /// <summary>
        /// Sends Data to the Server
        /// </summary>
        public async void CommitAdd()
        {
            MetroWindow window = (MetroWindow)Application.Current.MainWindow;
            MessageDialogResult result = await window.ShowMessageAsync("Confirm offer placement", "Do you want to place the " + (IsSellOffer ? "SELL" : "BUY") + $" offer for \n - {Name}" + (ModRank >= 0 ? " (Rank "+ModRank+")" : "") + $"\n - {Count} times for\n - {Price} Platinum to the Market ? ", MessageDialogStyle.AffirmativeAndNegative);

            if (result == MessageDialogResult.Affirmative)
            {
               
                IsUpdating = true;
                Task.Factory.StartNew(() =>
                {
                    ApplicationState.getInstance().Market.AddItem(this);
                    ApplicationState.getInstance().Market.UpdateListing();
                    IsUpdating = false;
                    BackUp = null;
                });
                Console.WriteLine("Added!");
            }
        }

        /// <summary>
        /// Sends Changes to the server
        /// </summary>
        public async void CommitEdit()
        {
            MetroWindow window = (MetroWindow)Application.Current.MainWindow;
            MessageDialogResult result = await window.ShowMessageAsync("Confirm offer change", "Do you want to place the " + (IsSellOffer ? "SELL" : "BUY") + $" offer for \n - {Name}" + (ModRank >= 0 ? " (Rank " + ModRank + ")" : "") + $"\n - {Count} times for\n - {Price} Platinum to the Market ? ", MessageDialogStyle.AffirmativeAndNegative);

            if (result == MessageDialogResult.Affirmative)
            {
                IsUpdating = true;
                if (BackUp.Name != Name)
                {
                    BackUp.RemoveItem();
                    Id = "";
                    
                    Task.Factory.StartNew(() =>
                    {
                        ApplicationState.getInstance().Market.AddItem(this);
                        ApplicationState.getInstance().Market.UpdateListing();
                        IsUpdating = false;
                        BackUp = null;

                    });
                }
                else
                {

                    
                    Task.Factory.StartNew(() =>
                    {
                        ApplicationState.getInstance().Market.EditItem(this);
                        ApplicationState.getInstance().Market.UpdateListing();
                        IsUpdating = false;
                        BackUp = null;
                    });
                    Console.WriteLine("Changes Committed!");
                }
                
            }
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
                if (IsSellOffer) ApplicationState.getInstance().SellItems.Remove(this);
                else ApplicationState.getInstance().BuyItems.Remove(this);
            }
        }

        public void RemoveItem()
        {
            if (HasChanged)
            {
                CancelEdit();
                return;
            }
            if(!String.IsNullOrWhiteSpace(Id)) Task.Factory.StartNew(()=> ApplicationState.getInstance().Market.RemoveItem(this));
            if (IsSellOffer) ApplicationState.getInstance().SellItems.Remove(this);
            else ApplicationState.getInstance().BuyItems.Remove(this);
        }





        public bool Equals(WarframeItem item)
        {
            return (item.Id ?? "") == (Id ?? "") && (item.Name ?? "") == (Name ?? "") && item.Count == Count && item.Price == Price && ModRank==item.ModRank && item.IsSellOffer == IsSellOffer;
        }

        public override string ToString()
        {
            return $"Itemname: {Name} Price: {Price} ID: {Id} Count: {Count} Modrank:{ModRank} IsSellOffer: {IsSellOffer} ";
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
