using System.Net.Http;
using System.Runtime.Serialization.Json;
using Picfinity.Common;
using Picfinity.DataModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Windows.UI;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.ApplicationSettings;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Animation;
using System.Threading.Tasks;
using Windows.ApplicationModel.Search;
using System.Net;
using Windows.Storage;
using Windows.System.UserProfile;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

// The Item Detail Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234232

namespace Picfinity
{
    /// <summary>
    /// A page that displays details for a single item within a group while allowing gestures to
    /// flip through other items belonging to the same group.
    /// </summary>
    public sealed partial class ItemDetailPage : Picfinity.Common.SharePage, INotifyPropertyChanged
    {
        public PhotoDetails CurrentPhotoDetails { get; set; }

        public ItemDetailPage()
        {
            this.InitializeComponent();
            selectedPhotoDetails.DataContext = this;

        }

        private PhotoStream currentStream;
        private Photo currentItem;
        private bool IsCommentsExpanded;

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

            //if (AppSettings.CurrentUser != null)
            //{
            //    currentUser.DataContext = AppSettings.CurrentUser;
            //    currentUser.Visibility = Windows.UI.Xaml.Visibility.Visible;
            //}

            // TODO: Create an appropriate data model for your problem domain to replace the sample data
            var navParamAsArray = navigationParameter as Object[];
            Photo item = null;
            PhotoStream stream = null;
            // If the details are passed in as the navigation parameter then we use that
            if (navParamAsArray != null)
            {
                item = navParamAsArray[1] as Photo;
                stream = navParamAsArray[0] as PhotoStream;
            }

            if (item != null && stream != null)
            {
                // store the stream and the current photo so that we can navigate back to this page
                currentStream = stream;
                this.DefaultViewModel["Group"] = stream.StreamPhotos;
                flipView.SelectedItem = item;
                currentItem = item;
                GetPhotoDetails();
            }

            IsInSlideShow = false;
            playSlideShow.Visibility = Windows.UI.Xaml.Visibility.Visible;
            stopSlideShow.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

        }


        private bool IsInSlideShow = false;


