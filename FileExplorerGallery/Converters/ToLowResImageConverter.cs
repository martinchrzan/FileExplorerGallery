using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace FileExplorerGallery.Converters
{
    public class ToLowResImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            var fullPath = new Uri((string)value, UriKind.Absolute);

            var smallerBitmapImage = new BitmapImage();
            smallerBitmapImage.BeginInit();
            smallerBitmapImage.DecodePixelWidth = 300;
            smallerBitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            smallerBitmapImage.UriSource = fullPath;
            smallerBitmapImage.EndInit();

            return smallerBitmapImage;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
