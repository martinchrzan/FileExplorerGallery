using FileExplorerGallery.ViewModelContracts;
using System.ComponentModel.Composition;
using System.Windows;

namespace FileExplorerGallery.Helpers
{
    [Export(typeof(AppStateHandler))]
    public class AppStateHandler
    {
        private readonly ApplicationDispatcher _applicationDispatcher;

        [ImportingConstructor]
        public AppStateHandler( ApplicationDispatcher applicationDispatcher)
        {
            _applicationDispatcher = applicationDispatcher;
        }

        public void ShowGallery(string folderPath, string selectedItem, Win32Apis.Rect fileExplorerLocation)
        {
            _applicationDispatcher.Invoke(() =>
            {
                // get new instance
                var imagePreviewViewModel = Bootstrapper.Container.GetExportedValue<IImagePreviewViewModel>();

                Application.Current.MainWindow = new MainWindow(imagePreviewViewModel) { Left = fileExplorerLocation.left, Top = fileExplorerLocation.top };
                imagePreviewViewModel.Initialize(folderPath, selectedItem);
                Application.Current.MainWindow.Show();
            });
        }

        public void HideGallery()
        {
            _applicationDispatcher.Invoke(() =>
            {
                Application.Current.MainWindow.Close();
            });
        }

        public bool IsGalleryVisible()
        {
            var isVisible = true;
            _applicationDispatcher.Invoke(() => { isVisible = Application.Current.MainWindow != null; });
            return isVisible;
        }

        public void SetTopMost()
        {
            _applicationDispatcher.Invoke(() =>
            {
                Application.Current.MainWindow.Topmost = false;
                Application.Current.MainWindow.Topmost = true;
            });
        }
    }
}
