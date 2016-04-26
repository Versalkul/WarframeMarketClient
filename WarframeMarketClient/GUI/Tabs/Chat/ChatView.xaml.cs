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

namespace WarframeMarketClient.GUI.Tabs.Chat
{
    /// <summary>
    /// Interaktionslogik für Chat.xaml
    /// </summary>
    public partial class ChatView : UserControl
    {
        public User user { get; private set; }

        public ObservableCollection<ChatMessage> ChatMessages { get; set; }

        public ChatView(User us)
        {
            user = us;
            InitializeComponent();
            this.DataContext = this;

            ChatMessages = new ObservableCollection<ChatMessage>()
            {
                new ChatMessage() { Message = "Test", SendHour="20", SendMinute="15", MessageFrom="A" },
                new ChatMessage() { Message = "Passt", SendHour="20", SendMinute="16", MessageFrom="B" },
                new ChatMessage() { Message = "Nice!", SendHour="20", SendMinute="17", MessageFrom="A" },
                new ChatMessage() { Message = "Test", SendHour="20", SendMinute="15", MessageFrom="A" },
                new ChatMessage() { Message = "Passt", SendHour="20", SendMinute="16", MessageFrom="B" },
                new ChatMessage() { Message = "Nice!", SendHour="20", SendMinute="17", MessageFrom="A" },
                new ChatMessage() { Message = "Test", SendHour="20", SendMinute="15", MessageFrom="A" },
                new ChatMessage() { Message = "Passt", SendHour="20", SendMinute="16", MessageFrom="B" },
                new ChatMessage() { Message = "Nice!", SendHour="20", SendMinute="17", MessageFrom="A" },
                new ChatMessage() { Message = "Test", SendHour="20", SendMinute="15", MessageFrom="A" },
                new ChatMessage() { Message = "Passt", SendHour="20", SendMinute="16", MessageFrom="B" },
                new ChatMessage() { Message = "Nice!", SendHour="20", SendMinute="17", MessageFrom="A" },
            };
        }


        private void Input_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return){
                if((e.KeyboardDevice.Modifiers & (ModifierKeys.Shift | ModifierKeys.Control)) != 0) {
                    int caretIndex = InputText.CaretIndex;
                    InputText.Text = InputText.Text.Insert(InputText.CaretIndex, "\r\n");
                    InputText.CaretIndex = caretIndex+1;
                }
            }
        }


        private void closeChat(object sender, MouseEventArgs e)
        {

        }

        
    }
}
