using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FileExplorerGallery.Behaviors
{
    // provider of main items control where file/folder items are located - this is always the same instance, caching it 
    public static class ItemsControlParentProvider
    {
        private static ItemsControl MainItemsControl;
        public static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            //get parent item
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            //we've reached the end of the tree
            if (parentObject == null) return null;

            //check if the parent matches the type we're looking for
            T parent = parentObject as T;
            if (parent != null)
                return parent;
            else
                return FindParent<T>(parentObject);
        }


        public static ItemsControl GetMainItemsControl(FrameworkElement childItem)
        {
            if (MainItemsControl == null)
            {
                MainItemsControl = FindParent<ItemsControl>(childItem);
            }
            return MainItemsControl;
        }
    }
}
