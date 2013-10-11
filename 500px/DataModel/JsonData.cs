using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Picfinity.DataModel
{
    public class Filters
    {
        public string category { get; set; }
        public string exclude { get; set; }
    }

     

    public class Image
    {
        public int size { get; set; }
        public string url { get; set; }
    }

    public class Contacts
    {
        public string website { get; set; }
        public string twitter { get; set; }
        public string livejournal { get; set; }
        public string flickr { get; set; }
        public string gtalk { get; set; }
        public string skype { get; set; }
        public string facebook { get; set; }
        public string facebookpage { get; set; }
    }

    public class Equipment
    {
        public List<string> camera { get; set; }
        public List<string> lens { get; set; }
    }

    public class Auth
    {
        public int facebook { get; set; }
        public int twitter { get; set; }
    }

    public class User
    {
        public int id { get; set; }
        public string username { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public object birthday { get; set; }
        public int sex { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string country { get; set; }
        public string registration_date { get; set; }
        public string about { get; set; }
        public string domain { get; set; }
        public int upgrade_status { get; set; }
        public bool fotomoto_on { get; set; }
        public string locale { get; set; }
        public bool show_nude { get; set; }
        public bool store_on { get; set; }
        public Contacts contacts { get; set; }
        public Equipment equipment { get; set; }
        public string fullname { get; set; }
        public string userpic_url { get; set; }
        public string email { get; set; }
        public int photos_count { get; set; }
        public int affection { get; set; }
        public int in_favorites_count { get; set; }
        public int friends_count { get; set; }
        public int followers_count { get; set; }
        public object upload_limit { get; set; }
        public string upload_limit_expiry { get; set; }
        public string upgrade_status_expiry { get; set; }
        public Auth auth { get; set; }
    }


    public class FavouriteResponse
    {
        public int status { get; set; }
        public string message { get; set; }
        public string error { get; set; }
    }

    public class Photo
    {
        public int id { get; set; }
        public int user_id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string camera { get; set; }
        public string lens { get; set; }
        public string focal_length { get; set; }
        public string iso { get; set; }
        public string shutter_speed { get; set; }
        public string aperture { get; set; }
        public int? times_viewed { get; set; }
        public double? rating { get; set; }
        public int? status { get; set; }
        public string created_at { get; set; }
        public int? category { get; set; }
        public string category_name { get; set; }
        public string categoryName
        {
            get { return AppSettings.AllCategories[category ?? 0]; }
        }

        public bool favorited { get; set; }

        public string favouritedImage
        {
            get { return favorited ? "images/fav_on.png" : "images/fav_off.png"; }
        }
                     
        public object location { get; set; }
        public string privacy { get; set; }
        public object latitude { get; set; }
        public object longitude { get; set; }
        public object taken_at { get; set; }
        public int? hi_res_uploaded { get; set; }
        public bool for_sale { get; set; }
        public int? width { get; set; }
        public int ?height { get; set; }
        public int? votes_count { get; set; }
        public int? favorites_count { get; set; }
        public int? comments_count { get; set; }
        public bool nsfw { get; set; }
        public bool voted { get; set; }
        public bool purchased { get; set; }
        public int? sales_count { get; set; }
        public object for_sale_date { get; set; }
        public double? highest_rating { get; set; }
        public string highest_rating_date { get; set; }
        public List<Image> images { get; set; }
        public string imageHighRes
        {
            get
            {
                if (images != null && images.Count > 0)
                {
                    return images.OrderByDescending(a => a.size).FirstOrDefault().url;
                }
                else
                    return string.Empty;
            }
        }

        public string imageLowRes
        {
            get
            {
                if (images != null && images.Count > 0)
                {
                    return images.OrderBy(a => a.size).FirstOrDefault().url;
                }
                else
                    return string.Empty;
            }
        }

        public bool store_download { get; set; }
        public bool store_print { get; set; }
        public User user { get; set; }
    }

    public class Comment
    {
        public int id { get; set; }
        public int user_id { get; set; }
        public int to_whom_user_id { get; set; }
        public string body { get; set; }
        public string created_at { get; set; }
        public object parent_id { get; set; }
        public User user { get; set; }
    }

    public class CommentDetails
    {
        public string media_type { get; set; }
        public int current_page { get; set; }
        public int total_pages { get; set; }
        public int total_items { get; set; }
        public List<Comment> comments { get; set; }
    }

    public class RootObject
    {
        public string feature { get; set; }
        public Filters filters { get; set; }
        public int current_page { get; set; }
        public int total_pages { get; set; }
        public int total_items { get; set; }
        public List<Photo> photos { get; set; }
    }

    public class PhotoDetails
    {
        public Photo photo { get; set; }
        public List<Comment> comments { get; set; }
    }

    public class UserDetails
    {
        public User user { get; set; }   
    }

    public class LikeDetails
    {
        public int current_page { get; set; }
        public int total_pages { get; set; }
        public int total_items { get; set; }
        public List<User> users { get; set; }
    }
}
