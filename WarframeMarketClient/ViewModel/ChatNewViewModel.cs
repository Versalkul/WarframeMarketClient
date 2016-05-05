using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarframeMarketClient.ViewModel
{
    public class ChatNewViewModel : ChatTabContentViewModel, INotifyPropertyChanged
    {

        public override string DisplayName { get { return "+";} }

        public string Status { get; set; }

        private string user;

        public string User
        {
            get { return user; }
            set { user = value; OnPropertyChanged("User"); }
        }

        public void openChat()
        {
            Console.WriteLine("Open Chat with: "+User);
        }





        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string property)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(property));
        }
    }
}
