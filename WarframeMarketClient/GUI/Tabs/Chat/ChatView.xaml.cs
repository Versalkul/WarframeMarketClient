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
using WarframeMarketClient.Model;
using WarframeMarketClient.ViewModel;

namespace WarframeMarketClient.GUI.Tabs.Chat
{
    /// <summary>
    /// Interaktionslogik für Chat.xaml
    /// </summary>
    public partial class ChatView : UserControl
    {

        public ChatView()
        {
            InitializeComponent();
            //ChatList.ScrollIntoView(ChatList.Items[ChatList.Items.Count - 1]);
        }


        private void Input_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return){
                if((e.KeyboardDevice.Modifiers & (ModifierKeys.Shift | ModifierKeys.Control)) != 0) {
                    int caretIndex = InputText.CaretIndex;
                    InputText.Text = InputText.Text.Insert(InputText.CaretIndex, "\r\n");
                    InputText.CaretIndex = caretIndex+1;
                }else
                {
                    ((ChatViewModel) this.DataContext).sendMessage();
                }
            }
        }


        private void closeChat(object sender, MouseEventArgs e)
        {
            ((ChatViewModel)this.DataContext).closeChat();
        }
    }
}
