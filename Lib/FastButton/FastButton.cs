using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace FastButton
{
    public class FastButton : ToggleButton
    {
        public static readonly DependencyProperty CornerRadiusProperty =
            Border.CornerRadiusProperty.AddOwner(typeof(FastButton));

        public static readonly DependencyProperty OuterBorderBrushProperty =
            DependencyProperty.Register("OuterBorderBrush", typeof(Brush), typeof(FastButton));

        public static readonly DependencyProperty GlowColorProperty =
            DependencyProperty.Register("GlowColor", typeof(SolidColorBrush), typeof(FastButton));

        public static readonly DependencyProperty ButtonColorProperty =
            DependencyProperty.Register("ButtonColor", typeof(SolidColorBrush), typeof(FastButton),
            new FrameworkPropertyMetadata(OnButtonColorChanged));

        public static readonly DependencyProperty HighlightBrightnessProperty =
            DependencyProperty.Register("HighlightBrightness", typeof(byte), typeof(FastButton));


        #region Properties...

        public Brush GlowColor
        {
            get { return (SolidColorBrush)GetValue(GlowColorProperty); }
            set { SetValue(GlowColorProperty, value); }
        }

        public Brush ButtonColor
        {
            get { return (SolidColorBrush)GetValue(ButtonColorProperty); }
            set { SetValue(ButtonColorProperty, value); }
        }

        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        public Brush OuterBorderBrush
        {
            get { return (Brush)GetValue(OuterBorderBrushProperty); }
            set { SetValue(OuterBorderBrushProperty, value); }
        }

        public byte HighlightBrightness
        {
            get { return (byte)GetValue(HighlightBrightnessProperty); }
            set { SetValue(HighlightBrightnessProperty, value); }
        }

        #endregion (Properties)

        static FastButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FastButton), new FrameworkPropertyMetadata(typeof(FastButton)));
        }

        public FastButton()
        {
            IsEnabledChanged += FastButtonIsEnabledChanged;
        }

        void FastButtonIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SolidColorBrush brush = ButtonColor != null ? (SolidColorBrush)ButtonColor : Brushes.Gainsboro;
            UpdateForeground(brush, this);
        }

        private static void OnButtonColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
                UpdateButtonColor((FastButton)d, (SolidColorBrush)e.NewValue);
            else UpdateButtonColor((FastButton)d, Brushes.Gainsboro);
        }

        private static void UpdateButtonColor(FastButton item, SolidColorBrush color)
        {
            var btn = item;
            btn.OuterBorderBrush = new SolidColorBrush(color.Color.Lerp(Colors.Black, Brightness(color.Color) > 150 ? 0.3f : 0f));
            UpdateForeground(color, btn);
            btn.Background = color;
            btn.GlowColor = (new SolidColorBrush(color.Color.Lerp(Colors.White, 0.65f)));
        }

        private static int Brightness(Color c)
        {
            return (int)Math.Sqrt(
               c.R * c.R * .241 +
               c.G * c.G * .691 +
               c.B * c.B * .068);
        }

        private static void UpdateForeground(SolidColorBrush fColor, FastButton btn)
        {
            btn.Foreground = btn.IsEnabled
                ? new SolidColorBrush(Brightness(fColor.Color) < 150 ? Colors.White : Colors.Black)
                : new SolidColorBrush(fColor.Color.Lerp(Colors.Black, 0.25f));
        }
    }
}
