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
using WarframeMarketClient.ViewModel;

namespace WarframeMarketClient.GUI.Tabs.Chat
{
    /// <summary>
    /// Interaktionslogik für Chat_New.xaml
    /// </summary>
    public partial class ChatNewView : UserControl
    {
        public ChatNewView()
        {
            InitializeComponent();
        }

        private void Input_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                ((ChatNewViewModel)this.DataContext).openChat();
            }
        }
    }
}
