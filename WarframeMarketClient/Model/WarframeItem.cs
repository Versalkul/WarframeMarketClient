﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls.Dialogs;
using MahApps.Metro.Controls;

namespace WarframeMarketClient.Model
{
    public class WarframeItem : INotifyPropertyChanged, IDataErrorInfo, IEditableObject,IEquatable<WarframeItem>
    {
        public static Dictionary<string, Tuple<string, int>> itemInfoMap = new Dictionary<string, Tuple<string, int>>(1000);

        private WarframeItem backUp;

        #region Properties

        #region Native Properties
        private string name;

        public string Name
        {
            get { return name; }
            set {
                if (value == null) return;
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

        public int ModRank { get; set; } = -1;
        public bool SellOffer { get; set; }
        public string Id { get; set; }
        #endregion

        #region Derivated Properties
        public int MaxRank { get { return (Name==null||!itemInfoMap.ContainsKey(name))? -1: itemInfoMap[Name].Item2; } }

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

        public bool Editing { get { return backUp != null; } }

        public bool HasChanged { get; private set; }

        #region IDataErrorInfo


        public ObservableCollection<string> ErrorProperties { get; } = new ObservableCollection<string>();


        public string Error
        {
            get
            {
                Console.WriteLine(" Called Error");
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
                {
                    if (Name==null||!itemInfoMap.ContainsKey(Name))
                        error = "Wrong Name";
                }
                
                if (columnName == "Count")
                {
                    if (Count<=0)
                        error = "Wrong Count";
                }
                if (columnName == "Price")
                {
                    if (Price <= 0)
                        error = "Wrong Price";
                }

                ErrorProperties.Remove(columnName); // Keep only once
                if (error != "")
                    ErrorProperties.Add(columnName);

                return error;
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
            Name = "";
            ErrorProperties.CollectionChanged += (_, __) => { OnPropertyChanged(nameof(ErrorProperties)); };
        }

        public WarframeItem(string name,int price,int count,bool sellOffer):this()
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

        #region IEditableObject

        public void BeginEdit()
        {
            if (backUp != null) return;
            backUp = new WarframeItem(Name, Price, Count, ModRank, SellOffer, Id);
            Console.WriteLine("Begin Edit");
            OnPropertyChanged(nameof(Editing));
        }

        public void EndEdit()
        {
            HasChanged = true;
            // check if really edited
            Console.WriteLine("Commit Edit");
            OnPropertyChanged(nameof(Editing));
        }

        public void CancelEdit()
        {

            if (backUp == null) return;
            HasChanged = false;
            Name = backUp.Name;
            Price = backUp.Price;
            Count = backUp.Count;
            ModRank = backUp.ModRank;
            backUp = null;
            Console.WriteLine("Cancel Edit");
            OnPropertyChanged(nameof(Editing));
        }

        #endregion

        public async void CommitAdd()
        {
            MetroWindow window = (MetroWindow)Application.Current.MainWindow;
            if (Id != "") //editing an old item
            {
                MessageDialogResult result = await window.ShowMessageAsync("Confirm Edititem", "Do you want to edit the item to " + (SellOffer ? "Sell" : "Buy") + $" offer {Name} {Count}-times for {Price} Platinum on the Market ? ", MessageDialogStyle.AffirmativeAndNegative);

                if (result == MessageDialogResult.Affirmative)
                {
                    backUp = null;
                    ApplicationState.getInstance().asynchRun(() =>
                    {
                        ApplicationState.getInstance().Market.EditItem(this);
                        ApplicationState.getInstance().Market.UpdateListing();
                    });
                    Console.WriteLine("Added!");
                }
            }
            else // new Item 
            {

                MessageDialogResult result=  await window.ShowMessageAsync("Confirm Additem", "Do you want to add the " + (SellOffer ? "Sell" : "Buy") + $" offer {Name} {Count}-times for {Price} Platinum to the Market ? ", MessageDialogStyle.AffirmativeAndNegative);

                if (result == MessageDialogResult.Affirmative)
                {
                    backUp = null;
                    ApplicationState.getInstance().asynchRun(() =>
                    {
                        ApplicationState.getInstance().Market.AddItem(this);
                        ApplicationState.getInstance().Market.UpdateListing();
                    });
                    Console.WriteLine("Added!");
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
                if (SellOffer) ApplicationState.getInstance().SellItems.Remove(this);
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
            if(!String.IsNullOrWhiteSpace(Id))ApplicationState.getInstance().asynchRun(()=> ApplicationState.getInstance().Market.RemoveItem(this));
            if (SellOffer) ApplicationState.getInstance().SellItems.Remove(this);
            else ApplicationState.getInstance().BuyItems.Remove(this);
        }


        public bool Equals(WarframeItem item)
        {
            return item.Id == Id && item.Name == Name && item.Count == Count && item.Price == Price && item.SellOffer == SellOffer;
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
