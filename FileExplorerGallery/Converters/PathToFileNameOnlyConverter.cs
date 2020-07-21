using FileExplorerGallery.Helpers;
using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace FileExplorerGallery.Converters
{
    public class PathToFileNameOnlyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                try
                {
                    return Path.GetFileName((string)value);
                }
                catch (Exception ex)
                {
                    Logger.LogError("Trying to get filename out of " + value.ToString() + " " + ex.Message);
                    return string.Empty;
                }
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
