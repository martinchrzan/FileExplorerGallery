using System.Windows;
using System.Windows.Input;

namespace FileExplorerGallery.ViewModelContracts
{
    public interface IDialogViewModel
    {
        string Title { get; set; }

        string Text { get; set; }

        ICommand YesCommand { get; }

        ICommand NoCommand { get; }

        Window Window { get; set; }
    }
}
