using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace FileExplorerGallery.Animations
{
    public class GridLengthAnimation : AnimationTimeline
    {
        public override Type TargetPropertyType => typeof(GridLength);

        protected override Freezable CreateInstanceCore()
        {
            return new GridLengthAnimation();
        }

        public static readonly DependencyProperty FromProperty =
            DependencyProperty.Register("From", typeof(GridLength), typeof(GridLengthAnimation));

        public GridLength From
        {
            get { return (GridLength)GetValue(FromProperty); }
            set { SetValue(FromProperty, value); }
        }

        public static readonly DependencyProperty ToProperty =
            DependencyProperty.Register("To", typeof(GridLength), typeof(GridLengthAnimation));

        public GridLength To
        {
            get { return (GridLength)GetValue(ToProperty); }
            set { SetValue(ToProperty, value); }
        }

        public override object GetCurrentValue(object defaultOriginValue, object defaultDestinationValue, AnimationClock clock)
        {
            double fromValue = From.Value;
            double toValue = To.Value;

            if (fromValue > toValue)
            {
                return new GridLength((1 - clock.CurrentProgress.Value) * (fromValue - toValue) + toValue);
            }
            else
            {
                return new GridLength(clock.CurrentProgress.Value *
                    (toValue - fromValue) + fromValue);
            }
        }
    }
}
