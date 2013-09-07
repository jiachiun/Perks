using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;

namespace Perks
{
    public partial class AboutPage2 : PhoneApplicationPage
    {
        public AboutPage2()
        {
            InitializeComponent();

            btnRate.Tap += btnRate_Tap;
            btnFeedback.Tap += btnFeedback_Tap;
            txtDeveloperLink.Tap += TxtDeveloperLinkOnTap;
        }

        private void TxtDeveloperLinkOnTap(object sender, GestureEventArgs gestureEventArgs)
        {
            WebBrowserTask webBrowserTask = new WebBrowserTask();
            webBrowserTask.Uri = new Uri("http://jiachiun.net", UriKind.Absolute);
            webBrowserTask.Show();
        }

        void btnRate_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            MarketplaceReviewTask marketplaceReviewTask = new MarketplaceReviewTask();
            marketplaceReviewTask.Show();
        }

        void btnFeedback_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            EmailComposeTask emailComposeTask = new EmailComposeTask();

            emailComposeTask.Subject = "Feedback on Perks";
            emailComposeTask.To = "jiachiun@live.com";

            emailComposeTask.Show();
        }
    }
}