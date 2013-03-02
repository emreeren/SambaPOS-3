using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Samba.Presentation.Common;

namespace Samba.Presentation.Controls.UIControls
{
    public class KineticBehaviour
    {
        #region Friction

        /// <summary>
        /// Friction Attached Dependency Property
        /// </summary>
        public static readonly DependencyProperty FrictionProperty =
            DependencyProperty.RegisterAttached("Friction", typeof(double), typeof(KineticBehaviour),
                new FrameworkPropertyMetadata((double)0.90));

        /// <summary>
        /// Gets the Friction property.  This dependency property 
        /// indicates ....
        /// </summary>
        public static double GetFriction(DependencyObject d)
        {
            return (double)d.GetValue(FrictionProperty);
        }

        /// <summary>
        /// Sets the Friction property.  This dependency property 
        /// indicates ....
        /// </summary>
        public static void SetFriction(DependencyObject d, double value)
        {
            d.SetValue(FrictionProperty, value);
        }

        #endregion

        #region ScrollStartPoint

        /// <summary>
        /// ScrollStartPoint Attached Dependency Property
        /// </summary>
        private static readonly DependencyProperty ScrollStartPointProperty =
            DependencyProperty.RegisterAttached("ScrollStartPoint", typeof(Point), typeof(KineticBehaviour),
                new FrameworkPropertyMetadata((Point)new Point()));

        /// <summary>
        /// Gets the ScrollStartPoint property.  This dependency property 
        /// indicates ....
        /// </summary>
        private static Point GetScrollStartPoint(DependencyObject d)
        {
            return (Point)d.GetValue(ScrollStartPointProperty);
        }

        /// <summary>
        /// Sets the ScrollStartPoint property.  This dependency property 
        /// indicates ....
        /// </summary>
        private static void SetScrollStartPoint(DependencyObject d, Point value)
        {
            d.SetValue(ScrollStartPointProperty, value);
        }

        #endregion

        #region ScrollStartOffset

        /// <summary>
        /// ScrollStartOffset Attached Dependency Property
        /// </summary>
        private static readonly DependencyProperty ScrollStartOffsetProperty =
            DependencyProperty.RegisterAttached("ScrollStartOffset", typeof(Point), typeof(KineticBehaviour),
                new FrameworkPropertyMetadata((Point)new Point()));

        /// <summary>
        /// Gets the ScrollStartOffset property.  This dependency property 
        /// indicates ....
        /// </summary>
        private static Point GetScrollStartOffset(DependencyObject d)
        {
            return (Point)d.GetValue(ScrollStartOffsetProperty);
        }

        /// <summary>
        /// Sets the ScrollStartOffset property.  This dependency property 
        /// indicates ....
        /// </summary>
        private static void SetScrollStartOffset(DependencyObject d, Point value)
        {
            d.SetValue(ScrollStartOffsetProperty, value);
        }

        #endregion

        #region InertiaProcessor

        /// <summary>
        /// InertiaProcessor Attached Dependency Property
        /// </summary>
        private static readonly DependencyProperty InertiaProcessorProperty =
            DependencyProperty.RegisterAttached("InertiaProcessor", typeof(InertiaHandler), typeof(KineticBehaviour),
                new FrameworkPropertyMetadata((InertiaHandler)null));

        /// <summary>
        /// Gets the InertiaProcessor property.  This dependency property 
        /// indicates ....
        /// </summary>
        private static InertiaHandler GetInertiaProcessor(DependencyObject d)
        {
            return (InertiaHandler)d.GetValue(InertiaProcessorProperty);
        }

        /// <summary>
        /// Sets the InertiaProcessor property.  This dependency property 
        /// indicates ....
        /// </summary>
        private static void SetInertiaProcessor(DependencyObject d, InertiaHandler value)
        {
            d.SetValue(InertiaProcessorProperty, value);
        }

        #endregion

        #region HandleKineticScrolling

        /// <summary>
        /// HandleKineticScrolling Attached Dependency Property
        /// </summary>
        public static readonly DependencyProperty HandleKineticScrollingProperty =
            DependencyProperty.RegisterAttached("HandleKineticScrolling", typeof(bool),
            typeof(KineticBehaviour),
                new FrameworkPropertyMetadata((bool)false,
                    new PropertyChangedCallback(OnHandleKineticScrollingChanged)));

        /// <summary>
        /// Gets the HandleKineticScrolling property.  This dependency property 
        /// indicates ....
        /// </summary>
        public static bool GetHandleKineticScrolling(DependencyObject d)
        {
            return (bool)d.GetValue(HandleKineticScrollingProperty);
        }

        /// <summary>
        /// Sets the HandleKineticScrolling property.  This dependency property 
        /// indicates ....
        /// </summary>
        public static void SetHandleKineticScrolling(DependencyObject d, bool value)
        {
            d.SetValue(HandleKineticScrollingProperty, value);
        }

        /// <summary>
        /// Handles changes to the HandleKineticScrolling property.
        /// </summary>
        private static void OnHandleKineticScrollingChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            AttachScrollViewer(d, (bool)e.NewValue);
        }

