using Picfinity.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Windows.UI.Popups;

namespace Picfinity.DataModel
{
    public class AppSettings
    {
        static AppSettings()
        {
            BaseUrl = "https://api.500px.com/v1/";
            ConsumerKey = "YOUR CONSUMER KEY";
            ConsumerSecret = "YOUR CONSUMER SECRET";
            UsersUrl = BaseUrl + "users";
            PhotosUrl = BaseUrl + "photos";
            OAuthAccessUrl = BaseUrl + "oauth/access_token";
            OAuthRequestUrl = BaseUrl + "oauth/request_token";
            OAuthAuthorizeUrl = BaseUrl + "oauth/authorize";
            OAuthCallbackUrl = "http://www.bing.com/";
            PurchaseUrl = "https://500px.com/photo/{0}/download";
            UpgradeUrl = "https://500px.com/upgrade";

            FormDataBoundary ="--0246824681357ACXZabcxyz";

            #region Available Categories
            AllCategories = new Dictionary<int, string>()
                       {
                           {-1,"All"},
                           {0,"Uncategorized"},
                           {1,"Celebrities"},
                           {2,"Film"},
                           {3,"Journalism"},
                           {4,"Nude"},
                           {5,"Black and White"},
                           {6,"Still Life"},
                           {7,"People"},
                           {8,"Landscapes"},
                           {9,"City and Architecture"},
                           {10,"Abstract"},
                           {11,"Animals"},
                           {12,"Macro"},
                           {13,"Travel"},
                           {14,"Fashion"},
                           {15,"Commercial"},
                           {16,"Concert"},
                           {17,"Sport"},
                           {18,"Nature"},
                           {19,"Performing Arts"},
                           {20,"Family"},
                           {21,"Street"},
                           {22,"Underwater"},
                           {23,"Food"},
                           {24,"Fine Art"},
                           {25,"Wedding"},
                           {26,"Transportation"},
                           {27,"Urban Exploration"}
                       };

            #endregion

            #region Available Photo Streams

            Streams = new Dictionary<string, string>()
                                 {
                                     {"Popular","popular"},
                                     {"Upcoming","upcoming"},
                                     {"Editor's Choice","editors"},
                                     {"Today","fresh_today"},
                                     {"Yesterday","fresh_yesterday"},
                                     {"Week","fresh_week"}
                                 };

            #endregion

            LoadStreams();
        }

        public static void LoadStreams()
        {
            AllStreams = new List<PhotoStream>();
            AllStreams.AddRange(Streams.Select(a => new PhotoStream(a.Key)).ToList());
        }

        public static string SelectedCategory { get; set; }

        public static string PurchaseUrl { get; set; }

        public static string UpgradeUrl { get; set; }

        public static string FormDataBoundary { get; internal set; }

        public static bool HasInternetConnectivity
        {
            get
            {
                ConnectionProfile connections = NetworkInformation.GetInternetConnectionProfile();
                return connections != null && connections.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess;
            }
        }

        public static string AccessToken { get; set; }

        public static Oauth500px Oauth500Px { get; set; }

        public static UserDetails CurrentUser { get; set; }

        public static string BaseUrl { get; private set; }

        public static Dictionary<int, string> AllCategories { get; private set; }

        public static string ConsumerSecret { get; private set; }

        public static string ConsumerKey { get; private set; }

        public static string OAuthRequestUrl { get; private set; }

        public static string OAuthAuthorizeUrl { get; private set; }

        public static string OAuthAccessUrl { get; private set; }

        public static string OAuthCallbackUrl { get; private set; }

        public static string UsersUrl { get; private set; }

        public static string PhotosUrl { get; private set; }

        public static Dictionary<string, string> Streams { get; set; }

        public static List<PhotoStream> AllStreams { get; set; }

        public static void LogoutUser()
        {
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("token"))
            {
                ApplicationData.Current.LocalSettings.Values.Remove("token");
                ApplicationData.Current.LocalSettings.Values.Remove("SecretCode");
                ApplicationData.Current.LocalSettings.Values.Remove("Verifier");
            }
            CurrentUser = null;
            AppSettings.Oauth500Px = new Oauth500px(AppSettings.ConsumerKey, AppSettings.ConsumerSecret,
                                                AppSettings.OAuthCallbackUrl, AppSettings.OAuthRequestUrl,
                                                AppSettings.OAuthAuthorizeUrl, AppSettings.OAuthAccessUrl);
        }

        public static async Task<bool> LoginExistingUser()
        {
            Oauth500px oauth = new Oauth500px(AppSettings.ConsumerKey, AppSettings.ConsumerSecret,
                                                AppSettings.OAuthCallbackUrl, AppSettings.OAuthRequestUrl,
                                                AppSettings.OAuthAuthorizeUrl, AppSettings.OAuthAccessUrl);
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("token"))
            {
                oauth.IsAuthenticated = true;
                Picfinity.Common.Oauth500px.OauthToken token = new Oauth500px.OauthToken();
                token.Token = ApplicationData.Current.LocalSettings.Values["token"] as string;
                token.SecretCode = ApplicationData.Current.LocalSettings.Values["SecretCode"] as string;
                token.Verifier = ApplicationData.Current.LocalSettings.Values["Verifier"] as string;
                oauth.Token = token;
            }
            AppSettings.Oauth500Px = oauth;
            await GetCurrentUser();
            return true;
        }

        public static async Task<bool> AuthenticateNew()
        {

            if (AppSettings.Oauth500Px == null || !AppSettings.Oauth500Px.IsAuthenticated)
            {
                Oauth500px oauth = new Oauth500px(AppSettings.ConsumerKey, AppSettings.ConsumerSecret,
                                                  AppSettings.OAuthCallbackUrl, AppSettings.OAuthRequestUrl,
                                                  AppSettings.OAuthAuthorizeUrl, AppSettings.OAuthAccessUrl);
                await oauth.Authenticate();
                AppSettings.Oauth500Px = oauth;

                if (oauth.IsAuthenticated)
                {
                    await GetCurrentUser();

                    // Save the token details
                    ApplicationData.Current.LocalSettings.Values["token"] = oauth.Token.Token;
                    ApplicationData.Current.LocalSettings.Values["SecretCode"] = oauth.Token.SecretCode;
                    ApplicationData.Current.LocalSettings.Values["Verifier"] = oauth.Token.Verifier;
                }
            }
            return true;
        }

        public static async Task GetCurrentUser()
        {
            if (AppSettings.Oauth500Px != null && AppSettings.Oauth500Px.IsAuthenticated)
            {
                // load current user details only if the user is logged in 
                AppSettings.CurrentUser =
                      await
                      AppSettings.Oauth500Px.MakeRequest(Oauth500px.RequestType.GET).ExecuteRequest<UserDetails>(
                          "https://api.500px.com/v1/users", null);
            }
        }
    }
}
