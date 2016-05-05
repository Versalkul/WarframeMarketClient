using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using WarframeMarketClient.Logic;
using WarframeMarketClient.ViewModel;

namespace WarframeMarketClient.GUI.Tabs
{
    /// <summary>
    /// Interaktionslogik für Tab_Chats.xaml
    /// </summary>
    public partial class Tab_Chats : UserControl
    {

        public ObservableCollection<ChatTabContentViewModel> chats;

        public ObservableCollection<ChatTabContentViewModel> Chats
        {
            get { return chats; }
            set { chats = value; }
        }


        public Tab_Chats()
        {
            InitializeComponent();

            this.DataContext = this;

            Chats = new ObservableCollection<ChatTabContentViewModel>();
            Chats.Add(new ChatNewViewModel());
            Chats.Add(new ChatViewModel(new Model.User() { Name = "Xarlas", State = Model.OnlineState.OFFLINE }));
        }

    }
}
