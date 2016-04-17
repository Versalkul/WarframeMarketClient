using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarframeMarketOnlineController
{
    class WarframeItem
    {

        public string name;
        public int price;
        public int count;
        public int modRank=-1;
        public bool sellOffer;
        public string id="";

        public WarframeItem(string name,int price,int count,bool sellOffer)
        {
            this.name = name;
            this.price = price;
            this.count = count;
            this.sellOffer = sellOffer;
        }

        public WarframeItem(string name, int price, int count,int modRank, bool sellOffer):this(name, price, count, sellOffer)
        {
            this.modRank = modRank;
        }

        public WarframeItem(string name, int price, int count, bool sellOffer,string id):this(name, price, count, sellOffer)
        {

            this.id = id;
        }

        public WarframeItem(string name, int price, int count, int modRank, bool sellOffer,string id) : this(name, price, count, sellOffer, id)
        {
            this.modRank = modRank;
        }

    }
}
