using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace Samba.Presentation.Controls.UIControls
{
    /// <summary>
    /// FilteredTextBox is a class that represent a TecBox which can contains only letters, digits, punctuation, etc....
    /// The choice is done by specifying the Type property.
    /// </summary>
    public class FilteredTextBox : TextBox
    {
        #region Constructors

        static FilteredTextBox()
        {
            //DefaultStyleKeyProperty.OverrideMetadata(typeof(FilteredTextBox), new FrameworkPropertyMetadata((typeof(FilteredTextBox))));
        }

        public FilteredTextBox()
            : base()
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Indicate the type of the filter to apply.
        /// </summary>
        public FilteredTextBoxType Type
        {
            get { return (FilteredTextBoxType)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }

        public static readonly DependencyProperty TypeProperty =
            DependencyProperty.Register("Type", typeof(FilteredTextBoxType), typeof(FilteredTextBox), new FrameworkPropertyMetadata(FilteredTextBoxType.Letters));


        /// <summary>
        /// Indicate the label to use to describe the FilteredTextBox.
        /// </summary>
        public string LabelInfo
        {
            get { return (string)GetValue(LabelInfoProperty); }
            set { SetValue(LabelInfoProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LabelInfo.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LabelInfoProperty =
            DependencyProperty.Register("LabelInfo", typeof(string), typeof(FilteredTextBox), new FrameworkPropertyMetadata(string.Empty));

        #endregion

        /// <summary>
        /// Enum used to specify the type of the FilteredTextBox.
        /// </summary>
        public enum FilteredTextBoxType
        {
            Digits,
            Letters,
            Punctuation,
            Symbol,
            Number
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            this.SelectAll();
        }

        protected override void OnPreviewTextInput(System.Windows.Input.TextCompositionEventArgs e)
        {
            base.OnPreviewTextInput(e);

            char LetterOrDigit = Convert.ToChar(e.Text);
            char ds = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0];

            switch (Type)
            {
                case FilteredTextBoxType.Digits:
                    if (!char.IsDigit(LetterOrDigit))
                    {
                        e.Handled = true;
                    }
                    break;

                case FilteredTextBoxType.Number:
                    if (LetterOrDigit == '-' && SelectionStart > 0)
                        e.Handled = true;
                    if (LetterOrDigit == ds && Text.Contains(ds.ToString()))
                        e.Handled = true;
                    else if (!char.IsDigit(LetterOrDigit) && (LetterOrDigit != ds) && (LetterOrDigit != '-'))
                    {
                        e.Handled = true;
                    }
                    break;

                case FilteredTextBoxType.Letters:
                    if (!char.IsLetterOrDigit(LetterOrDigit))
                    {
                        e.Handled = true;
                    }
                    break;

                case FilteredTextBoxType.Punctuation:
                    if (!char.IsPunctuation(LetterOrDigit))
                    {
                        e.Handled = true;
                    }
                    break;

                case FilteredTextBoxType.Symbol:
                    if (!char.IsSymbol(LetterOrDigit))
                    {
                        e.Handled = true;
                    }
                    break;

                default:
                    break;
            }
        }
    }
}
