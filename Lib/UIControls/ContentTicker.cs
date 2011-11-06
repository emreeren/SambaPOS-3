using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace UIControls
{
    public enum TickerDirection
    {
        East,
        West
    }

    public class ContentTicker : ContentControl
    {
        Storyboard _ContentTickerStoryboard = null;
        Canvas _ContentControl = null;
        ContentPresenter _Content = null;

        static ContentTicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ContentTicker), new FrameworkPropertyMetadata(typeof(ContentTicker)));
        }

        public ContentTicker()
        {
            this.Loaded += new RoutedEventHandler(ContentTicker_Loaded);
        }

        public void Start()
        {
            if (_ContentTickerStoryboard != null &&
                !IsStarted)
            {
                UpdateAnimationDetails(_ContentControl.ActualWidth, _Content.ActualWidth);

                _ContentTickerStoryboard.Begin(_ContentControl, true);
                IsStarted = true;
            }
        }

        public void Pause()
        {
            if (IsStarted &&
                !IsPaused &&
                _ContentTickerStoryboard != null)
            {
                _ContentTickerStoryboard.Pause(_ContentControl);
                IsPaused = true;
            }
        }

        public void Resume()
        {
            if (IsPaused &&
                _ContentTickerStoryboard != null)
            {
                _ContentTickerStoryboard.Resume(_ContentControl);
                IsPaused = false;
            }
        }

        public void Stop()
        {
            if (_ContentTickerStoryboard != null &&
                IsStarted)
            {
                _ContentTickerStoryboard.Stop(_ContentControl);
                IsStarted = false;
            }
        }

        public bool IsStarted { get; private set; }
        public bool IsPaused { get; private set; }

        public double Rate
        {
            get { return (double)GetValue(RateProperty); }
            set { SetValue(RateProperty, value); }
        }

        public static readonly DependencyProperty RateProperty =
            DependencyProperty.Register("Rate", typeof(double), typeof(ContentTicker), new UIPropertyMetadata(60.0));

        public TickerDirection Direction
        {
            get { return (TickerDirection)GetValue(DirectionProperty); }
            set { SetValue(DirectionProperty, value); }
        }

        public static readonly DependencyProperty DirectionProperty =
            DependencyProperty.Register("Direction", typeof(TickerDirection), typeof(ContentTicker), new UIPropertyMetadata(TickerDirection.West));

        void ContentTicker_Loaded(object sender, RoutedEventArgs e)
        {
            _ContentControl = GetTemplateChild("PART_ContentControl") as Canvas;
            if (_ContentControl != null)
                _ContentControl.SizeChanged += new SizeChangedEventHandler(_ContentControl_SizeChanged);
            _Content = GetTemplateChild("PART_Content") as ContentPresenter;
            if (_Content != null)
                _Content.SizeChanged += new SizeChangedEventHandler(_Content_SizeChanged);
            _ContentTickerStoryboard = GetTemplateChild("ContentTickerStoryboard") as Storyboard;

            if (_ContentControl.ActualWidth == 0 && double.IsNaN(_ContentControl.Width))
                _ContentControl.Width = _Content.ActualWidth;
            if (_ContentControl.ActualHeight == 0 && double.IsNaN(_ContentControl.Height))
                _ContentControl.Height = _Content.ActualHeight;

            VerticallyAlignContent(_ContentControl.ActualHeight);

            Start();
        }

        void _Content_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateAnimationDetails(_ContentControl.ActualWidth, e.NewSize.Width);
        }

        void _ContentControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            VerticallyAlignContent(e.NewSize.Height);
            UpdateAnimationDetails(e.NewSize.Width, _Content.ActualWidth);
        }

        void VerticallyAlignContent(double height)
        {
            double contentHeight = _Content.ActualHeight;
            switch (_Content.VerticalAlignment)
            {
                case System.Windows.VerticalAlignment.Top:
                    Canvas.SetTop(_Content, 0);
                    break;
                case System.Windows.VerticalAlignment.Bottom:
                    if (height > contentHeight)
                        Canvas.SetTop(_Content, height - contentHeight);
                    break;
                case System.Windows.VerticalAlignment.Center:
                case System.Windows.VerticalAlignment.Stretch:
                    if (height > contentHeight)
                        Canvas.SetTop(_Content, (height - contentHeight) / 2);
                    break;
            }
        }

        void UpdateAnimationDetails(double holderLength, double contentLength)
        {
            DoubleAnimation animation =
                _ContentTickerStoryboard.Children.First() as DoubleAnimation;
            if (animation != null)
            {
                bool start = false;
                if (IsStarted)
                {
                    Stop();
                    start = true;
                }

                double from = 0, to = 0, time = 0;
                switch (Direction)
                {
                    case TickerDirection.West:
                        from = holderLength;
                        to = -1 * contentLength;
                        time = from / Rate;
                        break;
                    case TickerDirection.East:
                        from = -1 * contentLength;
                        to = holderLength;
                        time = to / Rate;
                        break;
                }

                animation.From = from;
                animation.To = to;
                TimeSpan newDuration = TimeSpan.FromSeconds(time);
                animation.Duration = new Duration(newDuration);

                if (start)
                {
                    TimeSpan? oldDuration = null;
                    if (animation.Duration.HasTimeSpan)
                        oldDuration = animation.Duration.TimeSpan;
                    TimeSpan? currentTime = _ContentTickerStoryboard.GetCurrentTime(_ContentControl);
                    int? iteration = _ContentTickerStoryboard.GetCurrentIteration(_ContentControl);
                    TimeSpan? offset =
                        TimeSpan.FromSeconds(
                        currentTime.HasValue ?
                        currentTime.Value.TotalSeconds % (oldDuration.HasValue ? oldDuration.Value.TotalSeconds : 1.0) :
                        0.0);

                    Start();

                    if (offset.HasValue &&
                        offset.Value != TimeSpan.Zero &&
                        offset.Value < newDuration)
                        _ContentTickerStoryboard.SeekAlignedToLastTick(_ContentControl, offset.Value, TimeSeekOrigin.BeginTime);
                }
            }
        }
    }
}
