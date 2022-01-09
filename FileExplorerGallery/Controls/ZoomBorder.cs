using FileExplorerGallery.Helpers;
using FileExplorerGallery.ViewModelContracts;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace FileExplorerGallery.Controls
{
    public class ZoomBorder : Border
    {
        private const int ZoomAndTranslateAnimationDurationInMs = 200;
        private const int QuickAnimationDurationInMs = 10;
        private const double ZoomFactor = 1.3;

        private UIElement _child = null;
        private Point _origin;
        private Point _start;

        private bool _translateAvailable = true;
        private DoubleAnimation _animationX;
       
        public Button RotateButton
        {
            get { return (Button)GetValue(RotateButtonProperty); }
            set { SetValue(RotateButtonProperty, value); }
        }

        public static readonly DependencyProperty RotateButtonProperty = DependencyProperty.Register(
            "RotateButton", typeof(Button), typeof(ZoomBorder));

        public IImagePreviewItemViewModel PreviewItemViewModel
        {
            get { return (IImagePreviewItemViewModel)GetValue(PreviewItemProperty); }
            set { SetValue(PreviewItemProperty, value); }
        }

        public static readonly DependencyProperty PreviewItemProperty = DependencyProperty.Register(
            "PreviewItemViewModel", typeof(IImagePreviewItemViewModel), typeof(ZoomBorder), new PropertyMetadata(PropertyChangedCallback));


        public int Rotation
        {
            get { return (int)GetValue(RotationProperty); }
            set { SetValue(RotationProperty, value); } 
        }

        public static readonly DependencyProperty RotationProperty = DependencyProperty.Register("Rotation", typeof(int), typeof(ZoomBorder));

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var caller = (ZoomBorder)d;

            //reset any rotation we might have
            Application.Current.MainWindow.Dispatcher.Invoke(() => { Reset(true, caller); });
        }

        private static TranslateTransform GetTranslateTransform(UIElement element)
        {
            return (TranslateTransform)((TransformGroup)element.RenderTransform)
              .Children.First(tr => tr is TranslateTransform);
        }

        private static ScaleTransform GetScaleTransform(UIElement element)
        {
            return (ScaleTransform)((TransformGroup)element.RenderTransform)
              .Children.First(tr => tr is ScaleTransform);
        }

        private static RotateTransform GetRotateTransform(UIElement element)
        {
            return (RotateTransform)((TransformGroup)element.RenderTransform)
                 .Children.FirstOrDefault(tr => tr is RotateTransform);
        }

        public override UIElement Child
        {
            get { return base.Child; }
            set
            {
                if (value != null && value != Child)
                    Initialize(value);
                var grid = new Grid();
                grid.Children.Add(value);

                base.Child = grid;
            }
        }

        public void Initialize(UIElement element)
        {
            _child = element;
            if (_child != null)
            {
                TransformGroup group = new TransformGroup();
                ScaleTransform st = new ScaleTransform();
                TranslateTransform tt = new TranslateTransform();
                RotateTransform rt = new RotateTransform();

                group.Children.Add(st);
                group.Children.Add(tt);
                group.Children.Add(rt);
                _child.RenderTransform = group;
                _child.RenderTransformOrigin = new Point(0.5, 0.5);
                MouseWheel += Child_MouseWheel;
                MouseLeftButtonDown += Child_MouseLeftButtonDown;
                MouseLeftButtonUp += Child_MouseLeftButtonUp;
                MouseMove += Child_MouseMove;
                PreviewMouseRightButtonDown += Child_PreviewMouseRightButtonDown;
            }
            Unloaded += ZoomBorder_Unloaded;

            RotateButton.Click += (s, e) => Rotate();
        }

        private void ZoomBorder_Unloaded(object sender, RoutedEventArgs e)
        {
            Unloaded -= ZoomBorder_Unloaded;
            MouseWheel -= Child_MouseWheel;
            MouseLeftButtonDown -= Child_MouseLeftButtonDown;
            MouseLeftButtonUp -= Child_MouseLeftButtonUp;
            MouseMove -= Child_MouseMove;
            PreviewMouseRightButtonDown -= Child_PreviewMouseRightButtonDown;
        }

        private void Rotate()
        {
            if (_child != null)
            {
                var rt = GetRotateTransform(_child);
                var to = Math.Round(rt.Angle);
                to -= 90;
                Rotation = (int)to;
                AnimateRotateTransform(rt, to, 400);
            }
        }

        public static void Reset(bool quickReset, ZoomBorder instance)
        {
            if (instance._child != null)
            {
                // reset zoom
                var st = GetScaleTransform(instance._child);
                AnimateScaleTransform(st, 1.0, ZoomAndTranslateAnimationDurationInMs);

                // reset pan
                var tt = GetTranslateTransform(instance._child);
                AnimateTranslateTransform(tt, 0.0, 0.0, quickReset ? QuickAnimationDurationInMs : ZoomAndTranslateAnimationDurationInMs, instance);

                var rt = GetRotateTransform(instance._child);
                AnimateRotateTransform(rt, 0, QuickAnimationDurationInMs);
            }
        }

        private void Child_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (_child != null)
            {
                var st = GetScaleTransform(_child);
                st.CenterX = -(_child.RenderSize.Width / 2);
                st.CenterY = -(_child.RenderSize.Height / 2);

                var tt = GetTranslateTransform(_child);

                bool incrementZoom = e.Delta > 0;

                if (!(e.Delta > 0) && (st.ScaleX < .4 || st.ScaleY < .4))
                    return;

                Point relative = e.GetPosition(_child);
                double abosuluteX;
                double abosuluteY;

                abosuluteX = relative.X * st.ScaleX + tt.X;
                abosuluteY = relative.Y * st.ScaleY + tt.Y;

                var newScale = incrementZoom ? st.ScaleX * ZoomFactor : st.ScaleX / ZoomFactor;
                AnimateScaleTransform(st, newScale, ZoomAndTranslateAnimationDurationInMs);
                AnimateTranslateTransform(tt, abosuluteX - relative.X * newScale, abosuluteY - relative.Y * newScale, ZoomAndTranslateAnimationDurationInMs, this);
            }
        }

        private static void AnimateRotateTransform(RotateTransform rt, double to, int durationInMs)
        {
            var animation = new DoubleAnimation(to, TimeSpan.FromMilliseconds(durationInMs))
            {
                EasingFunction = new QuarticEase() { EasingMode = EasingMode.EaseOut }
            };
            rt.BeginAnimation(RotateTransform.AngleProperty, animation);
        }

        private static void AnimateScaleTransform(ScaleTransform st, double to, int durationInMs, bool stop = false)
        {
            var animation = new DoubleAnimation(to, TimeSpan.FromMilliseconds(durationInMs))
            {
                EasingFunction = new QuarticEase() { EasingMode = EasingMode.EaseOut },
                FillBehavior = stop ? FillBehavior.Stop : FillBehavior.HoldEnd
            };
            st.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
            st.BeginAnimation(ScaleTransform.ScaleYProperty, animation);
        }

        private static void AnimateTranslateTransform(TranslateTransform tt, double toX, double toY, int durationInMs, ZoomBorder instance)
        {
            if (instance._translateAvailable == true)
            {
                instance._translateAvailable = false;
                instance._animationX = new DoubleAnimation(toX, TimeSpan.FromMilliseconds(durationInMs))
                {
                    EasingFunction = new QuarticEase() { EasingMode = EasingMode.EaseOut }
                };

                var animationY = new DoubleAnimation(toY, TimeSpan.FromMilliseconds(durationInMs))
                {
                    EasingFunction = new QuarticEase() { EasingMode = EasingMode.EaseOut }
                };

                instance._animationX.Completed += instance._animationX_Completed;

                tt.BeginAnimation(TranslateTransform.XProperty, instance._animationX, HandoffBehavior.Compose);

                tt.BeginAnimation(TranslateTransform.YProperty, animationY, HandoffBehavior.Compose);
            }
        }

        private void _animationX_Completed(object sender, EventArgs e)
        {
            _animationX.Completed -= _animationX_Completed;
            _translateAvailable = true;
        }

        private void Child_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_child != null)
            {
                if (e.ClickCount == 1)
                {
                    var tt = GetTranslateTransform(_child);
                    _start = e.GetPosition(this);
                    _origin = new Point(tt.X, tt.Y);
                    // Cursor = Cursors.Hand;
                    _child.CaptureMouse();
                }
                else
                {
                    Point relative = e.GetPosition(_child);
                    ZoomIn(relative.X, relative.Y, 2);
                }
            }
        }

        private void ZoomIn(double relativeX, double relativeY, double zoomScale)
        {
            var st = GetScaleTransform(_child);
            st.CenterX = -(_child.RenderSize.Width / 2);
            st.CenterY = -(_child.RenderSize.Height / 2);

            var tt = GetTranslateTransform(_child);

            double abosuluteX;
            double abosuluteY;

            abosuluteX = relativeX * st.ScaleX + tt.X;
            abosuluteY = relativeY * st.ScaleY + tt.Y;

            var newScale = st.ScaleX * zoomScale;

            AnimateScaleTransform(st, newScale, ZoomAndTranslateAnimationDurationInMs);
            AnimateTranslateTransform(tt, abosuluteX - relativeX * newScale, abosuluteY - relativeY * newScale, ZoomAndTranslateAnimationDurationInMs, this);
        }

        private void Child_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_child != null)
            {
                _child.ReleaseMouseCapture();
                Cursor = Cursors.Arrow;
            }
        }

        private void Child_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Reset(false, this);
        }

        private void Child_MouseMove(object sender, MouseEventArgs e)
        {
            if (_child != null)
            {
                if (_child.IsMouseCaptured)
                {
                    var tt = GetTranslateTransform(_child);
                    var rt = GetRotateTransform(_child);

                    Vector v = _start - e.GetPosition(this);

                    var result = Rotate(v, -rt.Angle);

                    AnimateTranslateTransform(tt, _origin.X - result.X, _origin.Y - result.Y, QuickAnimationDurationInMs, this);
                }
            }
        }

        public static Vector Rotate(Vector v, double degrees)
        {
            var radians = degrees * Math.PI / 180;
            var ca = Math.Cos(radians);
            var sa = Math.Sin(radians);
            return new Vector(ca * v.X - sa * v.Y, sa * v.X + ca * v.Y);
        }
    }
}