        private static void AttachScrollViewer(DependencyObject d, bool newValue)
        {
            var scoller = d as ScrollViewer;
            if (scoller != null)
            {
                if (newValue)
                {
                    scoller.PreviewMouseLeftButtonDown += ScollerPreviewMouseLeftButtonDown;
                    scoller.PreviewMouseLeftButtonUp += ScollerPreviewMouseLeftButtonUp;
                    scoller.PreviewMouseMove += ScollerPreviewMouseMove;
                    SetInertiaProcessor(scoller, new InertiaHandler(scoller));
                }
                else
                {
                    scoller.PreviewMouseLeftButtonDown -= ScollerPreviewMouseLeftButtonDown;
                    scoller.PreviewMouseLeftButtonUp -= ScollerPreviewMouseLeftButtonUp;
                    scoller.PreviewMouseMove -= ScollerPreviewMouseMove;
                    var inertia = GetInertiaProcessor(scoller);
                    if (inertia != null)
                        inertia.Dispose();
                }
            }
            else
            {
                var sbar = ExtensionServices.GetVisualChild<ScrollViewer>(d);
                if (sbar != null)
                    AttachScrollViewer(sbar, newValue);
                else
                {
                    var visual = d as FrameworkElement;
                    if (visual != null)
                        visual.Loaded += VisualLoaded;
                }
            }
        }

        static void VisualLoaded(object sender, RoutedEventArgs e)
        {
            var fe = sender as FrameworkElement;
            if (fe != null)
            {
                var v = ExtensionServices.GetVisualChild<ScrollViewer>(fe);
                if (v != null)
                {
                    AttachScrollViewer(v, true);
                    fe.Loaded -= VisualLoaded;
                }
            }
        }

        #endregion

        #region Mouse Events

        private static bool InRage(double val)
        {
            if (val < -5 && val > -20) return true;
            if (val > 5 && val < 20) return true;
            return false;
        }

        private static bool _previewCapture;
        private static bool _scrollBarClicked;

        static void ScollerPreviewMouseMove(object sender, MouseEventArgs e)
        {
            var scrollViewer = (ScrollViewer)sender;

            if (_previewCapture && (scrollViewer.ScrollableHeight > 0 || scrollViewer.ScrollableWidth > 0))
            {
                var currentPoint = e.GetPosition(scrollViewer);
                if (currentPoint.X > scrollViewer.ViewportWidth) _scrollBarClicked = true;
                var scrollStartPoint = GetScrollStartPoint(scrollViewer);
                var delta = new Point(scrollStartPoint.X - currentPoint.X,
                    scrollStartPoint.Y - currentPoint.Y);
                if (!_scrollBarClicked && (InRage(delta.X) || InRage(delta.Y)))
                {
                    _previewCapture = false;
                    scrollViewer.CaptureMouse();
                }
            }

            if (scrollViewer.IsMouseCaptured)
            {
                var currentPoint = e.GetPosition(scrollViewer);

                var scrollStartPoint = GetScrollStartPoint(scrollViewer);
                // Determine the new amount to scroll.
                var delta = new Point(scrollStartPoint.X - currentPoint.X,
                    scrollStartPoint.Y - currentPoint.Y);

                var scrollStartOffset = GetScrollStartOffset(scrollViewer);
                var scrollTarget = new Point(scrollStartOffset.X + delta.X,
                    scrollStartOffset.Y + delta.Y);

                var inertiaProcessor = GetInertiaProcessor(scrollViewer);
                if (inertiaProcessor != null)
                    inertiaProcessor.ScrollTarget = scrollTarget;

                // Scroll to the new position.
                scrollViewer.ScrollToHorizontalOffset(scrollTarget.X);
                scrollViewer.ScrollToVerticalOffset(scrollTarget.Y);
            }
        }

        static void ScollerPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _previewCapture = false;
            _scrollBarClicked = false;
            var scrollViewer = (ScrollViewer)sender;
            if (scrollViewer.IsMouseCaptured)
            {
                scrollViewer.ReleaseMouseCapture();
            }
        }

        static void ScollerPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var scrollViewer = (ScrollViewer)sender;
            if (scrollViewer.IsMouseOver)
            {
                // Save starting point, used later when determining how much to scroll.
                _previewCapture = true;

                SetScrollStartPoint(scrollViewer, e.GetPosition(scrollViewer));
                SetScrollStartOffset(scrollViewer, new
                    Point(scrollViewer.HorizontalOffset, scrollViewer.VerticalOffset));
                //scrollViewer.CaptureMouse();
            }
        }

        #endregion

        #region Inertia Stuff

        /// <summary>
        /// Handles the inertia 
        /// </summary>
        class InertiaHandler : IDisposable
        {
            private Point _previousPoint;
            private Vector _velocity;
            readonly ScrollViewer _scroller;
            readonly DispatcherTimer _animationTimer;

            private Point _scrollTarget;
            public Point ScrollTarget
            {
                get { return _scrollTarget; }
                set { _scrollTarget = value; }
            }

            public InertiaHandler(ScrollViewer scroller)
            {
                this._scroller = scroller;
                _animationTimer = new DispatcherTimer();
                _animationTimer.Interval = new TimeSpan(0, 0, 0, 0, 20);
                _animationTimer.Tick += HandleWorldTimerTick;
                _animationTimer.Start();
            }

            private void HandleWorldTimerTick(object sender, EventArgs e)
            {
                if (_scroller.IsMouseCaptured)
                {
                    Point currentPoint = Mouse.GetPosition(_scroller);
                    _velocity = _previousPoint - currentPoint;
                    _previousPoint = currentPoint;
                }
                else
                {
                    if (_velocity.Length > 1)
                    {
                        _scroller.ScrollToHorizontalOffset(ScrollTarget.X);
                        _scroller.ScrollToVerticalOffset(ScrollTarget.Y);
                        _scrollTarget.X += _velocity.X;
                        _scrollTarget.Y += _velocity.Y;
                        _velocity *= KineticBehaviour.GetFriction(_scroller);
                    }
                }
            }

            #region IDisposable Members

            public void Dispose()
            {
                _animationTimer.Stop();
            }

            #endregion
        }

        #endregion
    }
}
