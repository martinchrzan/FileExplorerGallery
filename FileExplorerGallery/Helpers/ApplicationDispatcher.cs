using System;
using System.ComponentModel.Composition;
using System.Windows;

namespace FileExplorerGallery.Helpers
{
    [Export(typeof(ApplicationDispatcher))]
    public class ApplicationDispatcher
    {
        [ImportingConstructor]
        public ApplicationDispatcher()
        {
        }

        public void Invoke(Action action)
        {
            Application.Current.Dispatcher.Invoke(action);
        }
    }
}
