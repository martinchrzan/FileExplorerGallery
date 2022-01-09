using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace FileExplorerGallery.Behaviors
{
    public class FocusFirstButtonBehavior : Behavior<Window>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += AssociatedObject_Loaded;
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            AssociatedObject.GetChildOfType<Button>().Focus();
        }      
    }
}
