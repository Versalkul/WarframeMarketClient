using System;
using System.Windows;
using MahApps.Metro.Controls;
using WarframeMarketClient.Model;
using Hardcodet.Wpf.TaskbarNotification;
using System.Linq;
using System.Windows.Media;
using System.IO;
using WarframeMarketClient.Logic;

namespace WarframeMarketClient.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {

        string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WarframeMarketClient");

        public MainWindow()
        {
            this.DataContext = ApplicationState.getInstance();
            InitializeComponent();
            TimeSpan uptime = TimeSpan.FromMilliseconds(Environment.TickCount & Int32.MaxValue);
            if (uptime.Minutes < 5)
            {
                if(ApplicationState.getInstance().Settings.Autostart&& ApplicationState.getInstance().Settings.ToTray)
                {
                    Hide();
                    
                }
                else
                {
                    WindowState = WindowState.Minimized;
                }
            }
            (new SaveLoadFile()).ExtractStandartSounds();

            ApplicationState.getInstance().NewMessage += MainWindow_NewMessage;

            // TestCall
            ApplicationState.getInstance().asynchRun(() =>
            {
                System.Threading.Thread.Sleep(10000);
                Application.Current.Dispatcher.InvokeAsync(()=>
                MainWindow_NewMessage(null, new System.Collections.Generic.List<ChatMessage>() { new ChatMessage() { MessageFrom = "TestUser", Message = "Hello i just want to say Test" } }));
            });

        }

        private void MainWindow_NewMessage(object sender, System.Collections.Generic.List<ChatMessage> e)
        {


            Console.WriteLine($"FocusState={IsKeyboardFocusWithin} and ChatFocus is {TabChats.IsKeyboardFocusWithin}");
            Console.WriteLine($"Selected Tabname {((System.Windows.Controls.TabItem)TabChats.chatTabs.SelectedItem).Name} and it is Focused {((System.Windows.Controls.TabItem)TabChats.chatTabs.SelectedItem).IsKeyboardFocused}"); // imo this line should do something else -.-' Tabname very funny c# -.-'

            if(!(IsKeyboardFocusWithin && TabChats.IsKeyboardFocusWithin)) // and selected chat is the user im chatting with (dont know how to find activeChattab)
            {

                System.Windows.Media.MediaPlayer player = new System.Windows.Media.MediaPlayer();
                player.Open(new Uri(Path.Combine(folderPath, ApplicationState.getInstance().Settings.ChoosenSoundFile)));
                player.Position = new TimeSpan(0);
                player.Play();

                string title = "You got new Messages";
                string text = String.Join("\n", e.Select(x=>($"{x.MessageFrom}:{x.Message}")));

                TrayIcon.ShowBalloonTip(title, text, BalloonIcon.Info);
            }

        }

        private void MainButtonClick(object sender, RoutedEventArgs args)
        {
            ApplicationState.getInstance().OnlineChecker.changeRunning();
        }


        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized && this.IsMouseOver && ApplicationState.getInstance().Settings.ToTray)
            {
                Hide();
            }
        }

        private void onTrayClick(object o, RoutedEventArgs args)
        {
            Show();
            Activate();
            WindowState = WindowState.Normal;
        }
    }
}
