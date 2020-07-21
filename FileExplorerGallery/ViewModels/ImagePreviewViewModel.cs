using FileExplorerGallery.Common;
using FileExplorerGallery.Helpers;
using FileExplorerGallery.Settings;
using FileExplorerGallery.ViewModelContracts;
using FileExplorerGallery.VIewModelFactories;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FileExplorerGallery.ViewModels
{
    [Export(typeof(IImagePreviewViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class ImagePreviewViewModel : ViewModelBase, IImagePreviewViewModel
    {
        private static IEnumerable<string> recognisedImageExtensions = System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders().SelectMany(codec => codec.FilenameExtension.ToUpperInvariant().Split(';'));
        private const int MsPerSecond = 1000;
        private IThrottledActionInvoker _selectedImageThrottledActionInvoker;
        private IThrottledActionInvoker _selectedImageLowResThrottledActionInvoker;
        private IThrottledActionInvoker _slideshowScheduledActionInvoker;
        private readonly IThrottledActionInvokerFactory _throttledActionInvokerFactory;
        private readonly IImagePreviewItemViewModelFactory _imagePreviewItemViewModelFactory;
        private readonly IUserSettings _userSettings;
        private IImagePreviewItemViewModel _selectedImageInList;
        private IImagePreviewItemViewModel _selectedImage;
        private bool _nullSet;
        private int _slideshowDelayInSeconds;
        private bool _inSlideshowMode = false;
        private bool _noImagesMessageVisible = false;
        private bool _previousImageButtonVisible;
        private bool _nextImageButtonVisible;

        [ImportingConstructor]
        public ImagePreviewViewModel(IThrottledActionInvokerFactory throttledActionInvokerFactory,
            IImagePreviewItemViewModelFactory imagePreviewItemViewModelFactory,
            IUserSettings userSettings)
        {
            _throttledActionInvokerFactory = throttledActionInvokerFactory;
            _imagePreviewItemViewModelFactory = imagePreviewItemViewModelFactory;
            _userSettings = userSettings;

            CloseCommand = new RelayCommand(() =>
            {
                Application.Current.MainWindow.Close();
            });

            SlideshowCommand = new RelayCommand(() =>
            {
                _inSlideshowMode = true;
                SetSelectedImage(SelectedImageInList);
            });

            NextImageCommand = new RelayCommand(() =>
            {
                SelectedImage = Images.SkipWhile(it => it != SelectedImage).Skip(1).First();
                _selectedImageInList = SelectedImage;
                OnPropertyChanged(nameof(SelectedImageInList));
                NextImageButtonVisible = SelectedImage != Images.Last();
                PreviousImageButtonVisible = SelectedImage != Images.First();
            });

            PreviousImageCommand = new RelayCommand(() =>
            {
                SelectedImage = Images.TakeWhile(it => it != SelectedImage).Last();
                _selectedImageInList = SelectedImage;
                OnPropertyChanged(nameof(SelectedImageInList));
                PreviousImageButtonVisible = SelectedImage != Images.First();
                NextImageButtonVisible = SelectedImage != Images.Last();
            });
        }

        public void Initialize(string directoryPath, string selectedItem)
        {
            NoImagesMessageVisible = false;

            _selectedImageThrottledActionInvoker = _throttledActionInvokerFactory.CreateThrottledActionInvoker();
            _selectedImageLowResThrottledActionInvoker = _throttledActionInvokerFactory.CreateThrottledActionInvoker();
            _slideshowScheduledActionInvoker = _throttledActionInvokerFactory.CreateThrottledActionInvoker();
            _slideshowDelayInSeconds = _userSettings.SlideshowDuration.Value;

            LoadImageFiles(directoryPath, selectedItem);
            if(Images.Count == 0)
            {
                NoImagesMessageVisible = true;
            }
        }

        public void Dispose()
        {
            _selectedImageThrottledActionInvoker.Dispose();
            _selectedImageLowResThrottledActionInvoker.Dispose();
            _slideshowScheduledActionInvoker.Dispose();
        }

        public ObservableCollection<IImagePreviewItemViewModel> Images { get; private set; }

        public IImagePreviewItemViewModel SelectedImageInList
        {
            get
            {
                return _selectedImageInList;
            }
            set
            {
                _selectedImageInList = value;
                OnPropertyChanged();

                if (!_nullSet)
                {
                    SelectedImage = null;
                    SelectedImageLowRes = null;
                    OnPropertyChanged(nameof(SelectedImageLowRes));
                    _nullSet = true;
                }
                _selectedImageThrottledActionInvoker.ScheduleAction(() => {
                    SelectedImage = _selectedImageInList;
                    SelectedImageLowRes = null;
                    OnPropertyChanged(nameof(SelectedImageLowRes));
                    SetNavigationButtonsVisibility();
                }, 200);

                _selectedImageLowResThrottledActionInvoker.ScheduleAction(() =>
                {
                    SelectedImageLowRes = _selectedImageInList;
                    OnPropertyChanged(nameof(SelectedImageLowRes));
                    _nullSet = false;
                }, 50);

                SetSelectedImage(value);
            }
        }

        public IImagePreviewItemViewModel SelectedImage
        {
            get
            {
                return _selectedImage;
            }
            set
            {
                _selectedImage = value;
                OnPropertyChanged();
            }
        }

        public IImagePreviewItemViewModel SelectedImageLowRes { get; set; }

        public ICommand SlideshowCommand { get; set; }

        public ICommand CloseCommand { get; }

        public ICommand PreviousImageCommand { get; }

        public ICommand NextImageCommand { get; }

        public bool NoImagesMessageVisible
        {
            get
            {
                return _noImagesMessageVisible;
            }
            set
            {
                _noImagesMessageVisible = value;
                OnPropertyChanged();
            }
        }

        public bool PreviousImageButtonVisible
        {
            get
            {
                return _previousImageButtonVisible;
            }
            private set
            {
                _previousImageButtonVisible = value;
                OnPropertyChanged();
            }
        }

        public bool NextImageButtonVisible
        {
            get
            {
                return _nextImageButtonVisible;
            }
            private set
            {
                _nextImageButtonVisible = value;
                OnPropertyChanged();
            }
        }

        private void SetSelectedImage(IImagePreviewItemViewModel image)
        {
            if (_inSlideshowMode)
            {
                NextImageButtonVisible = false;
                PreviousImageButtonVisible = false;
                SelectedImage = image;
                _selectedImageInList = image;

                OnPropertyChanged(nameof(SelectedImageInList));

                if (Images.IndexOf(image) < Images.Count - 1)
                {
                    _slideshowScheduledActionInvoker.ScheduleAction(() =>
                    {
                        var currentIndex = Images.IndexOf(SelectedImage);
                        if (Images.Count > currentIndex + 1)
                        {
                            SetSelectedImage(Images[currentIndex + 1]);
                        }
                    }, _slideshowDelayInSeconds * MsPerSecond);
                }
            }
        }

        public BitmapSource Convert(System.Drawing.Bitmap bitmap)
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

        private void LoadImageFiles(string directoryPath, string selectedImage)
        {
            var imageFiles = recognisedImageExtensions.SelectMany(pattern => new DirectoryInfo(directoryPath).GetFiles(pattern, SearchOption.TopDirectoryOnly).Where(it => (it.Attributes & FileAttributes.Hidden)==0)).Distinct().ToArray();
           
            var images = new List<IImagePreviewItemViewModel>();

            foreach (var file in FileExplorerSortingProvider.Sort(imageFiles, directoryPath))
            {
                images.Add(_imagePreviewItemViewModelFactory.CreateImagePreviewItemViewModel(file.FullName, (SelectedImage) =>
                {
                    SelectedImageInList = SelectedImage;
                }));
            }
            Images = new ObservableCollection<IImagePreviewItemViewModel>(images);
            OnPropertyChanged(nameof(Images));

            if (Images.Count > 0)
            {
                if (string.IsNullOrEmpty(selectedImage))
                {
                    SelectedImage = Images.First();
                }
                else
                {
                    SelectedImage = Images.FirstOrDefault(it => Path.Equals(it.Path, selectedImage));
                }
                SetNavigationButtonsVisibility();
                _selectedImageInList = SelectedImage;
                OnPropertyChanged(nameof(SelectedImageInList));
            }
            else
            {
                SelectedImage = null;
            }
        }

        private void SetNavigationButtonsVisibility()
        {
            NextImageButtonVisible = SelectedImage != Images.Last();
            PreviousImageButtonVisible = SelectedImage != Images.First();
        }
    }
}
