using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WarframeMarketClient.GUI.Tabs
{
    /// <summary>
    /// Interaktionslogik für Tab_Items.xaml
    /// </summary>
    public partial class Tab_Items : UserControl
    {
        public Tab_Items()
        {
            InitializeComponent();

            List<Item> items = new List<Item>();
            items.Add(new Item() { Category = "Bla", Name = "Blablub", Price=20, Count=2, Rank=0, Buttons="Nix" });
            items.Add(new Item() { Category = "Bla", Name = "Blablub", Price = 20, Count = 2, Rank = 0, Buttons = "Nix" });
            items.Add(new Item() { Category = "Bla", Name = "Blablub", Price = 20, Count = 2, Rank = 0, Buttons = "Nix" });
            //list.ItemsSource = items;
        }
    }

    public class Item
    {
        public string Category { get; set; }

        public string Name { get; set; }

        public int Price { get; set; }

        public int Count { get; set; }

        public int Rank { get; set; }

        public string Buttons { get; set; }
    }

}
