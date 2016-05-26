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

        private string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WarframeMarketClient");

        private MediaPlayer PlimPlayer = new MediaPlayer();


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
                System.Threading.Thread.Sleep(5000);
                Application.Current.Dispatcher.InvokeAsync(()=>
                MainWindow_NewMessage(null, new System.Collections.Generic.List<ChatMessage>() { new ChatMessage() { MessageFrom = "AnywayTheWindbro", Message = "Hello i just want to say Test" } }));
                System.Threading.Thread.Sleep(1000);
                Application.Current.Dispatcher.InvokeAsync(() =>
                MainWindow_NewMessage(null, new System.Collections.Generic.List<ChatMessage>() { new ChatMessage() { MessageFrom = "AnywayTheWindbro", Message = "Hello i just want to say Test" }, new ChatMessage() { MessageFrom = "TestUser2", Message = "Hello i just want to say Test" } }));
                System.Threading.Thread.Sleep(1000);
                Application.Current.Dispatcher.InvokeAsync(() =>
                MainWindow_NewMessage(null, new System.Collections.Generic.List<ChatMessage>() { new ChatMessage() { MessageFrom = "TestUser2", Message = "Hello i just want to say Test" } }));
            });


            PlimPlayer.Open(new Uri(Path.Combine(folderPath, ApplicationState.getInstance().Settings.ChoosenSoundFile)));
        }

        private void MainWindow_NewMessage(object sender, System.Collections.Generic.List<ChatMessage> e)
        {
            //TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Indeterminate;
            //TaskbarItemInfo.ProgressValue = 0.5;
            //TaskbarItemInfo.
            //this.focus
            // Make the window blink... WinAPI? :(

            if (!(IsKeyboardFocusWithin && TabChats.IsVisible && !e.Where(m => (TabChats.SelectedChat?.User?.Name != m.MessageFrom)).Any()))
            {
                Console.WriteLine("Plim");
                PlimPlayer.Position = new TimeSpan(0);
                PlimPlayer.Play();
            }
            if (!IsKeyboardFocusWithin)
            {
                string title = "You've got new Messages";
                string text = String.Join("\n", e.Select(x => ($"{x.MessageFrom} : {x.Message}")));

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
                Hide();
        }

        private void onTrayClick(object o, RoutedEventArgs args)
        {
            Show();
            Activate();
            WindowState = WindowState.Normal;
        }
    }
}
