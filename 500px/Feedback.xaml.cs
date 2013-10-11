using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ApplicationSettings;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Picfinity
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class Feedback : Picfinity.Common.LayoutAwarePage
    {
        public Feedback()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
           await Windows.System.Launcher.LaunchUriAsync(new Uri("https://www.facebook.com/Picfinity"));
        }

        

        /// <summary>
        /// This is the click handler for the back button on the Flyout.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void GoBack(object sender, RoutedEventArgs e)
        {
            // First close our Flyout.
            Popup parent = this.Parent as Popup;
            if (parent != null)
            {
                parent.IsOpen = false;
            }

            // If the app is not snapped, then the back button shows the Settings pane again.
            if (Windows.UI.ViewManagement.ApplicationView.Value != Windows.UI.ViewManagement.ApplicationViewState.Snapped)
            {
                SettingsPane.Show();
            }
        }

        private async void twitterClicked(object sender, RoutedEventArgs e)
        {
           await Windows.System.Launcher.LaunchUriAsync(new Uri("https://twitter.com/Picfinity"));
        }

        private async void MailClicked(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("mailto:picfinity@outlook.com?subject=Feedback: Pifinity"));
        }

        
    }
}
