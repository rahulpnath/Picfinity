using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Picfinity.DataModel
{
    public class PhotoStream
    {
        public PhotoStream(string title)
        {
            this.Title = title;
            this.PhotoStreamUrl = AppSettings.PhotosUrl + "?feature=" + AppSettings.Streams[title] + "&consumer_key=" + AppSettings.ConsumerKey + "&rpp={0}" + "&image_size[]=3&image_size[]=4" + "&exclude=Nude";
            if (!string.IsNullOrEmpty( AppSettings.SelectedCategory) && AppSettings.SelectedCategory != "All")
            {
                this.PhotoStreamUrl += "&only=" + AppSettings.SelectedCategory;
            }
            StreamPhotos = new IncrementalSource<RootObject, Photo>(PhotoStreamUrl, RootObjectResponse);
            DataLoaded = new ManualResetEvent(false);
            LoadPhotos();
        }

        private PagedResponse<Photo> RootObjectResponse(RootObject rootObject)
        {
            if (rootObject != null)
            {
                return new PagedResponse<Photo>(rootObject.photos, rootObject.total_items, rootObject.photos != null ? rootObject.photos.Count : 0);
            }
            else
            {
                // if no details are returned propably there was an error at the server/network
                List<Photo> photos = new List<Photo>();
                return new PagedResponse<Photo>(photos, 0, 0);
            }
        }

        public PhotoStream(string title, string url)
        {
            this.Title = title;
            this.PhotoStreamUrl = url;
            StreamPhotos = new IncrementalSource<RootObject, Photo>(PhotoStreamUrl, RootObjectResponse);
            DataLoaded = new ManualResetEvent(false);
            LoadPhotos();
        }

        public async void LoadPhotos()
        {
            // load the initial set of photos 
            if ( StreamPhotos.Count == 0)
            {
                var result = await StreamPhotos.LoadMoreItemsAsync(8);
                (DataLoaded as ManualResetEvent).Set();
                // Load some more photos so that we have some photos when we go into a category
                await StreamPhotos.LoadMoreItemsAsync(0);
            }
        }

        public WaitHandle DataLoaded { get; set; }

        public string Title { get; set; }

        public String PhotoStreamUrl { get; set; }

        public IncrementalSource<RootObject, Photo> StreamPhotos { get; set; }

        public List<Photo> FilteredPhotos
        {

            get
            {
                return StreamPhotos.Take(8).ToList();
            }
        }

    }
}
