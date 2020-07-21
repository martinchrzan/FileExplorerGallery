using FileExplorerGallery.Common;
using FileExplorerGallery.ViewModelContracts;
using System;
using System.ComponentModel.Composition;
using System.Windows.Input;

namespace FileExplorerGallery.ViewModels
{
    public class ImagePreviewItemViewModel : IImagePreviewItemViewModel
    {
        public ImagePreviewItemViewModel(string imagePath, Action<IImagePreviewItemViewModel> selectCommandAction)
        {
            SelectCommand = new RelayCommand((obj) =>
            {
                selectCommandAction(this);
            });
            Path = imagePath;
        }

        public string Path { get; }

        public ICommand SelectCommand { get; }
    }
}
