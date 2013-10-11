using Picfinity.Common;
using Picfinity.DataModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Search;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
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
    public sealed partial class UploadPhoto : Picfinity.Common.LayoutAwarePage
    {
        public ObservableCollection<UploadData> Photos { get; set; }
        private int? uploadCount = null;

        public UploadPhoto()
        {
            this.InitializeComponent();
            Photos = new ObservableCollection<UploadData>();
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
            // the user details needs to be fetched again, so that we have the correct upload count
            RefreshUserDetails();
            uploadPhotos.ItemsSource = Photos;
            LoadPictures();
            if (AppSettings.CurrentUser != null && AppSettings.CurrentUser.user.upload_limit != null)
            {
                int count = 0;
                // check if there is a limit to upload photos
                if (int.TryParse(AppSettings.CurrentUser.user.upload_limit.ToString(), out count))
                {
                    uploadCount = count;
                    CheckUploadCount();
                    uploadMessage.Visibility = Windows.UI.Xaml.Visibility.Visible;
                }
            }
        }

        private void CheckUploadCount()
        {
            if (uploadCount.HasValue)
            {
                // set the text to reflect this
                if (uploadCount > 0)
                {
                    addPhotosText.Text = "You can upload " + uploadCount + " more photos. ";
                }
                else
                {
                    // user has reached upload limit for the week
                    addPhotosText.Text = "You have reached your upload limit for the week.";
                    // disable upload options
                    uploadPhoto.IsEnabled = false;
                    addPhotos.IsEnabled = false;
                }
                upgrdaeAccountText.Visibility = Windows.UI.Xaml.Visibility.Visible;
                
            }
        }

        private static async Task RefreshUserDetails()
        {
            await AppSettings.GetCurrentUser();
        }

        private async void LoadPictures()
        {
            bool unsnapped = ((ApplicationView.Value != ApplicationViewState.Snapped) || ApplicationView.TryUnsnap());
            if (unsnapped)
            {
                FileOpenPicker openPicker = new FileOpenPicker();
                openPicker.ViewMode = PickerViewMode.Thumbnail;
                openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
                
                openPicker.FileTypeFilter.Add(".jpg");
                openPicker.FileTypeFilter.Add(".jpeg");
                var selectedFiles = await openPicker.PickMultipleFilesAsync();

                
                if (selectedFiles.Count > 0)
                {
                    uploadPhotos.Visibility = Visibility.Visible;

                    addPhotosText.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

                    foreach (var file in selectedFiles)
                    {
                        Photos.Add(new UploadData(file));
                    }
                    uploadPhoto.IsEnabled = true;
                    uploadMessage.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                }
                addPhotos.IsEnabled = true;
                bottomAppBar.IsOpen = true;
            }
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

        private void DisableSearchPane(object sender, RoutedEventArgs e)
        {
            SearchPane.GetForCurrentView().ShowOnKeyboardInput = false;
        }

        private void EnableSearchPane(object sender, RoutedEventArgs e)
        {
            SearchPane.GetForCurrentView().ShowOnKeyboardInput = true;
        }

        

        private async void uploadPhotos_Click_1(object sender, RoutedEventArgs e)
        {
            uploadPhoto.IsEnabled = false;
            addPhotos.IsEnabled = false;

            // check for upload limit
            if (uploadCount != null)
            {
                //this means there is a upload limit
                var extraFiles = uploadPhotos.Items.Skip(uploadCount.Value).Cast<UploadData>().ToList();
                if (extraFiles.Count() > 0)
                {
                    var fileNames = string.Join(Environment.NewLine,
                                   extraFiles.Select(a => a.File.Name).ToArray());
                    string message = "These files could not be uploaded because uploading them would put you over your limit of photos for the week. To have unlimited uploads upgrade your account." + Environment.NewLine;
                    message += fileNames;

                    // these files cannot be uploaded as there is a upload limit
                    MessageDialog dialog = new MessageDialog(message);
                    
                    dialog.Options = MessageDialogOptions.AcceptUserInputAfterDelay;
                    await dialog.ShowAsync();
                    // remove the extra items
                   foreach (var item in extraFiles)
                   {
                       Photos.Remove(item);
                   }
                }
            }

            foreach (UploadData photo in uploadPhotos.Items)
            {
                // upload only photos that are not uploaded once
                if (!photo.IsUploaded)
                {
                    photo.IsUploading = true;
                    bool isUpload = await UploadPhotoFile(photo);

                    photo.IsUploading = false;

                    photo.IsUploaded = isUpload;
                    photo.IsUploadFailed = !isUpload;
                }
            }
            uploadPhoto.IsEnabled = true;
            addPhotos.IsEnabled = true;
            // check if all photos were added successfully

        }

        protected async override void GoBack(object sender, RoutedEventArgs e)
        {
            //check if there are pending photos to be uploaded and raise
            // a confirmation message
            if (uploadPhotos.Items.Cast<UploadData>().Any(a => a.IsUploadFailed || a.IsUploading || !a.IsUploaded))
            {
                MessageDialog dialog = new MessageDialog("There are pending or failed uploads. Are you sure you want to cancel");
                dialog.Commands.Add(new UICommand("Ok", new UICommandInvokedHandler(OkCommandHandler)));
                dialog.Commands.Add(new UICommand("Cancel", new UICommandInvokedHandler(CancelCommandHandler)));

                dialog.Options = MessageDialogOptions.AcceptUserInputAfterDelay;
                await dialog.ShowAsync();
            }
            else
            {
                base.GoBack(sender, e);
            }
        }

        private void OkLimitCommandHandler(IUICommand command)
        {
            
        }



        private void OkCommandHandler(IUICommand command)
        {
            base.GoBack(this, null);
        }

        private void CancelCommandHandler(IUICommand command)
        {

        }
        private async Task<bool> UploadPhotoFile(UploadData file)
        {

            // var read = file.OpenReadAsync();
            string postUrl = AppSettings.PhotosUrl + "/upload";
            byte[] formData = await HtmlHelper.GetMultiFormPostData(file.File);
            Dictionary<string,string> param = new Dictionary<string, string>() 
                { 
                { "name", file.Name ?? String.Empty },
                { "description", file.Description ?? String.Empty },
                { "category", file.SelectedCategoryIndex.ToString() },
                { "privacy", "0" }
                };
            if (!string.IsNullOrEmpty(file.ShutterSpeed))
            {
                param.Add("shutter_speed", file.ShutterSpeed);
            }

            if (!string.IsNullOrEmpty(file.FocalLength))
            {
                param.Add("focal_length", file.FocalLength);
            }

            if (!string.IsNullOrEmpty(file.Aperture))
            {
                param.Add("aperture", file.Aperture);
            }

            if (!string.IsNullOrEmpty(file.ISO))
            {
                param.Add("iso", file.ISO);
            }

            if (!string.IsNullOrEmpty(file.Camera))
            {
                param.Add("camera", file.Camera);
            }

            var result = await AppSettings.Oauth500Px.WithData(formData).MakeRequest(Picfinity.Common.Oauth500px.RequestType.POST).
                ExecuteRequest<PhotoDetails>(postUrl, param);
            if (result == null)
            {
                //photo was not uploaded 
                return false;
            }
            
            // reduce the upload count if it has a value, by 1
            uploadCount = uploadCount == null ? null : uploadCount - 1;

            // we need to set the privacy flag as there is some bug in the upload endpoint which is not honoring the privacy flag
            string privacyUrl = AppSettings.PhotosUrl + "/" + result.photo.id;
            var result1 = await AppSettings.Oauth500Px.MakeRequest(Picfinity.Common.Oauth500px.RequestType.PUT).
                  ExecuteRequest<Photo>(privacyUrl, new Dictionary<string, string>() { { "privacy", "0" } });
            if (result1 == null)
            {
                return false;
            }
            return true;
        }

        private void deletePhoto_Click_1(object sender, RoutedEventArgs e)
        {
            if (uploadPhotos.SelectedItems != null)
            {
                while (uploadPhotos.SelectedItems.Count > 0)
                {
                    Photos.Remove((UploadData)uploadPhotos.SelectedItem);
                }
            }
            if (uploadPhotos.Items.Count == 0)
            {
                addPhotosText.Visibility = Windows.UI.Xaml.Visibility.Visible;
                uploadPhotos.Visibility = Visibility.Collapsed;
                uploadPhoto.IsEnabled = false;
                CheckUploadCount();
                uploadMessage.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
        }

        private void uploadPhotos_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (uploadPhotos.SelectedItem != null)
            {
                deletePhoto.IsEnabled = true;
            }
            else
            {
                deletePhoto.IsEnabled = false;
            }
        }

        private void addPhotos_Click_1(object sender, RoutedEventArgs e)
        {
            LoadPictures();
        }

        private async void upgrdaeAccountText_Tapped_1(object sender, TappedRoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri(string.Format(AppSettings.UpgradeUrl)));
        }
    }
}
