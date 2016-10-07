using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WarframeMarketClient.ViewModel;

namespace WarframeMarketClient.GUI.Tabs.Chat
{
    /// <summary>
    /// Interaktionslogik für Chat.xaml
    /// </summary>
    public partial class ChatView : UserControl
    {
        private ScrollViewer chatScroll;

        public ChatView()
        {
            InitializeComponent();
            ((INotifyCollectionChanged)ChatList.Items).CollectionChanged += OnChatChanged;
            DataContextChanged += new DependencyPropertyChangedEventHandler(OnDataContextChanged);
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(DataContext != null)
                (DataContext as ChatTabContentViewModel).Focus = FocusInput;
        }
        private void FocusInput()
        {
            InputText.Focus();
        }

        private void OnChatChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Scroll to the bottom on new message (if already was at bottom)
            if (chatScroll == null)
            {
                if (VisualTreeHelper.GetChildrenCount(ChatList) > 0)
                {
                    Border border = (Border)VisualTreeHelper.GetChild(ChatList, 0);
                    chatScroll = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
                }
            }
            if(chatScroll != null && chatScroll.VerticalOffset > chatScroll.ScrollableHeight - 1)
                chatScroll.ScrollToBottom();
        }


        private void Input_KeyDown(object sender, KeyEventArgs e)
        {
            // Behaviour: Enter sends message; Shift-Enter or Ctrl-Enter inserts Newline
            if (e.Key == Key.Return){
                if((e.KeyboardDevice.Modifiers & (ModifierKeys.Shift | ModifierKeys.Control)) != 0) {
                    int caretIndex = InputText.CaretIndex;
                    InputText.Text = InputText.Text.Insert(InputText.CaretIndex, "\r\n");
                    InputText.CaretIndex = caretIndex+1;
                }
                else{
                    ((ChatViewModel) this.DataContext).SendMessage();
                }
            }
        }


        private void closeChat(object sender, RoutedEventArgs e)
        {
            ((ChatViewModel)this.DataContext).CloseChat();
        }
    }
}
