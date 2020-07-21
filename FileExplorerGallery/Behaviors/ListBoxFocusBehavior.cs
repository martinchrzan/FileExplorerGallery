using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace FileExplorerGallery.Behaviors
{
    public class ListBoxFocusBehavior : Behavior<ListBox>
    {
        protected override void OnAttached()
        {
            AssociatedObject.Loaded += AssociatedObject_Loaded;
            AssociatedObject.Unloaded += AssociatedObject_Unloaded;
            
            AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;
        }

        private void AssociatedObject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FocusSelectedItem();
        }

        private void AssociatedObject_Unloaded(object sender, RoutedEventArgs e)
        {
            OnDetaching();
            AssociatedObject.Loaded -= AssociatedObject_Loaded;
            AssociatedObject.Unloaded -= AssociatedObject_Unloaded;
            AssociatedObject.SelectionChanged -= AssociatedObject_SelectionChanged;
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            FocusSelectedItem();
            AssociatedObject.Focus();
        }

        private void FocusSelectedItem()
        {
            AssociatedObject.ScrollIntoView(AssociatedObject.SelectedItem);
            var listBoxItem = (ListBoxItem)AssociatedObject.ItemContainerGenerator.ContainerFromItem(AssociatedObject.SelectedItem);
            if (listBoxItem != null)
            {
                listBoxItem.Focus();
            }
        }
    }
}
