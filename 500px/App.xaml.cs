using Picfinity.Common;
using Picfinity.DataModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Search;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Connectivity;
using Windows.UI.ApplicationSettings;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Grid App template is documented at http://go.microsoft.com/fwlink/?LinkId=234226

namespace Picfinity
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton Application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        protected override void OnWindowCreated(WindowCreatedEventArgs args)
        {
            // Place your CommandsRequested handler here to ensure your settings are available at all times in your app
            LoadSettings();
            SubscribeSearch();
        }

        private void SubscribeSearch()
        {
            SearchPane.GetForCurrentView().QuerySubmitted += new TypedEventHandler<SearchPane, SearchPaneQuerySubmittedEventArgs>(OnQuerySubmitted);
            SearchPane.GetForCurrentView().ShowOnKeyboardInput = true;
        }

        
        private void OnQuerySubmitted(object sender, SearchPaneQuerySubmittedEventArgs args)
        {

            if (string.IsNullOrEmpty(args.QueryText) || AppSettings.Oauth500Px == null)
            {
                return;
            }
            Dictionary<string, string> parameters = new Dictionary<string, string>()
            {
                {"consumer_key",AppSettings.ConsumerKey},
                {"term",args.QueryText}
            };

            string searchUrl = AppSettings.PhotosUrl + "/search" + "?consumer_key=" + AppSettings.ConsumerKey  +"&image_size[]=3&image_size[]=4" + "&term=" + args.QueryText;

            var result = new PhotoStream("Search - " + args.QueryText, searchUrl);
            Frame rootframe = Window.Current.Content as Frame;
            if (rootframe != null)
            {
                rootframe.Navigate(typeof(GroupDetailPage), result);
            }
        }


        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active

            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();
                //Associate the frame with a SuspensionManager key                                
                SuspensionManager.RegisterFrame(rootFrame, "AppFrame");

                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // Restore the saved session state only when appropriate
                    try
                    {
                        await SuspensionManager.RestoreAsync();
                    }
                    catch (SuspensionManagerException)
                    {
                        //Something went wrong restoring state.
                        //Assume there is no state and continue
                    }
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (!AppSettings.HasInternetConnectivity)
            {
                // navigate to no internet connectivity page
                rootFrame.Navigate(typeof(NoInternetPage));
            }
            else if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                if (!rootFrame.Navigate(typeof(GroupedItemsPage)))
                {
                    throw new Exception("Failed to create initial page");
                }
            }
            // Ensure the current window is active
            Window.Current.Activate();
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            //var deferral = e.SuspendingOperation.GetDeferral();
            //await SuspensionManager.SaveAsync();
            //deferral.Complete();
        }

        private void LoadSettings()
        {
            windowBounds = Window.Current.Bounds;
            SettingsPane.GetForCurrentView().CommandsRequested += onCommandsRequested;
        }

        void onCommandsRequested(SettingsPane settingsPane, SettingsPaneCommandsRequestedEventArgs eventArgs)
        {
            UICommandInvokedHandler handler = new UICommandInvokedHandler(onSettingsCommand);
            UICommandInvokedHandler handlerUrl = new UICommandInvokedHandler(onSettingsUrlCommand);

            SettingsCommand generalCommand = new SettingsCommand("AccountId", "Account", handler);
            eventArgs.Request.ApplicationCommands.Add(generalCommand);
            generalCommand = new SettingsCommand("TermsId", "500px Terms of Service", handlerUrl);
            eventArgs.Request.ApplicationCommands.Add(generalCommand);
            generalCommand = new SettingsCommand("PrivacyPolicyId", "Privacy Policy", handlerUrl);
            eventArgs.Request.ApplicationCommands.Add(generalCommand);
            generalCommand = new SettingsCommand("FeedbackId", "Feedback", handler);
            eventArgs.Request.ApplicationCommands.Add(generalCommand);
            generalCommand = new SettingsCommand("AboutId", "About", handler);
            eventArgs.Request.ApplicationCommands.Add(generalCommand);
        }

        private Popup settingsPopup;
        private double settingsWidth = 346;
        // Used to determine the correct height to ensure our custom UI fills the screen.
        private Rect windowBounds;

        private void OnWindowActivated(object sender, Windows.UI.Core.WindowActivatedEventArgs e)
        {
            if (e.WindowActivationState == Windows.UI.Core.CoreWindowActivationState.Deactivated)
            {
                settingsPopup.IsOpen = false;
            }
        }

        void OnPopupClosed(object sender, object e)
        {
            Window.Current.Activated -= OnWindowActivated;
        }

        async  void onSettingsUrlCommand(IUICommand command)
        {
            if (command.Id == "TermsId")
            {
                await Windows.System.Launcher.LaunchUriAsync(new Uri("http://500px.com/terms"));
            }
            else if (command.Id == "PrivacyPolicyId")
            {
                await Windows.System.Launcher.LaunchUriAsync(new Uri("http://picfinity.wordpress.com/picfinity-privacy-policy/"));
            }
        }

        void onSettingsCommand(IUICommand command)
        {

            // Create a Popup window which will contain our flyout.
            settingsPopup = new Popup();
            settingsPopup.Closed += OnPopupClosed;
            Window.Current.Activated += OnWindowActivated;
            settingsPopup.IsLightDismissEnabled = true;
            settingsPopup.Width = settingsWidth;
            settingsPopup.Height = windowBounds.Height;

            // Add the proper animation for the panel.
            settingsPopup.ChildTransitions = new TransitionCollection();
            settingsPopup.ChildTransitions.Add(new PaneThemeTransition()
            {
                Edge = (SettingsPane.Edge == SettingsEdgeLocation.Right) ?
                       EdgeTransitionLocation.Right :
                       EdgeTransitionLocation.Left
            });

            // Create a SettingsFlyout the same dimenssions as the Popup.
            var mypane = GetPane(command.Label);
            mypane.Width = settingsWidth;
            mypane.Height = windowBounds.Height;

            // Place the SettingsFlyout inside our Popup window.
            settingsPopup.Child = mypane;

            // Let's define the location of our Popup.
            settingsPopup.SetValue(Canvas.LeftProperty, SettingsPane.Edge == SettingsEdgeLocation.Right ? (windowBounds.Width - settingsWidth) : 0);
            settingsPopup.SetValue(Canvas.TopProperty, 0);
            settingsPopup.IsOpen = true;
        }

        private LayoutAwarePage GetPane(string title)
        {
            LayoutAwarePage returnPage;
            switch (title)
            {
                case "Account":
                    returnPage= new AccountSettings();
                    break;
                case "Feedback":
                    returnPage= new Feedback();
                    break;
                case "About":
                    returnPage= new AboutUs();
                    break;
                 default:
                    returnPage= null;
                    break;
            }
            return returnPage;
        }

    }
}
