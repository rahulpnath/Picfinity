using Picfinity.Common;
using Picfinity.DataModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Item Detail Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234232

namespace Picfinity
{
    /// <summary>
    /// A page that displays details for a single item within a group while allowing gestures to
    /// flip through other items belonging to the same group.
    /// </summary>
    public sealed partial class UserDetailPage : Picfinity.Common.LayoutAwarePage
    {
        public UserDetailPage()
        {
            this.InitializeComponent();
        }

        List<PhotoStream> UserStreams;

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
            // Allow saved page state to override the initial item to display
            if (pageState != null && pageState.ContainsKey("SelectedItem"))
            {
                navigationParameter = pageState["SelectedItem"];
            }

            User userDetails = navigationParameter as User;
            if (userDetails != null)
            {
                LoadDetails(userDetails);
            }
        }

        private async Task LoadDetails(User userDetails)
        {
            // if the page is for the current user then show the upload option
            if (AppSettings.CurrentUser != null && AppSettings.CurrentUser.user != null && userDetails.id == AppSettings.CurrentUser.user.id)
            {
                upload.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            else 
            {
                upload.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                aboutText.Height = 180;
                // load the details of the user.If it was the current user we would already have had the full details
                string url = AppSettings.BaseUrl + "users/show";
           var userResult= await  AppSettings.Oauth500Px.MakeRequest(Picfinity.Common.Oauth500px.RequestType.GET).ExecuteNonAuthorizedRequest<UserDetails>(url,
                    new Dictionary<string, string>() {
                        {"consumer_key", AppSettings.ConsumerKey},
                        {"id", userDetails.id.ToString()}
                    });
           userDetails = userResult.user;
            }
            progressBar.Visibility = Windows.UI.Xaml.Visibility.Visible;
            userProfile.DataContext = userDetails;
            // load the user specific photos 
            string userPhotos = AppSettings.PhotosUrl + "?user_id=" + userDetails.id + "&feature=user&consumer_key=51Se7wpTdDZPMD3edVsU7WzfCIl8kSDYsejF5TM7&image_size[]=3&image_size[]=4";
            PhotoStream stream = new PhotoStream(" Photos", userPhotos);
            string userFavs = AppSettings.PhotosUrl + "?user_id=" + userDetails.id + "&feature=user_favorites&consumer_key=51Se7wpTdDZPMD3edVsU7WzfCIl8kSDYsejF5TM7&image_size[]=3&image_size[]=4";
            PhotoStream streamFavs = new PhotoStream("Favorites", userFavs);
            UserStreams = new List<PhotoStream>() { stream, streamFavs };
            Task<bool> task = new Task<bool>(() => AssignDataSource());
            task.Start();
            bool result = await task;
            if (result)
            {
                this.DefaultViewModel["Groups"] = UserStreams;
                progressBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            else
            {
                // loading 
                this.Frame.Navigate(typeof(NoInternetPage));
            }
        }

        private bool AssignDataSource()
        {
            // we will wait for a minute to load all the streams. If not will assume there was a problem with internet connectivity/server api
            return WaitHandle.WaitAll(UserStreams.Select(a => a.DataLoaded).ToArray(), 1000 * 60);
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
            // TODO: Derive a serializable navigation parameter and assign it to pageState["SelectedItem"]
        }

        private void imageLoadingFailed(object sender, ExceptionRoutedEventArgs e)
        {
            userImage.Source = new BitmapImage(new Uri(this.BaseUri, "/images/userpic.png"));
        }

        /// <summary>
        /// Invoked when a group header is clicked.
        /// </summary>
        /// <param name="sender">The Button used as a group header for the selected group.</param>
        /// <param name="e">Event data that describes how the click was initiated.</param>
        void Header_Click(object sender, RoutedEventArgs e)
        {
            if (AppSettings.HasInternetConnectivity)
            {
                // Determine what group the Button instance represents
                var group = (sender as FrameworkElement).DataContext;

                // Navigate to the appropriate destination page, configuring the new page
                // by passing required information as a navigation parameter
                this.Frame.Navigate(typeof(GroupDetailPage), group);
            }
            else
            {
                this.Frame.Navigate(typeof(NoInternetPage));
            }
        }

        /// <summary>
        /// Invoked when an item within a group is clicked.
        /// </summary>
        /// <param name="sender">The GridView (or ListView when the application is snapped)
        /// displaying the item clicked.</param>
        /// <param name="e">Event data that describes the item clicked.</param>
        void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (AppSettings.HasInternetConnectivity)
            {
                // Navigate to the appropriate destination page, configuring the new page
                // by passing required information as a navigation parameter
                var photo = ((Photo)e.ClickedItem);
                foreach (var photoStream in UserStreams)
                {
                    if (photoStream.StreamPhotos.Contains(photo))
                    {
                        this.Frame.Navigate(typeof(ItemDetailPage), new Object[] { photoStream, photo });
                        break;
                    }

                }
            }
            else
            {
                this.Frame.Navigate(typeof(NoInternetPage));
            }
        }

        private void upload_Click_1(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(UploadPhoto));
        }

    }
}
