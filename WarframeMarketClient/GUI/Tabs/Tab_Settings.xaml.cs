﻿using Hardcodet.Wpf.TaskbarNotification;
using System.Windows.Controls;
using System.Drawing;
using System;
using System.Windows;
using WarframeMarketClient.Model;
using System.Windows.Input;
using WarframeMarketClient.Logic;

namespace WarframeMarketClient.GUI.Tabs
{
    /// <summary>
    /// Interaktionslogik für Tab_Settings.xaml
    /// </summary>
    public partial class Tab_Settings : UserControl
    {



        public string SessionTokenInput { get; set; }
        private bool autostart;

        public bool Autostart
        {
            get { return autostart; }
            set { autostart = value; Console.WriteLine("Set Autostart"); }
        }

        public bool ToTray { get; set; }



        public Tab_Settings()
        {
            InitializeComponent();
            this.DataContext = this;
          
        }

        private void save()
        {
            ApplicationState.getInstance().SessionToken = SessionTokenInput;
            Tuple<bool,string> verification = HtmlParser.verifyToken();
            if (verification.Item1)
            {
                Console.WriteLine("Valid Token");
            }

        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            save();
        }

        private void tokenBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                save();

            }
        }


    }
}
