using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FileExplorerGallery.ViewModelContracts
{
    public interface IImagePreviewViewModel
    {
        void Initialize(string directoryPath, string selectedItem);

        void Dispose();

        ICommand CloseCommand { get; }

        ICommand SlideshowCommand { get; }

        ICommand PreviousImageCommand { get; }

        ICommand NextImageCommand { get; }

        bool PreviousImageButtonVisible { get; }

        bool NextImageButtonVisible { get; }

        ObservableCollection<IImagePreviewItemViewModel> Images { get; }

        //converter on view will load full res image from path
        IImagePreviewItemViewModel SelectedImage { get; set; }

        //converter on view model will load low res image from path
        IImagePreviewItemViewModel SelectedImageLowRes { get; set; }

        IImagePreviewItemViewModel SelectedImageInList { get; set; }

        bool NoImagesMessageVisible { get; set; }


    }
}
