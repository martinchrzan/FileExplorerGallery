using FileExplorerGallery.ViewModelContracts;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FileExplorerGallery.Helpers
{
    [Export(typeof(DialogHelper))]
    public class DialogHelper
    {
        private readonly IDialogViewModel _dialogViewModel;

        [ImportingConstructor]
        public DialogHelper(IDialogViewModel dialogViewModel)
        {
            _dialogViewModel = dialogViewModel;
        }
        public bool ShowDialog(string title, string text)
        {
            var window = new DialogWindow();
            _dialogViewModel.Window = window;
            _dialogViewModel.Text = text;
            _dialogViewModel.Title = title;

            window.Content = _dialogViewModel;

            return (bool)window.ShowDialog();
        }
    }
}
