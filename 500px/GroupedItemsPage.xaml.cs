using System.Net;
using System.Text.RegularExpressions;
using Windows.Security.Authentication.Web;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using Picfinity.DataModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using System.Threading;
using Picfinity.Common;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Core;


// The Grouped Items Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234231

namespace Picfinity
{
    /// <summary>
    /// A page that displays a grouped collection of items.
    /// </summary>
    public sealed partial class GroupedItemsPage : Picfinity.Common.LayoutAwarePage
    {
        public GroupedItemsPage()
        {
            this.InitializeComponent();
        }

        private void SetCategoryFilterOptions()
        {
            if (AppSettings.CurrentUser != null && AppSettings.CurrentUser.user.show_nude)
            {
                categoryOptions.ItemsSource = AppSettings.AllCategories.Values;
            }
            else
            {
                // nude option should be there only if its enabled in the profile 
                categoryOptions.ItemsSource = AppSettings.AllCategories.Values.Where(a => a != "Nude");
            }

            if (string.IsNullOrEmpty(AppSettings.SelectedCategory))
            {
                categoryOptions.SelectedItem = "All";
            }
            else
            {
                categoryOptions.SelectedItem = AppSettings.SelectedCategory;
            }
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
            LoadCurrentUser();
            
            SetCategoryFilterOptions();

            LoadData();
        }

        private async void LoadCurrentUser()
        {
            await AppSettings.LoginExistingUser();
            ShowHideUserButton();
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

        private async void LoadData()
        {
            categoryOptions.IsEnabled = false;
            progressBar.Visibility = Visibility.Visible;
            // we need to trigger the load data to make sure that the data is loaded
            foreach (var item in AppSettings.AllStreams)
            {
                item.LoadPhotos();
            }
            Task<bool> task = new Task<bool>(() => AssignDataSource());
            task.Start();
            bool result = await task;
            if (result)
            {
                // check if any one stream was populated
                if (AppSettings.AllStreams.Any(a => a.FilteredPhotos != null && a.FilteredPhotos.Count() > 0))
                {
                    this.DefaultViewModel["Groups"] = AppSettings.AllStreams;
                    progressBar.Visibility = Visibility.Collapsed;
                }
                else
                {
                    // none of the streams were populated
                    this.Frame.Navigate(typeof(NoInternetPage));
                }
            }
            else
            {
                this.Frame.Navigate(typeof(NoInternetPage));
            }
            categoryOptions.IsEnabled = true;
        }

        private bool AssignDataSource()
        {
            // we will wait for a minute to load all the streams. If not will assume there was a problem with internet connectivity/server api
            return WaitHandle.WaitAll(AppSettings.AllStreams.Select(a => a.DataLoaded).ToArray(), 1000 * 60);
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
                foreach (var photoStream in AppSettings.AllStreams)
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

        private void categoryOptions_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            AppSettings.SelectedCategory = (String)categoryOptions.SelectedItem;
            AppSettings.LoadStreams();
            this.LoadData();
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
            SetCategoryFilterOptions();
        }

        private void CurrentUserUploadPhotos(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(UploadPhoto));
        }
    }
}
