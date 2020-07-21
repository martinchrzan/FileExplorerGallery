using FileExplorerGallery.ViewModelContracts;
using System;

namespace FileExplorerGallery.VIewModelFactories
{
    public interface IImagePreviewItemViewModelFactory
    {
        IImagePreviewItemViewModel CreateImagePreviewItemViewModel(string imagePath, Action<IImagePreviewItemViewModel> selectCommandAction);
    }
}