        private void flipView_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (AppSettings.HasInternetConnectivity)
            {
                commentsList.ItemsSource = null;
                defaultUserImage.Visibility = Visibility.Visible;
                currentItem = flipView.SelectedItem as Photo;
                userImage.Visibility = Visibility.Collapsed;
                PhotoStream stream = currentStream;
                if (stream != null && stream.StreamPhotos.HasMoreItems &&
                    flipView.SelectedIndex + 5 >= stream.StreamPhotos.Count)
                {
                    var noAwait = stream.StreamPhotos.LoadMoreItemsAsync(0);
                }
                SetCurrentPhotoDetails(null);
                if (!IsInSlideShow)
                {
                    // if in slide show mode we do not need to load the details
                    GetPhotoDetails();
                }
            }
            else
            {
                this.Frame.Navigate(typeof(NoInternetPage));
            }
        }




        private void ShowHideAppBarButtons()
        {
            if (AppSettings.Oauth500Px == null || !AppSettings.Oauth500Px.IsAuthenticated)
            {
                // like and favorite not valid for not logged in user
                likePhoto.IsEnabled = false;
                UnFavoritePhoto.IsEnabled = false;
                favoritePhoto.IsEnabled = false;
                newComment.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                commentButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                purchasePhoto.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            else
            {
                if (!CurrentPhotoDetails.photo.purchased)
                {
                    purchasePhoto.Visibility = Windows.UI.Xaml.Visibility.Visible;
                }
                else
                {
                    purchasePhoto.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                }

                newComment.Visibility = Windows.UI.Xaml.Visibility.Visible;
                commentButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
                // Set the state of the appbar button for like photo
                likePhoto.IsEnabled = !CurrentPhotoDetails.photo.voted;

                // Set the state of the appbar button for favorite
                if (CurrentPhotoDetails.photo.favorited)
                {
                    UnFavoritePhoto.Visibility = Visibility.Visible; favoritePhoto.Visibility = Visibility.Collapsed;
                    UnFavoritePhoto.IsEnabled = true;
                }
                else
                {
                    UnFavoritePhoto.Visibility = Visibility.Collapsed; favoritePhoto.Visibility = Visibility.Visible;
                    favoritePhoto.IsEnabled = true;
                }
            }
        }


        private void detailsAppBarShowClicked(object sender, RoutedEventArgs e)
        {
            if (!IsInSlideShow)
            {
                ShowPhotoDetails();
            }
        }

        private void ShowPhotoDetails()
        {
            selectedPhotoDetails.Visibility = Visibility.Visible;
            flipView.SetValue(Grid.ColumnSpanProperty, 1);
            commentsDetails.SetValue(Grid.ColumnSpanProperty, 1);
            bottomAppBar.IsOpen = false;
            detailsAppBarHide.Visibility = Windows.UI.Xaml.Visibility.Visible;
            detailsAppBarShow.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private void detailsAppBarHideClicked(object sender, RoutedEventArgs e)
        {
            HidePhotoDetails();
        }

        private void HidePhotoDetails()
        {
            selectedPhotoDetails.Visibility = Visibility.Collapsed;
            flipView.SetValue(Grid.ColumnSpanProperty, 2);
            commentsDetails.SetValue(Grid.ColumnSpanProperty, 2);
            bottomAppBar.IsOpen = false;
            detailsAppBarShow.Visibility = Windows.UI.Xaml.Visibility.Visible;
            detailsAppBarHide.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private void userImage_ImageOpened_1(object sender, RoutedEventArgs e)
        {
            defaultUserImage.Visibility = Visibility.Collapsed;
            userImage.Visibility = Visibility.Visible;
        }

        private async void GetPhotoDetails()
        {
            progressBar.Visibility = Visibility.Visible;
            string photoDetails;

            PhotoDetails temp;
            // if authorized you would get the user specific details too like voted/favourited
            // If not request to get the additional photo details
            if (AppSettings.Oauth500Px != null && AppSettings.Oauth500Px.IsAuthenticated)
            {
                photoDetails = AppSettings.BaseUrl + "photos/" + currentItem.id;
                temp = await
                AppSettings.Oauth500Px.MakeRequest(Oauth500px.RequestType.GET).ExecuteRequest<PhotoDetails>(
                    photoDetails, null);
            }
            else
            {
                photoDetails = AppSettings.BaseUrl + "photos/" + currentItem.id + "?consumer_key=" + AppSettings.ConsumerKey;
                temp = await
                AppSettings.Oauth500Px.MakeRequest(Oauth500px.RequestType.GET).ExecuteNonAuthorizedRequest<PhotoDetails>(
                    photoDetails, null);
            }

            //HttpClient client = new HttpClient();
            //HttpResponseMessage response = await client.GetAsync(photoDetails);
            //var data = await response.Content.ReadAsStreamAsync();
            //DataContractJsonSerializer json = new DataContractJsonSerializer(typeof(PhotoDetails));
            //PhotoDetails dat = json.ReadObject(data) as PhotoDetails;
            if (temp != null)
                SetCurrentPhotoDetails(temp);
            progressBar.Visibility = Visibility.Collapsed;
        }

        private void SetCurrentPhotoDetails(PhotoDetails dat)
        {
            if (dat != null)
            {
                CurrentPhotoDetails = dat;
                ShowHideAppBarButtons();
                if (PropertyChanged != null)
                    this.PropertyChanged(this, new PropertyChangedEventArgs("CurrentPhotoDetails"));
            }

        }



        private async void LoadComments()
        {
            if (commentsDetails.Visibility == Windows.UI.Xaml.Visibility.Collapsed)
            {
                return;
            }
            if (commentsList.ItemsSource == null)
            {
                progressRing.Visibility = Visibility.Visible;
                progressRing.IsActive = true;

                var commentsUrl = AppSettings.BaseUrl + "photos/" + currentItem.id + "/comments?consumer_key=" + AppSettings.ConsumerKey;
                IncrementalSource<CommentDetails, Comment> paged = new IncrementalSource<CommentDetails, Comment>(commentsUrl, CommentDetailsResponse);

                commentsList.ItemsSource = paged;
                await paged.LoadMoreItemsAsync(1);
                progressRing.Visibility = Visibility.Collapsed;
                progressRing.IsActive = false;
            }
        }

        protected override void GoBack(object sender, RoutedEventArgs e)
        {
            if (AppSettings.HasInternetConnectivity)
            {
                //add the current item to the resource so that we can focus this on the GroupDetailPage
                App.Current.Resources["CurrentItem"] = currentItem;
                base.GoBack(sender, e);
            }
            else
            {
                this.Frame.Navigate(typeof(NoInternetPage));
            }
        }

        private PagedResponse<Comment> CommentDetailsResponse(CommentDetails rootObject)
        {
            return new PagedResponse<Comment>(rootObject.comments, rootObject.total_items, rootObject.comments == null ? 0 : rootObject.comments.Count);
        }

        public event PropertyChangedEventHandler PropertyChanged;


        private async System.Threading.Tasks.Task<bool> LikeCurrentPhoto()
        {
            if (CurrentPhotoDetails.photo.voted)
            {
                return false;
            }
            Dictionary<string, string> parameters = new Dictionary<string, string>
                                                        {
                                                            {"vote", "1"}
                                                        };
            var photo =
                await
                AppSettings.Oauth500Px.MakeRequest(Oauth500px.RequestType.POST).ExecuteRequest<PhotoDetails>(
                    AppSettings.BaseUrl + "photos/" + currentItem.id + "/vote", parameters);
            if (photo != null)
            {
                // voted is getting returned as false even after voting
                photo.photo.voted = !photo.photo.voted;
                this.SetCurrentPhotoDetails(photo);
            }
            return CurrentPhotoDetails.photo.voted;
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            await FavUnfavPhoto();
        }

        private async Task FavUnfavPhoto()
        {
            if (AppSettings.Oauth500Px == null || !AppSettings.Oauth500Px.IsAuthenticated)
            {
                return;
            }
            FavouriteResponse response;
            if (!CurrentPhotoDetails.photo.favorited)
            {
                response =
                   await
                   AppSettings.Oauth500Px.MakeRequest(Oauth500px.RequestType.POST).ExecuteRequest<FavouriteResponse>(
                       AppSettings.BaseUrl + "photos/" + currentItem.id + "/favorite", null);
            }
            else
            {
                response =
                  await
                  AppSettings.Oauth500Px.MakeRequest(Oauth500px.RequestType.DELETE).ExecuteRequest<FavouriteResponse>(
                      AppSettings.BaseUrl + "photos/" + currentItem.id + "/favorite", null);
            }
            if (response != null && response.status == 200)
            {
                GetPhotoDetails();
            }
        }

        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {

            Dictionary<string, string> parameters = new Dictionary<string, string>()
                                                       {
                                                           {"body", newComment.Text}
                                                       };
            var commentUrl = AppSettings.BaseUrl + "photos/" + currentItem.id + "/comments";
            var response = await AppSettings.Oauth500Px.MakeRequest(Oauth500px.RequestType.POST).ExecuteRequest<FavouriteResponse>(
                    commentUrl, parameters);
            if (response != null && response.status == 200)
            {

            }
        }

        private void TextBlock_Tapped_1(object sender, TappedRoutedEventArgs e)
        {

        }

        private void RichTextBlock_PointerEntered_1(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor =
new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Hand, 1);

        }

        private void RichTextBlock_PointerExited_1(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor =
            new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 2);
        }




        private void newComment_GotFocus_1(object sender, RoutedEventArgs e)
        {
            SearchPane.GetForCurrentView().ShowOnKeyboardInput = false;
            //if (!IsCommentsExpanded)
            //{
            //    newComment.Height = newComment.Height * 2;
            //    IsCommentsExpanded = true;

            //}
        }

        private void newComment_LostFocus_1(object sender, RoutedEventArgs e)
        {
            SearchPane.GetForCurrentView().ShowOnKeyboardInput = true;
            //if (IsCommentsExpanded )
            //{
            //    newComment.Height = newComment.Height / 2;
            //    IsCommentsExpanded = false;
            //}
        }

        private void toggleAppBar_Checked_2(object sender, RoutedEventArgs e)
        {
            commentsDetails.Visibility = Windows.UI.Xaml.Visibility.Visible;
            LoadComments();
        }

        private void toggleAppBar_Unchecked_2(object sender, RoutedEventArgs e)
        {
            commentsDetails.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private void HomeClicked(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(GroupedItemsPage), null);
            bottomAppBar.IsOpen = false;
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (timer != null)
            {
                timer.Tick -= timer_Tick;
                timer = null;
            }
            base.OnNavigatingFrom(e);
        }

        DispatcherTimer timer;
        private void SlideShowClicked(object sender, RoutedEventArgs e)
        {
            // Hide details if open. As we dont want to show details in slideshow mode
            HidePhotoDetails();
            playSlideShow.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            stopSlideShow.Visibility = Windows.UI.Xaml.Visibility.Visible;
            IsInSlideShow = true;
            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 3);
            timer.Tick += timer_Tick;
            timer.Start();
        }

        private void StopSlideShowClicked(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            timer.Tick -= timer_Tick;
            timer = null;
            IsInSlideShow = false;
            playSlideShow.Visibility = Windows.UI.Xaml.Visibility.Visible;
            stopSlideShow.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        void timer_Tick(object sender, object e)
        {
            // restart from the start once the source end is reached
            int nextIndex = flipView.SelectedIndex + 1;
            flipView.SelectedIndex = currentStream.StreamPhotos.Count > nextIndex ? nextIndex : 0;
        }

        private void CommentClicked(object sender, RoutedEventArgs e)
        {
            commentsDetails.Visibility = Windows.UI.Xaml.Visibility.Visible;
            LoadComments();
            bottomAppBar.IsOpen = false;
        }

        private async void LikeClicked(object sender, RoutedEventArgs e)
        {
            var liked = await LikeCurrentPhoto();
            if (liked)
            {
                likePhoto.IsEnabled = false;
            }
            bottomAppBar.IsOpen = false;
        }

        public void FlipViewTapped(object sender, TappedRoutedEventArgs e)
        {
            commentsDetails.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private async void FavoriteClicked(object sender, RoutedEventArgs e)
        {
            await FavUnfavPhoto();
        }

        private async void UnFavoriteClicked(object sender, RoutedEventArgs e)
        {
            await FavUnfavPhoto();
        }



        protected override void GetShareContent(DataRequest request)
        {
            DataPackage requestData = request.Data;
            requestData.Properties.Title = "Picfinity";
            requestData.Properties.Description = CurrentPhotoDetails.photo.description; // The description is optional.
            requestData.SetUri(new Uri("http://500px.com/photo/" + CurrentPhotoDetails.photo.id.ToString()));
        }

        private async void OnSetLockScreen(object sender, RoutedEventArgs e)
        {
            try
            {
                StorageFile imageFile = await StorageFile.CreateStreamedFileFromUriAsync("test.jpg", new Uri(currentItem.imageHighRes), null);

                if (imageFile != null)
                {

                    // Application now has access to the picked file, setting image to lockscreen.  This will fail if the file is an invalid format.
                    await LockScreen.SetImageFileAsync(imageFile);

                    // Retrieve the lock screen image that was set
                    IRandomAccessStream imageStream = LockScreen.GetImageStream();

                    if (imageStream != null)
                    {
                        BitmapImage lockScreen = new BitmapImage();
                        lockScreen.SetSource(imageStream);
                    }
                }
            }
            catch (Exception)
            {
            }

        }

        private void userPicFailed(object sender, ExceptionRoutedEventArgs e)
        {
            currentUserImage.Source = new BitmapImage(new Uri(this.BaseUri, "/images/userpic.png"));
        }

        private void currentUserClicked(object sender, RoutedEventArgs e)
        {

            this.Frame.Navigate(typeof(UserDetailPage), AppSettings.CurrentUser.user);
        }

        private void photoUserClicked(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(UserDetailPage), currentItem.user);
        }

        private async void purchasePhotoClicked(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri(string.Format(AppSettings.PurchaseUrl, currentItem.id)));
        }




    }
}
