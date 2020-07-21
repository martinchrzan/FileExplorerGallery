using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows.Media.Animation;

namespace FileExplorerGallery.Behaviors
{
    public class ImageGalleryThumbnailAppearBehavior : Behavior<Border>
    {
        private int InitialDelayInMs = 600;
        private bool _loaded;
        private DependencyObject _container;

        protected override void OnAttached()
        {
            AssociatedObject.Loaded += AssociatedObject_Loaded;
            AssociatedObject.Unloaded += AssociatedObject_Unloaded;
        }

        private void AssociatedObject_Unloaded(object sender, RoutedEventArgs e)
        {
            OnDetaching();
            AssociatedObject.Loaded -= AssociatedObject_Loaded;
            AssociatedObject.Unloaded -= AssociatedObject_Unloaded;
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_loaded)
            {
                var parent = GalleryCacheProvider.GetGalleryItemsControl(AssociatedObject);

                var scrollViewer = GalleryCacheProvider.GetGalleryScrollViewer(AssociatedObject);

                Point relativePoint = AssociatedObject.TransformToAncestor(parent)
                              .Transform(new Point(0, 0));

                _container = parent.ItemContainerGenerator.ContainerFromItem(AssociatedObject.DataContext);

                var index = parent.ItemContainerGenerator.IndexFromContainer(_container);

                //no need to animate non visible items
                if (relativePoint.X < parent.ActualWidth && scrollViewer.HorizontalOffset == 0)
                {
                    AssociatedObject.Padding = new Thickness(0, 50, 0, 0);

                    DoubleAnimation opacity = new DoubleAnimation(0.9, new Duration(TimeSpan.FromMilliseconds(400)));
                    opacity.BeginTime = new TimeSpan(0, 0, 0, 0, 100 * index + InitialDelayInMs);
                    opacity.EasingFunction = new QuarticEase();
                    AssociatedObject.BeginAnimation(Border.OpacityProperty, opacity);

                    ThicknessAnimation moveUp = new ThicknessAnimation(new Thickness(0), new Duration(TimeSpan.FromMilliseconds(400)));
                    moveUp.BeginTime = new TimeSpan(0, 0, 0, 0, 100 * index + InitialDelayInMs);
                    moveUp.EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut };
                    AssociatedObject.BeginAnimation(Border.PaddingProperty, moveUp);
                }
                else
                {
                    AssociatedObject.Opacity = 0.9;
                    AssociatedObject.Margin = new Thickness(0);
                }

                _loaded = true;
            }
        }
    }
}
