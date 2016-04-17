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

namespace WarframeMarketClient.GUI.Tabs
{
    /// <summary>
    /// Interaktionslogik für ChatMsg.xaml
    /// </summary>
    public partial class Chat_Msg : UserControl
    {
        public Chat_Msg(String time, String msg)
        {
            InitializeComponent();
            Time.Text = time;
            Message.Text = msg;
        }
    }
}
