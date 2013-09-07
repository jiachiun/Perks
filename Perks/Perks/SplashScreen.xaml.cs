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
using Microsoft.Phone.Shell;
using Newtonsoft.Json.Linq;
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
            if (!Helper.isNetworkAvailable())
            {
                MessageBox.Show("Unable to connect to the internet. Please check your data connection and try again.", "No Data Connection", MessageBoxButton.OK);

                return;
            }

            StoryboardLoading.Begin();

            // show loading bar
            loadingBar.IsIndeterminate = true;
            loadingBar.Visibility = Visibility.Visible;

            try
            {
                txtMessage.Text = "Locating...";
                await getCoordinates();

                txtMessage.Text = "Grabbing goods...";
                await getVenues();

                if (Helper.venues.Count == 0)
                {
                    MessageBox.Show("Sorry, no perks nearby. Please try again elsewhere :'(");

                    resetPage();
                    //txtMessage.Text = "Sorry, no perks nearby :'(";

                    //// hide loading bar
                    //loadingBar.Visibility = Visibility.Collapsed;
                    //loadingBar.IsIndeterminate = false;
                }
                else
                {
                    txtMessage.Text = "Done!";

                    NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
                }
            }
            catch (Exception ex)
            {
                if (ex.Message == "NoLocationException")
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
            // if user has not opted in or out of location
            //if (!IsolatedStorageSettings.ApplicationSettings.Contains("LocationConsent"))
            //{
            //    MessageBoxResult result =
            //        MessageBox.Show("This app accesses your phone's location. Is that ok?",
            //        "Location",
            //        MessageBoxButton.OKCancel);

            //    if (result == MessageBoxResult.OK)
            //    {
            //        IsolatedStorageSettings.ApplicationSettings["LocationConsent"] = true;
            //    }
            //    else
            //    {
            //        IsolatedStorageSettings.ApplicationSettings["LocationConsent"] = false;
            //    }

            //    IsolatedStorageSettings.ApplicationSettings.Save();
            //}

            if (Helper.isRefreshing)
            {
                Helper.isRefreshing = false;
                explore();

            }
            else
            {
                resetPage();
            }


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
            //loadingBar.Visibility = Visibility.Visible;

            //if ((bool)IsolatedStorageSettings.ApplicationSettings["LocationConsent"] != true)
            //{
            //    // The user has opted out of Location.
            //    return;
            //}

            Geolocator geolocator = new Geolocator();
            geolocator.DesiredAccuracyInMeters = 50;

            try
            {
                Geoposition geoposition = await geolocator.GetGeopositionAsync(TimeSpan.FromMilliseconds(5), TimeSpan.FromSeconds(10));

                Helper.latitude = geoposition.Coordinate.Latitude.ToString("0.00");
                Helper.longitude = geoposition.Coordinate.Longitude.ToString("0.00");

                //string location = "Longitude: " + Helper.longitude + ", Latitude: " + Helper.latitude;
                //MessageBox.Show(location);
            }
            catch (Exception ex)
            {

                if ((uint)ex.HResult == 0x80004004)
                {
                    throw new Exception("NoLocationException");
                    // the application does not have the right capability or the location master switch is off
                    //MessageBox.Show("This app requires your location to search for perks nearby. Please enable location in phone setting.", "Location Service Required", MessageBoxButton.OKCancel);
                }
                //else
                //{
                //    // something else happened acquring the location
                //}
            }
        }


        private async Task getVenues()
        {

            WebClient webClient = new WebClient();

            webClient.DownloadStringCompleted += webClient_DownloadStringCompleted;


            string url = "https://api.foursquare.com/v2/";
            url += String.Format("specials/search?ll={0},{1}", Helper.latitude, Helper.longitude); // Specials Explore
            url += "&oauth_token=BZNDEQTA5GYGZHLMDRKO43IQZEW4THCFCNKYB2PHBIC2ALEZ&v=20130903";  // OAuth Token

            //webClient.DownloadStringAsync(new Uri(url));
            await Extensions.DownloadStringTask(webClient, new Uri(url, UriKind.Absolute));

        }

        public static bool isEven(int value)
        {
            return value % 2 == 0;
        }

        async void webClient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            Helper.venues = new List<Venue>();

            var root = JObject.Parse(e.Result);
            var items = root["response"]["specials"]["items"];

            //MessageBox.Show(String.Format("name: {0}", firstGroup["venue"]["name"]));

            // clear list
            Helper.venues.Clear();

            int i = 0;

            Random random = new Random();
            int adIndex = random.Next(0, items.Count()-1 );

            foreach (var item in items)
            {
                if(i == adIndex)
                {
                    // randomly add an ads for demo purposes
                    Venue venueAds = new Venue();

                    venueAds.index = i;

                    venueAds.name = "[Ads] Ferrari";
                    venueAds.description = "Only those who dare... truly live";
                    venueAds.imageVenue = "Images/ads-ferrari.jpg"

                    venueAds.imageType = "ads";

                    Helper.venues.Add(venueAds);

                    continue;
                }
               

                Venue venue = new Venue();

                venue.index = i;
                venue.id = item["venue"]["id"].ToString();
                venue.name = item["venue"]["name"].ToString();
                venue.canonicalUrl = item["venue"]["canonicalUrl"].ToString();

                venue.imageType = item["type"].ToString();

                venue.imageVenue = "";

                // location
                venue.address = item["venue"]["location"]["address"].ToString();
                venue.longitude = item["venue"]["location"]["lng"].ToString();
                venue.latitude = item["venue"]["location"]["lat"].ToString();

                var sCoord = new GeoCoordinate(Double.Parse(Helper.latitude), Double.Parse(Helper.longitude));
                var eCoord = new GeoCoordinate(Double.Parse(venue.latitude), Double.Parse(venue.longitude));
                venue.distance = Math.Round(sCoord.GetDistanceTo(eCoord) * 0.000621371, 2).ToString();   // convert meters to miles

                // specials
                venue.message = item["message"].ToString();
                venue.description = item["description"].ToString();

                if (item["finePrint"] != null)
                    venue.fineprint = item["finePrint"].ToString();

                // stats
                venue.checkinsCount = item["venue"]["stats"]["checkinsCount"].ToString();
                venue.usersCount = item["venue"]["stats"]["usersCount"].ToString();


                Helper.venues.Add(venue);
            }


            // stop loading bar
            loadingBar.Visibility = Visibility.Collapsed;
            loadingBar.IsIndeterminate = false;
        }

        public double getDistance(GeoCoordinate p1, GeoCoordinate p2)
        {
            double d = p1.Latitude * 0.017453292519943295;
            double num3 = p1.Longitude * 0.017453292519943295;
            double num4 = p2.Latitude * 0.017453292519943295;
            double num5 = p2.Longitude * 0.017453292519943295;
            double num6 = num5 - num3;
            double num7 = num4 - d;
            double num8 = Math.Pow(Math.Sin(num7 / 2.0), 2.0) + ((Math.Cos(d) * Math.Cos(num4)) * Math.Pow(Math.Sin(num6 / 2.0), 2.0));
            double num9 = 2.0 * Math.Atan2(Math.Sqrt(num8), Math.Sqrt(1.0 - num8));
            return (6376500.0 * num9);
        }
    }
}