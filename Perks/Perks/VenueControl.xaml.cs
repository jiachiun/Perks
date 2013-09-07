using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Newtonsoft.Json.Linq;

namespace Perks
{
    public partial class VenueControl : UserControl
    {
        public int index;
        public string imageVenue = "";

        public VenueControl()
        {
            InitializeComponent();
        }

        public VenueControl(Venue venue)
        {
            InitializeComponent();

            this.index = venue.index;
            this.imageVenue = venue.imageVenue;

            txtName.Text = venue.name;
            txtMessage.Text = venue.message;

            if (venue.imageVenue != "")
                imgVenue.Source = new BitmapImage(new Uri(venue.imageVenue, UriKind.Absolute));

            switch (venue.imageType)
            {
                case "mayor":
                    imgType.Source = new BitmapImage(new Uri("Images/mayor.png", UriKind.Relative));
                    ellipseType.Fill = new SolidColorBrush(Helper.ConvertStringToColor("#FFE8C906"));
                    break;

                case "frequency":
                    imgType.Source = new BitmapImage(new Uri("Images/frequency.png", UriKind.Relative));
                    ellipseType.Fill = new SolidColorBrush(Helper.ConvertStringToColor("#FF8E00DF"));
                    break;

                case "check-in":
                    imgType.Source = new BitmapImage(new Uri("Images/check-in.png", UriKind.Relative));
                    ellipseType.Fill = new SolidColorBrush(Helper.ConvertStringToColor("#FF7DDF00"));
                    break;

                case "swarm":
                    imgType.Source = new BitmapImage(new Uri("Images/swarm.png", UriKind.Relative));
                    ellipseType.Fill = new SolidColorBrush(Helper.ConvertStringToColor("#FFE6D106"));
                    break;

                case "newbie":
                    imgType.Source = new BitmapImage(new Uri("Images/newbie.png", UriKind.Relative));
                    ellipseType.Fill = new SolidColorBrush(Helper.ConvertStringToColor("#FFB0B0B0"));
                    break;

                case "friends":
                    imgType.Source = new BitmapImage(new Uri("Images/friends.png", UriKind.Relative));
                    ellipseType.Fill = new SolidColorBrush(Helper.ConvertStringToColor("#FF00DFAC"));
                    break;

                case "flash":
                    imgType.Source = new BitmapImage(new Uri("Images/flash.png", UriKind.Relative));
                    ellipseType.Fill = new SolidColorBrush(Helper.ConvertStringToColor("#FFD14444"));
                    break;

                case "ads":
                    imgType.Source = new BitmapImage(new Uri("Images/flash.png", UriKind.Relative));
                    ellipseType.Fill = new SolidColorBrush(Helper.ConvertStringToColor("#FFD14444"));                    break;

                default:
                    imgType.Source = new BitmapImage(new Uri("Images/check-in.png", UriKind.Relative));
                    ellipseType.Fill = new SolidColorBrush(Helper.ConvertStringToColor("#FF7DDF00"));
                    break;

            }

            getImage(venue.id);
        }

        void getImage(string venueId)
        {

            // get the image of the venue
            WebClient webClientPhoto = new WebClient();
            string url = "https://api.foursquare.com/v2/";
            url += String.Format("venues/{0}/photos", venueId); // Get photo
            url += "?oauth_token=BZNDEQTA5GYGZHLMDRKO43IQZEW4THCFCNKYB2PHBIC2ALEZ&v=20130903";  // OAuth Token


            webClientPhoto.DownloadStringCompleted += delegate(object o, DownloadStringCompletedEventArgs e)
            {
                try
                {
                    var rootVenue = JObject.Parse(e.Result);
                    var items = rootVenue["response"]["photos"]["items"];
                    if (items.Count() > 0)
                    {
                        var firstPhoto = items[0];

                        string imageVenue = firstPhoto["prefix"].ToString() + "width100" +
                                            firstPhoto["suffix"].ToString();

                        this.imageVenue = imageVenue;
                        imgVenue.Source = new BitmapImage(new Uri(imageVenue, UriKind.Absolute));

                        Helper.MainPage.updateCurrentVenue();
                    }
                }
                catch (Exception)
                {
                    Debug.WriteLine("Some error occurred.");

                }
            };


            //Extensions.DownloadStringTask(webClientPhoto, new Uri(url, UriKind.Absolute));
            webClientPhoto.DownloadStringAsync(new Uri(url, UriKind.Absolute));



        }


    }
}
