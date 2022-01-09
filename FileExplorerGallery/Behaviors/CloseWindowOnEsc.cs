using System.Windows;
using System.Windows.Interactivity;

namespace FileExplorerGallery.Behaviors
{
    public class CloseWindowOnEsc : Behavior<Window>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += AssociatedObject_Loaded;
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            AssociatedObject.KeyDown += AssociatedObject_KeyDown;
        }

        private void AssociatedObject_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                AssociatedObject.Loaded -= AssociatedObject_Loaded;
                AssociatedObject.KeyDown -= AssociatedObject_KeyDown;
                AssociatedObject.Close();
            }
        }
    }
}
