using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WarframeMarketClient.Extensions
{
    static class KeyExtensions
    {
        private static List<Key> printableLongerThan1 = new List<Key>() { Key.Back,Key.Delete,Key.Divide,Key.Enter,Key.Multiply,Key.Space,Key.Add,Key.Subtract,Key.OemComma,Key.OemMinus,Key.OemPlus,Key.DeadCharProcessed,Key.OemPeriod,Key.OemOpenBrackets,Key.OemCloseBrackets,Key.OemQuestion,Key.OemQuotes,Key.OemSemicolon,Key.OemTilde,Key.OemBackslash,Key.Decimal};
        public static bool IsPrintable(this Key key)
        {
            
            string keychar = key.ToString().Replace("D","").Replace("NumPad", "");
            return printableLongerThan1.Contains(key) || keychar.Length <= 1;
        }
    }
}
