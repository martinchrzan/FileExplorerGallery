using FileExplorerGallery.ViewModelContracts;
using FileExplorerGallery.ViewModels;
using System;
using System.ComponentModel.Composition;

namespace FileExplorerGallery.VIewModelFactories
{
    [Export(typeof(IImagePreviewItemViewModelFactory))]
    public class ImagePreviewItemViewModelFactory : IImagePreviewItemViewModelFactory
    {
        public IImagePreviewItemViewModel CreateImagePreviewItemViewModel(string imagePath, Action<IImagePreviewItemViewModel> selectCommandAction)
        {
            return new ImagePreviewItemViewModel(imagePath, selectCommandAction);
        }
    }
}
