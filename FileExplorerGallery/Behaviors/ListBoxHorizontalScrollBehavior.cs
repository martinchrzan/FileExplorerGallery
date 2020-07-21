using FileExplorerGallery.Extensions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace FileExplorerGallery.Behaviors
{
    public class ListBoxHorizontalScrollBehavior : Behavior<ListBox>
    {
        private ScrollViewer _scrollViewer;
        protected override void OnAttached()
        {
            AssociatedObject.Loaded += AssociatedObject_Loaded;
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            _scrollViewer = AssociatedObject.GetChildOfType<ScrollViewer>();
            if(_scrollViewer != null)
            {
                _scrollViewer.PreviewMouseWheel += ScrollViewer_PreviewMouseWheel;
            }
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (e.Delta < 0)
            {
                _scrollViewer.LineRight();
            }
            else
            {
                _scrollViewer.LineLeft();
            }
        }
    }
}
