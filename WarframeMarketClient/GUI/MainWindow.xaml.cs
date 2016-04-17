using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Hardcodet.Wpf.TaskbarNotification;

namespace WarframeMarketClient.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Border StatusPanel;
        private Label StatusLabel;
        private TaskbarIcon tbi = new TaskbarIcon();
        public  bool toTray = false;

        public MainWindow()
        {
            InitializeComponent();
            this.SourceInitialized += new EventHandler(Window1_SourceInitialized);
            tbi.TrayMouseDoubleClick += new RoutedEventHandler(onTrayClick);
        }

        void Window1_SourceInitialized(object sender, EventArgs e)
        {
            WindowSizing.WindowInitialized(this);
        }



        public void setStatus()
        {
            if (StatusPanel == null) return;
            BrushConverter bc = new BrushConverter();

            if (true){
                StatusPanel.Background = (Brush) bc.ConvertFrom("#afa");
                StatusLabel.Content = "InGame";
            }
        }

        private void LoadStatusPanel(object sender, RoutedEventArgs e)
        {
            StatusPanel = (Border)sender;
            StatusLabel = (Label)StatusPanel.FindName("StatusText");
        }


        private void PART_LOADED(object sender, RoutedEventArgs e)
        {
            FrameworkElement Wnd = (FrameworkElement) sender;
            new WindowResizer(this,
                new WindowBorder(BorderPosition.TopLeft, (Rectangle)Wnd.FindName("topLeft")),
                new WindowBorder(BorderPosition.Top, (Rectangle)Wnd.FindName("top")),
                new WindowBorder(BorderPosition.TopRight, (Rectangle)Wnd.FindName("topRight")),
                new WindowBorder(BorderPosition.Right, (Rectangle)Wnd.FindName("right")),
                new WindowBorder(BorderPosition.BottomRight, (Rectangle)Wnd.FindName("bottomRight")),
                new WindowBorder(BorderPosition.Bottom, (Rectangle)Wnd.FindName("bottom")),
                new WindowBorder(BorderPosition.BottomLeft, (Rectangle)Wnd.FindName("bottomLeft")),
                new WindowBorder(BorderPosition.Left, (Rectangle)Wnd.FindName("left")));
        }

        private void PART_CLOSE_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void PART_MINIMIZE_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized;
        }



        private void PART_TITLEBAR_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (ResizeMode != ResizeMode.CanResize &&
                    ResizeMode != ResizeMode.CanResizeWithGrip)
                {
                    return;
                }

                WindowState = WindowState == WindowState.Maximized
                    ? WindowState.Normal
                    : WindowState.Maximized;
            }
            else
            {
                mRestoreForDragMove = WindowState == WindowState.Maximized;
                DragMove();
            }
        }

        private void PART_TITLEBAR_MouseMove(object sender, MouseEventArgs e)
        {
            if (mRestoreForDragMove)
            {
                mRestoreForDragMove = false;

                var point = PointToScreen(e.MouseDevice.GetPosition(this));

                Left = point.X - (RestoreBounds.Width * 0.5);
                Top = point.Y;

                WindowState = WindowState.Normal;

                DragMove();
            }
        }

        private void PART_TITLEBAR_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            mRestoreForDragMove = false;
        }

        private bool mRestoreForDragMove;

        private void Window_StateChanged(object sender, EventArgs e)
        {

            //Rect rect = new Rect(Application.Current.MainWindow.Left, Application.Current.MainWindow.Top, Application.Current.MainWindow.ActualWidth, Application.Current.MainWindow.ActualHeight);

            if(window.WindowState == WindowState.Minimized&&this.IsMouseOver&&SharedValues.toTray)
            {
                Hide();
                tbi.Visibility = Visibility.Visible;
            }
            else
            {
                tbi.Visibility = Visibility.Hidden;
            }

        }

        private void onTrayClick(object o, RoutedEventArgs args)
        {
            Show();
            window.Activate();
            window.WindowState = WindowState.Normal;
        }





    }
}
