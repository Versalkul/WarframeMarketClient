using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using WarframeMarketClient.ViewModel;

namespace WarframeMarketClient.GUI.Tabs
{
    /// <summary>
    /// Interaktionslogik für Tab_Chats.xaml
    /// </summary>
    public partial class Tab_Chats : UserControl, INotifyPropertyChanged
    {

        private ObservableCollection<ChatViewModel> chats;

        public ObservableCollection<ChatViewModel> Chats
        {
            get { return chats; }
            set {
                chats = value;
                chats.CollectionChanged += chatUpdated;
                OnPropertyChanged("Tabs");
            }
        }

        public ReadOnlyObservableCollection<ChatTabContentViewModel> Tabs
        {
            get {
                ObservableCollection<ChatTabContentViewModel> tmp = new ObservableCollection<ChatTabContentViewModel>(chats);
                tmp.Insert(0, newChat);
                return new ReadOnlyObservableCollection<ChatTabContentViewModel>(tmp);
            }
        }



        protected ChatNewViewModel newChat = new ChatNewViewModel();


        public Tab_Chats()
        {
            InitializeComponent();

            this.DataContext = this;

            Chats = new ObservableCollection<ChatViewModel>();
            Chats.Add(new ChatViewModel(new Model.User() { Name = "Xarlas", State = Model.OnlineState.OFFLINE }));
        }


        private void chatUpdated(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("Tabs");
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
