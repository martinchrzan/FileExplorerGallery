using FileExplorerGallery.Animations;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows.Media;

namespace FileExplorerGallery.Behaviors
{
    public class GallerySlideshowBehavior : Behavior<Grid>
    {
        public Grid MainGrid
        {
            get { return (Grid)GetValue(MainGridProperty); }
            set { SetValue(MainGridProperty, value); }
        }

        public static readonly DependencyProperty MainGridProperty = DependencyProperty.Register(
            "MainGrid", typeof(Grid), typeof(GallerySlideshowBehavior));

        public Button RotateButton
        {
            get { return (Button)GetValue(RotateButtonProperty); }
            set { SetValue(RotateButtonProperty, value); }
        }

        public static readonly DependencyProperty RotateButtonProperty = DependencyProperty.Register(
            "RotateButton", typeof(Button), typeof(GallerySlideshowBehavior));

        public Button SlideshowButton
        {
            get { return (Button)GetValue(SlideshowButtonProperty); }
            set { SetValue(SlideshowButtonProperty, value); }
        }

        public static readonly DependencyProperty SlideshowButtonProperty = DependencyProperty.Register(
            "SlideshowButton", typeof(Button), typeof(GallerySlideshowBehavior));

        public Button DeleteButton
        {
            get { return (Button)GetValue(DeleteButtonProperty); }
            set { SetValue(DeleteButtonProperty, value); }
        }

        public static readonly DependencyProperty DeleteButtonProperty = DependencyProperty.Register(
            "DeleteButton", typeof(Button), typeof(GallerySlideshowBehavior));

        public Button SaveButton
        {
            get { return (Button)GetValue(SaveButtonProperty); }
            set { SetValue(SaveButtonProperty, value); }
        }

        public static readonly DependencyProperty SaveButtonProperty = DependencyProperty.Register(
            "SaveButton", typeof(Button), typeof(GallerySlideshowBehavior));

        protected override void OnAttached()
        {
            AssociatedObject.Loaded += AssociatedObject_Loaded;
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            SlideshowButton.Click += (s, e1) =>
            {
                var brushAnimation = new BrushAnimation();
                brushAnimation.To = Brushes.Black;
                brushAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(800));

                GridLengthAnimation heightAnimation = new GridLengthAnimation();
                heightAnimation.To = new GridLength(0);
                heightAnimation.From = new GridLength(120.0);
                heightAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(500));

                GridLengthAnimation heightAnimation2 = new GridLengthAnimation();
                heightAnimation2.To = new GridLength(0);
                heightAnimation2.From = new GridLength(35.0);
                heightAnimation2.Duration = new Duration(TimeSpan.FromMilliseconds(500));

                heightAnimation.DecelerationRatio = 0.5;
                heightAnimation2.DecelerationRatio = 0.5;

                MainGrid.RowDefinitions[MainGrid.RowDefinitions.Count - 1].BeginAnimation(RowDefinition.HeightProperty, heightAnimation);
                MainGrid.RowDefinitions[0].BeginAnimation(RowDefinition.HeightProperty, heightAnimation2);

                MainGrid.BeginAnimation(Grid.BackgroundProperty, brushAnimation);

                RotateButton.Visibility = Visibility.Collapsed;
                SaveButton.Visibility = Visibility.Collapsed;
                DeleteButton.Visibility = Visibility.Collapsed;
                SlideshowButton.Visibility = Visibility.Collapsed;
            };
        }
    }
}
