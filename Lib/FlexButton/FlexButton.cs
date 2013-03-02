using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace FlexButton
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:FlexButton"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:FlexButton;assembly=FlexButton"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:CustomControl1/>
    ///
    /// </summary>
    public class FlexButton : ToggleButton
    {
        public enum Highlight { Diffuse, Elliptical, Image }

        public static readonly DependencyProperty CornerRadiusProperty =
            Border.CornerRadiusProperty.AddOwner(typeof(FlexButton));

        public static readonly DependencyProperty OuterBorderBrushProperty =
            DependencyProperty.Register("OuterBorderBrush", typeof(Brush), typeof(FlexButton));

        public static readonly DependencyProperty OuterBorderThicknessProperty =
            DependencyProperty.Register("OuterBorderThickness", typeof(Thickness), typeof(FlexButton));

        public static readonly DependencyProperty InnerBorderBrushProperty =
           DependencyProperty.Register("InnerBorderBrush", typeof(Brush), typeof(FlexButton));

        public static readonly DependencyProperty InnerBorderThicknessProperty =
            DependencyProperty.Register("InnerBorderThickness", typeof(Thickness), typeof(FlexButton));

        public static readonly DependencyProperty GlowColorProperty =
            DependencyProperty.Register("GlowColor", typeof(SolidColorBrush), typeof(FlexButton));

        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("Color", typeof(SolidColorBrush), typeof(FlexButton),
            new FrameworkPropertyMetadata(OnColorChanged));

        public static readonly DependencyProperty ButtonColorProperty =
            DependencyProperty.Register("ButtonColor", typeof(SolidColorBrush), typeof(FlexButton),
            new FrameworkPropertyMetadata(OnButtonColorChanged));

        public static readonly DependencyProperty HighlightAppearanceProperty =
            DependencyProperty.Register("HighlightAppearance", typeof(ControlTemplate), typeof(FlexButton));

        public static readonly DependencyProperty HighlightMarginProperty =
            DependencyProperty.Register("HighlightMargin", typeof(Thickness), typeof(FlexButton));

        public static readonly DependencyProperty HighlightBrightnessProperty =
            DependencyProperty.Register("HighlightBrightness", typeof(byte), typeof(FlexButton));

        public static readonly DependencyProperty ButtonImageProperty =
            DependencyProperty.Register("ButtonImage", typeof(ImageSource), typeof(FlexButton));


        #region Properties...

        public ImageSource ButtonImage
        {
            get { return (ImageSource)GetValue(ButtonImageProperty); }
            set { SetValue(ButtonImageProperty, value); }
        }

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

        public Brush Color
        {
            get { return (SolidColorBrush)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
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

        public Thickness OuterBorderThickness
        {
            get { return (Thickness)GetValue(OuterBorderThicknessProperty); }
            set { SetValue(OuterBorderThicknessProperty, value); }
        }

        public Brush InnerBorderBrush
        {
            get { return (Brush)GetValue(InnerBorderBrushProperty); }
            set { SetValue(InnerBorderBrushProperty, value); }
        }

        public Thickness InnerBorderThickness
        {
            get { return (Thickness)GetValue(InnerBorderThicknessProperty); }
            set { SetValue(InnerBorderThicknessProperty, value); }
        }

        // Force clients to pass enum value to HighlightStyle by hiding this accessor
        internal ControlTemplate HighlightAppearance
        {
            get { return (ControlTemplate)GetValue(HighlightAppearanceProperty); }
            set { SetValue(HighlightAppearanceProperty, value); }
        }

        public Thickness HighlightMargin
        {
            get { return (Thickness)GetValue(HighlightMarginProperty); }
            set { SetValue(HighlightMarginProperty, value); }
        }

        public byte HighlightBrightness
        {
            get { return (byte)GetValue(HighlightBrightnessProperty); }
            set { SetValue(HighlightBrightnessProperty, value); }
        }

        #endregion (Properties)

        static FlexButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FlexButton), new FrameworkPropertyMetadata(typeof(FlexButton)));
        }

        public FlexButton()
        {
            HighlightMargin = new Thickness(0);
            HighlightBrightness = 100;
            GlowColor = Brushes.WhiteSmoke;

            OuterBorderBrush = Brushes.Gray;

            InnerBorderBrush = new LinearGradientBrush(Colors.White, Colors.LightGray, 90);
            InnerBorderThickness = new Thickness(1);

            CornerRadius = new CornerRadius(3);

            UpdateButtonColor(this, Brushes.Gainsboro);
            Foreground = Brushes.Black;

            IsEnabledChanged += FlexButtonIsEnabledChanged;
        }

        void FlexButtonIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SolidColorBrush brush = ButtonColor != null ? (SolidColorBrush)ButtonColor : Brushes.Gainsboro;
            UpdateForeground(brush, this);
        }

        private static void OnButtonColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
                UpdateButtonColor((FlexButton)d, (SolidColorBrush)e.NewValue);
            else UpdateButtonColor((FlexButton)d, Brushes.Gainsboro);
        }

        private static void OnColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                var newColor = (SolidColorBrush)e.NewValue;
                var fb = d as FlexButton;
                if (fb != null && fb.ButtonColor == null)
                {
                    UpdateButtonColor(fb, newColor);
                }
            }
            else UpdateButtonColor((FlexButton)d, (SolidColorBrush)((FlexButton)d).ButtonColor ?? Brushes.Gainsboro);
        }

        private static void UpdateButtonColor(FlexButton item, SolidColorBrush color)
        {
            var btn = item;
            var fColor = color;
            var gColor = new SolidColorBrush(fColor.Color.Lerp(Colors.White, 0.65f));

            btn.InnerBorderBrush = new LinearGradientBrush(Colors.White, Colors.Transparent, 90) { Opacity = 0.5 };
            btn.OuterBorderBrush = new SolidColorBrush(fColor.Color.Lerp(Colors.Black, Brightness(fColor.Color) > 150 ? 0.3f : 0f));

            UpdateForeground(fColor, btn);

            btn.Background = fColor;
            btn.GlowColor = (gColor);
        }

        private static int Brightness(Color c)
        {
            return (int)Math.Sqrt(
               c.R * c.R * .241 +
               c.G * c.G * .691 +
               c.B * c.B * .068);
        }

        private static void UpdateForeground(SolidColorBrush fColor, FlexButton btn)
        {
            if (fColor == null) return;

            btn.Foreground = btn.IsEnabled
                ? new SolidColorBrush(Brightness(fColor.Color) < 150 ? Colors.White : Colors.Black)
                : new SolidColorBrush(fColor.Color.Lerp(Colors.Black, 0.25f));
        }
    }
}
