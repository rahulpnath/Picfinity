using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace Picfinity.DataModel
{
    public class UploadData : BaseEntity
    {
        private static List<String> ImageProperties = new List<string>()
        {
            "System.Photo.ExposureTimeNumerator",
            "System.Photo.CameraModel",
            "System.Photo.ExposureTimeDenominator",
            "System.Photo.FocalLengthNumerator",
            "System.Photo.ISOSpeed",
            "System.Photo.FNumber"
        };

        private UploadData()
        {
            
        }

        public List<string> CategoryOptions { get { return AppSettings.AllCategories.Values.Where(a => a != "All").ToList<String>(); } }

        public string SelectedCategory { get; set; }

        public int SelectedCategoryIndex {
            get
            {
                if (string.IsNullOrEmpty(SelectedCategory))
                {
                    // return Uncategorized Index
                    return 0;
                }
                else
                {
                    KeyValuePair<int,string>? item =  AppSettings.AllCategories.FirstOrDefault(a => a.Value == SelectedCategory);
                    if (item.HasValue )
                    {
                        return item.Value.Key;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }
        }

        public StorageFile File { get; set; }

        public BitmapImage Source { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int Category { get; set; }

        public string ShutterSpeed { get; set; }

        public string FocalLength { get; set; }

        public string Aperture { get; set; }

        public string ISO { get; set; }

        public string Camera { get; set; }

        public string Lens { get; set; }

        private bool _isUploading;
        public bool IsUploading
        {
            get { return _isUploading; }
            set { _isUploading = value; RaisePropertyChanged("IsUploading"); }
        }

        private bool _isUploaded;
        public bool IsUploaded
        {
            get { return _isUploaded; }
            set { _isUploaded = value; RaisePropertyChanged("IsUploaded"); }
        }

        private bool _isUploadFailed;
        public bool IsUploadFailed
        {
            get { return _isUploadFailed; }
            set { _isUploadFailed = value; RaisePropertyChanged("IsUploadFailed"); }
        }

        public UploadData(StorageFile file)
        {
            File = file;
            this.SelectedCategory = "Uncategorized";
            LoadData();
        }

        private async void LoadData()
        {
            var thumbnail = await File.GetThumbnailAsync(Windows.Storage.FileProperties.ThumbnailMode.PicturesView, 380);
            Source = new BitmapImage();
            Source.SetSource(thumbnail);
            RaisePropertyChanged("Source");
            PopulateImageProperties();
        }

        private async void PopulateImageProperties()
        {
            var prop = await File.Properties.GetImagePropertiesAsync();
            var list = await prop.RetrievePropertiesAsync(ImageProperties);
            if (list.ContainsKey("System.Photo.CameraModel"))
            {
                Camera = list["System.Photo.CameraModel"].ToString();
                RaisePropertyChanged("Camera");
            }

            if (list.ContainsKey("System.Photo.ISOSpeed"))
            {
                ISO = list["System.Photo.ISOSpeed"].ToString();
                RaisePropertyChanged("ISO");
            }

            if (list.ContainsKey("System.Photo.ExposureTimeDenominator"))
            {
                ShutterSpeed = "1/" + list["System.Photo.ExposureTimeDenominator"].ToString() + " sec";
                RaisePropertyChanged("ShutterSpeed");
            }

            if (list.ContainsKey("System.Photo.FocalLengthNumerator"))
            {
                FocalLength = list["System.Photo.FocalLengthNumerator"].ToString() + " mm";
                RaisePropertyChanged("FocalLength");
            }

            if (list.ContainsKey("System.Photo.FNumber"))
            {
                Aperture = "f/" + list["System.Photo.FNumber"].ToString();
                RaisePropertyChanged("Aperture");
            }



        }
    }

}
