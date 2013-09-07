using System;
using System.Collections;
using System.Collections.Generic;
using System.Device.Location;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
using Microsoft.Phone.Controls;


namespace Perks
{
    public partial class MainPage : PhoneApplicationPage
    {

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            btnDirections.Tap += BtnDirectionsOnTap;

        }


        private void BtnDirectionsOnTap(object sender, GestureEventArgs gestureEventArgs)
        {

        }


        private void appBarBtnRefresh_Click(object sender, System.EventArgs e)
        {

        }

        private void appBarMenuItemAbout_Click(object sender, System.EventArgs e)
        {


        }

        private void appBarBtnInfo_Click(object sender, System.EventArgs e)
        {

        }



    }
}