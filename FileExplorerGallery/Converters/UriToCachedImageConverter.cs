using FileExplorerGallery.ViewModelContracts;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace FileExplorerGallery.Converters
{
    public class UriToCachedImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value == null)
            {
                return null;
            }

            var image = (IImagePreviewItemViewModel)value;

            var fullPath = new Uri(image.Path, UriKind.Absolute);

            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.CreateOptions = image.NeedRefresh ? BitmapCreateOptions.IgnoreImageCache : BitmapCreateOptions.None;
            bitmapImage.UriSource = fullPath;
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            image.NeedRefresh = false;

            return bitmapImage;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
