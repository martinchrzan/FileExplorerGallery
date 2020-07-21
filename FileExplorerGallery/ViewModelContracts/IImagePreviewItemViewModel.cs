using System.Windows.Input;

namespace FileExplorerGallery.ViewModelContracts
{
    public interface IImagePreviewItemViewModel
    {
        string Path { get; }

        ICommand SelectCommand { get; }
    }
}
