using System;
using System.Collections;
using System.Device.Location;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using Nokia.Phone.HereLaunchers;

namespace Perks
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Variables for AnimatedScrollViewer
        private static ScrollViewer _scrollViewerVenues;
        private static double VENUE_CONTROL_WIDTH;
        private static int _currentVenueControlId = 0;   // default 0

        private bool alreadyHookedScrollEvents = false;
        private static bool _isAnimatingScrollViewer = false;
        private static DispatcherTimer _offsetTimer;  // for scrollviewer
        private static double _currentOffset;
        private static double _finalOffset;



        // Constructor
        public MainPage()
        {
            InitializeComponent();

            Helper.MainPage = this;

            btnDirections.Tap += BtnDirectionsOnTap;
            btnWebsites.Tap += BtnWebsitesOnTap;

            OnVenueControlSelectionChanged += MainPage_OnVenueControlSelectionChanged;
            scrollViewerVenues.Loaded += ScrollViewerVenuesOnLoaded;

            populateVenues();

            InitializeAnimatedScrollViewer();

        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {

        }

        public void updateCurrentVenue()
        {
            Venue venue = Helper.venues[_currentVenueControlId];

            // temp function to show ads control for demo purposes
            if(venue.imageType == "ads")
            {

                imgAds.Visibility = Visibility.Visible;

                TranslateTransform translateTransform = new TranslateTransform();
                translateTransform.X = -130;
                btnWebsites.RenderTransform = translateTransform;

                txtName.Text = venue.name;

                txtAddress.Text = venue.address;
                txtMessage.Text = venue.message;

                

                BitmapImage bmp = new BitmapImage(new Uri(venue.imageVenue, UriKind.Relative));
                imgAds.Source = bmp;
                imgVenue.Source = bmp;
                return;
            }
            else
            {
                imgAds.Visibility = Visibility.Collapsed;

                TranslateTransform translateTransform = new TranslateTransform();
                translateTransform.X = 0;
                btnWebsites.RenderTransform = translateTransform;
            }

            txtName.Text = venue.name;

            // location
            txtAddress.Text = venue.address;
            txtDistance.Text = venue.distance;

            // specials
            txtMessage.Text = venue.message;
            txtDescription.Text = venue.description;
            txtFinePrint.Text = venue.fineprint;

            // stats
            txtCheckins.Text = venue.checkinsCount;
            txtVisitors.Text = venue.usersCount;


            string imageVenue = ((VenueControl)stackPanelVenues.Children[_currentVenueControlId]).imageVenue;

            if (imageVenue != "")
            {
                BitmapImage bmp = new BitmapImage(new Uri(imageVenue, UriKind.Absolute));
                imgVenue.Source = bmp;
                imgVenueBig.Source = bmp;
            }
        }

        void MainPage_OnVenueControlSelectionChanged(int selectedIndex)
        {
            if (stackPanelVenues.Children.Count == 0)
                return;

            updateCurrentVenue();
        }


        #region AnimatedScrollViewer
        private void InitializeAnimatedScrollViewer()
        {
            _scrollViewerVenues = scrollViewerVenues;

            // To animate scrollviewer
            _currentOffset = scrollViewerVenues.HorizontalOffset;

            int i = 0;

            foreach (var venueControl in stackPanelVenues.Children)
            {
                VENUE_CONTROL_WIDTH = ((VenueControl)venueControl).Width;
                ((VenueControl)venueControl).index = i;
                venueControl.Tap += delegate(object sender, GestureEventArgs args)
                {
                    _currentVenueControlId =
                        ((VenueControl)venueControl).index;
                    _currentOffset = scrollViewerVenues.HorizontalOffset;
                    _finalOffset = (((VenueControl)venueControl).index) * VENUE_CONTROL_WIDTH;

                    if (!_isAnimatingScrollViewer)
                    {
                        _isAnimatingScrollViewer = true;
                        AnimateScrollToHorizontalOffset();
                    }
                };
                i++;
            }
            adjustVenueControlMargin();

            // to show some initial animation, scrolling to the 2nd one
            if (stackPanelVenues.Children.Count > 1)
            {
                _currentOffset = (stackPanelVenues.Children.Count - 1) * VENUE_CONTROL_WIDTH;
                _finalOffset = 1 * VENUE_CONTROL_WIDTH;
                AnimateScrollToHorizontalOffset();
            }
        }

        public delegate void OnVenueControlSelectionChangedHandler(int selectedIndex);
        public static event OnVenueControlSelectionChangedHandler OnVenueControlSelectionChanged;

        // Add left margin and right margin to the first and last child of the stackPanel to allow scrolling to both ends
        // Note: Always call this function when the stackPanel is updated.
        private void adjustVenueControlMargin()
        {
            int count = stackPanelVenues.Children.Count;
            //double LEADING_MARGIN = (scrollViewerVenues.Width - 3 * VENUE_CONTROL_WIDTH);  // We want to show only 3 controls on the main page
            double LEADING_MARGIN = VENUE_CONTROL_WIDTH;  // We want to show only 3 controls on the main page

            if (count == 0)
                return;

            if (count == 1)
            {
                ((VenueControl)stackPanelVenues.Children[0]).Margin = new Thickness(LEADING_MARGIN, 0, 160, 0);
            }
            else if (count >= 2)
            {
                ((VenueControl)stackPanelVenues.Children[0]).Margin = new Thickness(LEADING_MARGIN, 0, 0, 0);
                ((VenueControl)stackPanelVenues.Children[count - 1]).Margin = new Thickness(0, 0, LEADING_MARGIN, 0);

            }

        }


        private void ScrollViewerVenuesOnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            _scrollViewerVenues = sender as ScrollViewer;

            Binding binding = new Binding();
            binding.Source = _scrollViewerVenues;
            binding.Path = new PropertyPath("HorizontalOffset");
            binding.Mode = BindingMode.OneWay;
            this.SetBinding(ScrollViewHorizontalOffsetProperty, binding);


            // To detect if it is end of scrolling / inertia
            if (alreadyHookedScrollEvents)
                return;

            alreadyHookedScrollEvents = true;
            ScrollViewer viewer = FindSimpleVisualChild<ScrollViewer>(scrollViewerVenues);
            if (viewer != null)
            {
                // Visual States are always on the first child of the control template 
                FrameworkElement element = VisualTreeHelper.GetChild(viewer, 0) as FrameworkElement;
                if (element != null)
                {
                    VisualStateGroup group = FindVisualState(element, "ScrollStates");
                    if (group != null)
                    {
                        group.CurrentStateChanging += scrollViewer_CurrentStateChanging;
                    }
                }
            }
        }

        void scrollViewer_CurrentStateChanging(object sender, VisualStateChangedEventArgs e)
        {

            if (e.NewState.Name == "NotScrolling")
            {
                _currentOffset = scrollViewerVenues.HorizontalOffset;
                _currentVenueControlId = getCurrentVenueControlId(_currentOffset);
                _finalOffset = (_currentVenueControlId) * VENUE_CONTROL_WIDTH;

                AnimateScrollToHorizontalOffset();
            }


        }

        VisualStateGroup FindVisualState(FrameworkElement element, string name)
        {
            if (element == null)
                return null;

            IList groups = VisualStateManager.GetVisualStateGroups(element);
            foreach (VisualStateGroup group in groups)
                if (group.Name == name)
                    return group;

            return null;
        }

        T FindSimpleVisualChild<T>(DependencyObject element) where T : class
        {
            while (element != null)
            {

                if (element is T)
                    return element as T;

                element = VisualTreeHelper.GetChild(element, 0);
            }

            return null;
        }

        private static void AnimateScrollToHorizontalOffset()
        {

            _currentOffset = _scrollViewerVenues.HorizontalOffset;

            _offsetTimer = new DispatcherTimer();
            _offsetTimer.Interval = new TimeSpan(0, 0, 0, 0, 10);
            _offsetTimer.Tick += delegate
            {

                double STEP_VALUE = 20;

                double delta = Math.Abs(_finalOffset - _currentOffset);
                if (delta < STEP_VALUE)
                    STEP_VALUE = delta;

                if ((int)_finalOffset > (int)_currentOffset)
                    _currentOffset += STEP_VALUE;
                else if ((int)_finalOffset < (int)_currentOffset)
                    _currentOffset -= STEP_VALUE;
                else
                {
                    _offsetTimer.Stop();
                    _isAnimatingScrollViewer = false;
                    _currentVenueControlId = getCurrentVenueControlId(_finalOffset);
                    OnVenueControlSelectionChanged(_currentVenueControlId);
                }

                _scrollViewerVenues.ScrollToHorizontalOffset(_currentOffset);

            };
            _offsetTimer.Start();
        }

        public static readonly DependencyProperty ScrollViewHorizontalOffsetProperty =
        DependencyProperty.Register(
                                    "ScrollViewHorizontalOffset",
                                    typeof(double),
                                    typeof(MainPage),
                                    new PropertyMetadata(new PropertyChangedCallback(OnScollViewHorizontalOffsetChanged))
                                    );

        public double ScrollViewHorizontalOffset
        {
            get { return (double)this.GetValue(ScrollViewHorizontalOffsetProperty); }
            set { this.SetValue(ScrollViewHorizontalOffsetProperty, value); }
        }



        private static void OnScollViewHorizontalOffsetChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {

            int tempVenueControlId = _currentVenueControlId;

            _currentVenueControlId = getCurrentVenueControlId((double)e.NewValue);

            if (tempVenueControlId != _currentVenueControlId)
                OnVenueControlSelectionChanged(_currentVenueControlId);

        }

        private static int getCurrentVenueControlId(double offset)
        {
            int result;

            result = ((int)Math.Round(offset / VENUE_CONTROL_WIDTH)); // ID for the first item is 0

            return result;
        }



        #endregion


        private void BtnWebsitesOnTap(object sender, GestureEventArgs gestureEventArgs)
        {
            WebBrowserTask webBrowserTask = new WebBrowserTask();

            // append foursquare attribution to link https://developer.foursquare.com/overview/attribution
            webBrowserTask.Uri = new Uri(Helper.venues[_currentVenueControlId].canonicalUrl + "?ref=G4DJSUBSHHYUNMF2PISEAWFQZS1ZALJHMOHDCBG5UPNSCNQM", UriKind.Absolute);
            webBrowserTask.Show();
        }

        private void BtnDirectionsOnTap(object sender, GestureEventArgs gestureEventArgs)
        {
            try
            {
                DirectionsRouteDestinationTask routeTo = new DirectionsRouteDestinationTask();

                var venue = Helper.venues[_currentVenueControlId];
                routeTo.Destination = new GeoCoordinate(Double.Parse(venue.latitude), Double.Parse(venue.longitude));
                routeTo.Mode = RouteMode.Car;
                routeTo.Show();
            }
            catch (Exception erno)
            {
                MessageBox.Show("Error message: " + erno.Message);
            }
        }

        private void populateVenues()
        {
            stackPanelVenues.Children.Clear();

            foreach (var venue in Helper.venues)
            {
                stackPanelVenues.Children.Add(new VenueControl(venue));
            }
        }


        private void appBarBtnRefresh_Click(object sender, System.EventArgs e)
        {
            Helper.isRefreshing = true;
            NavigationService.GoBack();
        }

        private void appBarMenuItemAbout_Click(object sender, System.EventArgs e)
        {
            // go to about pivot
            //NavigationService.Navigate(new Uri("/AboutPage.xaml?goto=0", UriKind.Relative));
            NavigationService.Navigate(new Uri("/AboutPage2.xaml", UriKind.Relative));

        }

        private void appBarBtnInfo_Click(object sender, System.EventArgs e)
        {
            // go to info pivot
            NavigationService.Navigate(new Uri("/AboutPage.xaml", UriKind.Relative));
        }


        private void appBarBtnShare_Click(object sender, System.EventArgs e)
        {

            string longUrl = Helper.venues[_currentVenueControlId].canonicalUrl;
            string url = string.Format(@"http://api.bit.ly/v3/shorten?login={0}&apiKey={1}&longUrl={2}&format=txt", "o_7c1i6hsafe", "R_c4e9455c739d9d3bfe7abba51e3da2bf", HttpUtility.UrlEncode(longUrl));

            loadingBar.IsIndeterminate = true;
            loadingBar.Visibility = Visibility.Visible;
            
            
            WebClient wc = new WebClient();
            wc.OpenReadCompleted += new OpenReadCompletedEventHandler(wc_OpenReadCompleted);
            wc.OpenReadAsync(new Uri(url));

        }

        void wc_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            Stream stream = e.Result;
            var reader = new StreamReader(stream);
            var shortenedUrl = reader.ReadToEnd();


            loadingBar.Visibility = Visibility.Collapsed;

            ShareStatusTask shareStatusTask = new ShareStatusTask();

            shareStatusTask.Status = String.Format("Check out this perk: {0}. More at {1}", Helper.venues[_currentVenueControlId].message, shortenedUrl);

            shareStatusTask.Show();

        }


    }
}