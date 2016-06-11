using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
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
                ChatNewViewModel chat = (ChatNewViewModel) (this.DataContext);
                Task.Factory.StartNew(()=>chat.openChat());
            }
        }
    }
}
