using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace Perks
{
    public partial class AboutPage : PhoneApplicationPage
    {
        public AboutPage()
        {
            InitializeComponent();
        }

        //protected override void OnNavigatedTo(NavigationEventArgs e)
        //{
        //    string strItemIndex;
        //    if (NavigationContext.QueryString.TryGetValue("goto", out strItemIndex))
        //        thePivot.SelectedIndex = Convert.ToInt32(strItemIndex);

        //    base.OnNavigatedTo(e);
        //}
    }

}