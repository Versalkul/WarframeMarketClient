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
using WarframeMarketClient.Logic;

namespace WarframeMarketClient.GUI.Tabs
{
    /// <summary>
    /// Interaktionslogik für Tab_Chats.xaml
    /// </summary>
    public partial class Tab_Chats : UserControl
    {
        public Tab_Chats()
        {
            InitializeComponent();
            addChat(new User() {Name="Blablub"});
        }


        void addChat(User user)
        {
            Chat chat = new Chat(user);

            TabItem tab = new TabItem();
            tab.Header = user.Name;
            tab.Content = chat;

            chatTabs.Items.Add(tab);
        }

        void closeChat(Chat chat)
        {
            chatTabs.Items.Remove(chat.Parent);
        }
    }
}
