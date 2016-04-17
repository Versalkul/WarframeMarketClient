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
    /// Interaktionslogik für Chat.xaml
    /// </summary>
    public partial class Chat : UserControl
    {
        public Chat()
        {
            InitializeComponent();
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
    }
}
