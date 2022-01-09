using FileExplorerGallery.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FileExplorerGallery.Behaviors
{
    public static class GalleryCacheProvider
    {
        private static ItemsControl _galleryItemsControl;
        private static ScrollViewer _scrollViewer;
        private static Dictionary<string, Tuple<BitmapSource, int>> _thumbnails = new Dictionary<string, Tuple<BitmapSource, int>>();

        public static ItemsControl GetGalleryItemsControl(FrameworkElement childItem)
        {
            if (_galleryItemsControl == null)
            {
                _galleryItemsControl = ItemsControlParentProvider.FindParent<ItemsControl>(childItem);
            }
            return _galleryItemsControl;
        }

        internal static int GetThumbnailImageWidth(string pathToImage)
        {
            if (_thumbnails.ContainsKey(pathToImage))
            {
                return _thumbnails[pathToImage].Item2;
            }

            var width = 0;
            using (var fileStream = new FileStream(pathToImage, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (var image = System.Drawing.Image.FromStream(fileStream, false, false))
                {
                    var height = (double)image.Height;

                    // set width to correct aspect ratio (height is fixed to 100)
                    width = (int)Math.Floor((image.Width / height) * 100);
                }
            }

            _thumbnails[pathToImage] = new Tuple<BitmapSource, int>(null, width);
            return width;
        }

        static object _thumbLock = new object();

        internal static BitmapSource GetThumbnail(string value)
        {
            lock (_thumbLock)
            {
                if (_thumbnails.ContainsKey(value))
                {
                    if (_thumbnails[value].Item1 != null)
                    {
                        return _thumbnails[value].Item1;
                    }
                    else
                    {
                        var width = _thumbnails[value].Item2;

                        var thumbnail = Convert(WindowsThumbnailProvider.GetThumbnail((string)value, width, 100, ThumbnailOptions.None));
                        _thumbnails[value] = new Tuple<BitmapSource, int>(thumbnail, width);
                        return _thumbnails[value].Item1;
                    }

                }
            }

            return null;
        }

        public static ScrollViewer GetGalleryScrollViewer(FrameworkElement childItem)
        {
            if (_scrollViewer == null)
            {
                _scrollViewer = ItemsControlParentProvider.FindParent<ScrollViewer>(childItem);
            }
            return _scrollViewer;
        }

        public static void ResetThumbnail(string pathToImage)
        {
            _thumbnails.Remove(pathToImage);
            GetThumbnailImageWidth(pathToImage);
        }

        public static void ClearCache()
        {
            _thumbnails.Clear();
            _galleryItemsControl = null;
            _scrollViewer = null;
        }

        public static BitmapSource Convert(Bitmap bitmap)
        {
            var bitmapData = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);

            var bitmapSource = BitmapSource.Create(
                bitmapData.Width, bitmapData.Height,
                bitmap.HorizontalResolution, bitmap.VerticalResolution,
                PixelFormats.Pbgra32, null,
                bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

            bitmap.UnlockBits(bitmapData);
            return bitmapSource;
        }
    }
}
