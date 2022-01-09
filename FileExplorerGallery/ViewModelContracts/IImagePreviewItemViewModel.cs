using System.Windows.Input;

namespace FileExplorerGallery.ViewModelContracts
{
    public interface IImagePreviewItemViewModel
    {
        string Path { get; }

        ICommand SelectCommand { get; }

        bool NeedRefresh { get; set; }

        void Refresh();
    }
}
