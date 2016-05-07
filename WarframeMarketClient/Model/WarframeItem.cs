using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarframeMarketClient.Model
{
    public class WarframeItem
    {

        public string Name { get; set; }
        public int Price { get; set; }
        public int Count { get; set; }
        public int ModRank { get; set; }
        public bool SellOffer { get; set; }
        public string Id { get; set; }

        public string Category { get
            {
                return ApplicationState.getInstance().Market.GetCategory(Name);
            }
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

    }
}
