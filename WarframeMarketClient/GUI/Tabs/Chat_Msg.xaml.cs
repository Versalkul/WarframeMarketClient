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
    /// Interaktionslogik für ChatMsg.xaml
    /// </summary>
    public partial class Chat_Msg : UserControl
    {
        public Chat_Msg(PmData PM)
        {
            InitializeComponent();
            Time.Text = PM.time;
            Message.Text = PM.message;

            if (PM.fromMe)
            {
                Time.HorizontalAlignment = Message.HorizontalAlignment = HorizontalAlignment.Right;
            }
        }
    }
}
