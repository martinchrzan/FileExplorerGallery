using FileExplorerGallery.Behaviors;
using FileExplorerGallery.Common;
using FileExplorerGallery.Helpers;
using FileExplorerGallery.Settings;
using FileExplorerGallery.ViewModelContracts;
using FileExplorerGallery.VIewModelFactories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly DialogHelper _dialogHelper;
        private readonly ApplicationDispatcher _applicationDispatcher;
        private readonly ImageHelper _imageHelper;
        private IImagePreviewItemViewModel _selectedImageInList;
        private IImagePreviewItemViewModel _selectedImage;
        private bool _nullSet;
        private int _slideshowDelayInSeconds;
        private bool _inSlideshowMode = false;
        private bool _noImagesMessageVisible = false;
        private bool _previousImageButtonVisible;
        private bool _nextImageButtonVisible;
        private int _rotation;
        private bool _saveVisible;
        private bool _savingImageMessageVisible;

        [ImportingConstructor]
        public ImagePreviewViewModel(IThrottledActionInvokerFactory throttledActionInvokerFactory,
            IImagePreviewItemViewModelFactory imagePreviewItemViewModelFactory,
            IUserSettings userSettings,
            DialogHelper dialogHelper,
            ApplicationDispatcher applicationDispatcher,
            ImageHelper imageHelper)
        {
            _throttledActionInvokerFactory = throttledActionInvokerFactory;
            _imagePreviewItemViewModelFactory = imagePreviewItemViewModelFactory;
            _userSettings = userSettings;
            _dialogHelper = dialogHelper;
            _applicationDispatcher = applicationDispatcher;
            _imageHelper = imageHelper;
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

            DeleteImageCommand = new RelayCommand(() => DeleteSelectedImage());

            SaveCommand = new RelayCommand(() => SaveSelectedImage());
        }

        public void Initialize(string directoryPath, string selectedItem)
        {
            NoImagesMessageVisible = false;
            SaveVisible = false;

            _selectedImageThrottledActionInvoker = _throttledActionInvokerFactory.CreateThrottledActionInvoker();
            _selectedImageLowResThrottledActionInvoker = _throttledActionInvokerFactory.CreateThrottledActionInvoker();
            _slideshowScheduledActionInvoker = _throttledActionInvokerFactory.CreateThrottledActionInvoker();
            _slideshowDelayInSeconds = _userSettings.SlideshowDuration.Value;

            LoadImageFiles(directoryPath, selectedItem);
            if (Images.Count == 0)
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

                _selectedImageThrottledActionInvoker.ScheduleAction(() =>
                {
                    SelectedImage = _selectedImageInList;
                    SelectedImageLowRes = null;
                    OnPropertyChanged(nameof(SelectedImageLowRes));
                }, 150);

                _selectedImageLowResThrottledActionInvoker.ScheduleAction(() =>
                {
                    SelectedImageLowRes = _selectedImageInList;
                    OnPropertyChanged(nameof(SelectedImageLowRes));
                    _nullSet = false;
                    SetNavigationButtonsVisibility();

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

        public ICommand DeleteImageCommand { get; }

        public ICommand SaveCommand { get; }

        public int Rotation
        {
            get
            {
                return _rotation;
            }
            set
            {
                _rotation = value;
                if (Rotation % 360 != 0)
                {
                    SaveVisible = true;
                }
                else
                {
                    SaveVisible = false;
                }
                OnPropertyChanged();
            }
        }

        public bool SaveVisible
        {
            get
            {
                return _saveVisible;
            }
            set
            {
                _saveVisible = value;
                OnPropertyChanged();
            }
        }

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

        public bool SavingImageMessageVisible
        {
            get
            {
                return _savingImageMessageVisible;
            }
            set
            {
                _savingImageMessageVisible = value;
                OnPropertyChanged();
            }
        }

        private void DeleteSelectedImage()
        {
            if (!_userSettings.ShowDeleteConfirmation.Value || (_userSettings.ShowDeleteConfirmation.Value && _dialogHelper.ShowDialog("Do you want to delete?", Path.GetFileName(SelectedImage.Path))))
            {
                var selectedImage = SelectedImage;
                if (Images.Count > 1)
                {
                    if (selectedImage == Images.Last())
                    {
                        SelectedImage = Images.Skip(Images.Count - 2).First();
                    }
                    else
                    {
                        SelectedImage = Images.SkipWhile(it => it != SelectedImage).Skip(1).First();
                    }

                    _selectedImageInList = SelectedImage;
                    OnPropertyChanged(nameof(SelectedImageInList));
                }

                if (DeleteImage(selectedImage.Path))
                {
                    Images.Remove(selectedImage);
                }
                else
                {
                    // revert selection
                    SelectedImage = selectedImage;
                    _selectedImageInList = SelectedImage;
                    OnPropertyChanged(nameof(SelectedImageInList));
                }
            }
        }

        private async void SaveSelectedImage()
        {
            if (SaveVisible)
            {
                SavingImageMessageVisible = true;

                if (await _imageHelper.SaveRotatedImage(SelectedImage.Path, Rotation))
                {
                    SaveVisible = false;
                }

                GalleryCacheProvider.ResetThumbnail(SelectedImage.Path);
                SelectedImage.Refresh();
                SavingImageMessageVisible = false;
            }
        }

        private bool DeleteImage(string imagePath)
        {
            if (_userSettings.BackupDeletedImages.Value)
            {
                var backupFolder = Path.Combine(Path.GetDirectoryName(imagePath), "Backup");
            
                try
                {
                    Directory.CreateDirectory(backupFolder);
                    File.Move(imagePath, Path.Combine(backupFolder, Path.GetFileName(imagePath)));
                }
                catch(Exception ex)
                {
                    Logger.LogError("Failed to move image " + imagePath + " into " + backupFolder, ex);
                    return false;
                }
            }
            else
            {
                try
                {
                    File.Delete(imagePath);
                }
                catch(Exception ex)
                {
                    Logger.LogError("Failed to delete image " + imagePath, ex);
                    return false;
                }
            }
            return true;
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
                    // something else then image was selected
                    if(SelectedImage == null)
                    {
                        SelectedImage = Images.First();
                    }
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
            if (Images.Count > 0)
            {
                NextImageButtonVisible = SelectedImageLowRes != Images.Last();
                PreviousImageButtonVisible = SelectedImageLowRes != Images.First();
            }
            else
            {
                NextImageButtonVisible = false;
                PreviousImageButtonVisible = false;
                NoImagesMessageVisible = true;
                SaveVisible = false;
            }
        }
    }
}
