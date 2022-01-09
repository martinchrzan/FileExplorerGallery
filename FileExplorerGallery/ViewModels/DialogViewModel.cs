using FileExplorerGallery.Common;
using FileExplorerGallery.ViewModelContracts;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FileExplorerGallery.ViewModels
{
    [Export(typeof(IDialogViewModel))]
    internal class DialogViewModel : IDialogViewModel
    {
        public DialogViewModel()
        {
            YesCommand = new RelayCommand(() =>
            {
                Window.DialogResult = true;
                Window.Close();
            });

            NoCommand = new RelayCommand(() =>
            {
                Window.DialogResult = false;
                Window.Close();
            });
        }

        public string Title { get; set; }

        public string Text { get; set; }

        public ICommand YesCommand { get; private set; }

        public ICommand NoCommand { get; private set; }

        public Window Window { get; set; }
    }
}
