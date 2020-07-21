using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace FileExplorerGallery.Behaviors
{
    public class GalleryCacheBehavior : Behavior<ListBox>
    {
        protected override void OnAttached()
        {
            AssociatedObject.Unloaded += AssociatedObject_Unloaded;
        }

        private void AssociatedObject_Unloaded(object sender, RoutedEventArgs e)
        {
            AssociatedObject.Unloaded -= AssociatedObject_Unloaded;
            // to avoid reusing old cached elements which are being destroyed
            GalleryCacheProvider.ClearCache();
            Detach();
        }
    }
}
