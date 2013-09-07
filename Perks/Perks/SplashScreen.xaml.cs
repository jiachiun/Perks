using System;
using System.Collections.Generic;
using System.Device.Location;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;

using Windows.Devices.Geolocation;

namespace Perks
{
    public partial class SplashScreen : PhoneApplicationPage
    {

        public SplashScreen()
        {
            InitializeComponent();

            btnExplore.Tap += BtnExploreOnTap;
        }

        private void BtnExploreOnTap(object sender, GestureEventArgs gestureEventArgs)
        {
            explore();
        }

        private async void explore()
        {
            StoryboardLoading.Begin();

            // show loading bar
            loadingBar.IsIndeterminate = true;
            loadingBar.Visibility = Visibility.Visible;

            try
            {
                txtMessage.Text = "Locating...";
                await getCoordinates();

            
            }
            catch (Exception ex)
            {
                if (ex.Message=="NoLocationException")
                {
                    // the application does not have the right capability or the location master switch is off
                    MessageBox.Show("This app requires your location to search for perks nearby. Please enable location in phone setting.", "Location Service Required", MessageBoxButton.OKCancel);
                }
                else
                {
                    MessageBox.Show("Some error occurred. Please try again later.", "Error", MessageBoxButton.OK);
                }

                resetPage();

            }
        }

        

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {

        }

        private void resetPage()
        {
            StoryboardReset.Begin();

            // hide loading bar
            loadingBar.IsIndeterminate = false;
            loadingBar.Visibility = Visibility.Collapsed;
        }

        private async Task getCoordinates()
        {
            Geolocator geolocator = new Geolocator();
            geolocator.DesiredAccuracyInMeters = 50;

    
            Geoposition geoposition = await geolocator.GetGeopositionAsync(TimeSpan.FromMilliseconds(5), TimeSpan.FromSeconds(10));

               
            Helper.latitude = geoposition.Coordinate.Latitude.ToString("0.00");
            Helper.longitude = geoposition.Coordinate.Longitude.ToString("0.00");

            
        }


        
    }
}