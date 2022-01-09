using FileExplorerGallery.Common;
using FileExplorerGallery.ViewModelContracts;
using System;
using System.Windows.Input;

namespace FileExplorerGallery.ViewModels
{
    public class ImagePreviewItemViewModel : ViewModelBase, IImagePreviewItemViewModel
    {
        private string _path;

        public ImagePreviewItemViewModel(string imagePath, Action<IImagePreviewItemViewModel> selectCommandAction)
        {
            SelectCommand = new RelayCommand((obj) =>
            {
                selectCommandAction(this);
            });
            Path = imagePath;
        }

        public string Path
        {
            get
            {
                return _path;
            }
            set
            {
                _path = value;
                OnPropertyChanged();
            }
        }

        public bool NeedRefresh { get; set; }

        public ICommand SelectCommand { get; }

        public void Refresh()
        {
            NeedRefresh = true;
            OnPropertyChanged("Path");
        }
    }
}
