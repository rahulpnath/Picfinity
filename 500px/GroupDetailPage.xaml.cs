using Picfinity.DataModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Group Detail Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234229

namespace Picfinity
{
    /// <summary>
    /// A page that displays an overview of a single group, including a preview of the items
    /// within the group.
    /// </summary>
    public sealed partial class GroupDetailPage : Picfinity.Common.LayoutAwarePage
    {
        public GroupDetailPage()
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
            // TODO: Create an appropriate data model for your problem domain to replace the sample data
            LoadDetails(navigationParameter);
        }

        protected override void GoBack(object sender, RoutedEventArgs e)
        {
            if (AppSettings.HasInternetConnectivity)
            {
                base.GoBack(sender, e);
            }
            else
            {
                this.Frame.Navigate(typeof(NoInternetPage));
            }
        }

        private void ShowHideUserButton()
        {
            if (AppSettings.CurrentUser != null)
            {
                loginUser.Visibility = Visibility.Collapsed;
                currentUser.DataContext = AppSettings.CurrentUser;
                currentUser.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            else
            {
                currentUser.Visibility = Visibility.Collapsed;
                loginUser.Visibility = Visibility.Visible;
            }
        }

        private async void LoadDetails(Object navigationParameter)
        {
            ShowHideUserButton();

            progressBar.Visibility = Windows.UI.Xaml.Visibility.Visible;
            var photoStream = (PhotoStream)navigationParameter;

            this.DefaultViewModel["Group"] = photoStream;
            Task task = new Task(() => photoStream.DataLoaded.WaitOne());
            task.Start();
            await task;
            progressBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            if (photoStream.StreamPhotos.Count == 0)
            {
                noResults.Visibility = Windows.UI.Xaml.Visibility.Visible;
                itemGridView.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            else
            {
                noResults.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                itemGridView.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
        }

        /// <summary>
        /// Invoked when an item is clicked.
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
                var itemId = ((Photo)e.ClickedItem);
                this.Frame.Navigate(typeof(ItemDetailPage), new Object[] { this.DefaultViewModel["Group"], itemId });
            }
            else
            {
                this.Frame.Navigate(typeof(NoInternetPage));
            }
        }

        private void userPicFailed(object sender, ExceptionRoutedEventArgs e)
        {
            currentUserImage.Source = new BitmapImage(new Uri(this.BaseUri, "/images/userpic.png"));
        }

        private void currentUserClicked(object sender, RoutedEventArgs e)
        {
            popupOptions.IsOpen = true;
        }

        private async void loginUser_Click_1(object sender, RoutedEventArgs e)
        {
            MessageDialog dialog = new MessageDialog("Log in to get a personalized experience. Kindly use the username and password login, as facebook ,twitter and klout login have some problems");
            await dialog.ShowAsync();
            
            await AppSettings.AuthenticateNew();
            ShowHideUserButton();

        }

        private void pageRoot_Loaded_1(object sender, RoutedEventArgs e)
        {
            if (App.Current.Resources.ContainsKey("CurrentItem"))
            {
                Photo currentPhoto = (Photo)App.Current.Resources["CurrentItem"];
                // remove the item from the resource so that we dont focus this again
                App.Current.Resources.Remove("CurrentItem");
                Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => { itemGridView.ScrollIntoView(currentPhoto); });
            }
        }

        private void CurrentUserProfile(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(UserDetailPage), AppSettings.CurrentUser.user);
        }

        private void CurrentUserSignOut(object sender, RoutedEventArgs e)
        {
            popupOptions.IsOpen = false;
            AppSettings.LogoutUser();
            ShowHideUserButton();
        }

        private void CurrentUserUploadPhotos(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(UploadPhoto));
        }
    }
}
