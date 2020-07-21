using FileExplorerGallery.Behaviors;
using FileExplorerGallery.Keyboard;
using FileExplorerGallery.ViewModelContracts;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FileExplorerGallery
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(IImagePreviewViewModel mainViewModel)
        {
            MainViewModel = mainViewModel;
            Closing += MainWindow_Closing;
            Loaded += MainWindow_Loaded;
            InitializeComponent();
            DataContext = this;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Maximized;
        }

        public IImagePreviewViewModel MainViewModel { get; set; }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {   
            MainViewModel.Dispose();
            Application.Current.MainWindow = null;
            GalleryCacheProvider.ClearCache();
        }
    }
}
