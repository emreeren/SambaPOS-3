using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Samba.Presentation.Controls.UIControls
{
    public class MaskedTextBox : TextBox
    {
        #region DependencyProperties

        public static readonly DependencyProperty UnmaskedTextProperty =
            DependencyProperty.Register("UnmaskedText", typeof(string),
                                        typeof(MaskedTextBox), new UIPropertyMetadata(""));

        public static readonly DependencyProperty InputMaskProperty =
            DependencyProperty.Register("InputMask", typeof(string), typeof(MaskedTextBox), new FrameworkPropertyMetadata(OnInputMaskChanged));

        public static readonly DependencyProperty PromptCharProperty =
            DependencyProperty.Register("PromptChar", typeof(char), typeof(MaskedTextBox),
                                        new PropertyMetadata('_'));

        public string UnmaskedText
        {
            get { return (string)GetValue(UnmaskedTextProperty); }
            set { SetValue(UnmaskedTextProperty, value); }
        }

        public string InputMask
        {
            get { return (string)GetValue(InputMaskProperty); }
            set { SetValue(InputMaskProperty, value); }
        }

        public char PromptChar
        {
            get { return (char)GetValue(PromptCharProperty); }
            set { SetValue(PromptCharProperty, value); }
        }

        #endregion

        private MaskedTextProvider _provider;

        public MaskedTextBox()
        {
            Loaded += MaskedTextBoxLoaded;
            PreviewTextInput += MaskedTextBoxPreviewTextInput;
            PreviewKeyDown += MaskedTextBoxPreviewKeyDown;
        }

        private void MaskedTextBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                TreatSelectedText();

                int position = GetNextCharacterPosition(SelectionStart, true);

                if (_provider.InsertAt(" ", position))
                    RefreshText(position);

                e.Handled = true;
            }

            if (e.Key == Key.Back)
            {
                TreatSelectedText();
                var position = this.GetNextCharacterPosition(SelectionStart, false);
                e.Handled = true;

                if (SelectionStart > 0)
                {
                    if (position == SelectionStart)
                        position = this.GetNextCharacterPosition(position - 1, false);
                    if (_provider.RemoveAt(position))
                    {
                        position = GetNextCharacterPosition(position, false);
                    }
                }

                RefreshText(position);

                e.Handled = true;
            }

            if (e.Key == Key.Delete)
            {
                if (TreatSelectedText())
                {
                    RefreshText(SelectionStart);
                }
                else
                {
                    if (_provider.RemoveAt(SelectionStart))
                        RefreshText(SelectionStart);
                }

                e.Handled = true;
            }
        }

        private void MaskedTextBoxPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TreatSelectedText();

            int position = GetNextCharacterPosition(SelectionStart, true);

            if (Keyboard.IsKeyToggled(Key.Insert))
            {
                if (_provider.Replace(e.Text, position))
                    position++;
            }
            else
            {
                if (_provider.InsertAt(e.Text, position))
                    position++;
            }

            position = GetNextCharacterPosition(position, true);

            RefreshText(position);

            e.Handled = true;
        }

        private void MaskedTextBoxLoaded(object sender, RoutedEventArgs e)
        {
            RefreshProvider();
        }

        private void RefreshProvider()
        {
            _provider = new MaskedTextProvider(!string.IsNullOrEmpty(InputMask) ? InputMask : " ", CultureInfo.CurrentCulture);

            _provider.Set(String.IsNullOrWhiteSpace(UnmaskedText) ? String.Empty : UnmaskedText);

            _provider.PromptChar = ' ';
            Text = _provider.ToDisplayString();

            DependencyPropertyDescriptor textProp = DependencyPropertyDescriptor.FromProperty(TextProperty,
                                                                                              typeof (MaskedTextBox));
            if (textProp != null)
            {
                textProp.AddValueChanged(this, (s, args) => UpdateText());
            }
            DataObject.AddPastingHandler(this, Pasting);
        }

        private void Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                var pastedText = (string)e.DataObject.GetData(typeof(string));

                TreatSelectedText();

                int position = GetNextCharacterPosition(SelectionStart, true);

                if (_provider.InsertAt(pastedText, position))
                {
                    RefreshText(position);
                }
            }

            e.CancelCommand();
        }

        private void UpdateText()
        {
            if (_provider.ToDisplayString().Equals(Text))
                return;

            bool success = _provider.Set(Text);

            SetText(success ? _provider.ToDisplayString() : Text, _provider.ToString(false, false));
        }

        private bool TreatSelectedText()
        {
            if (SelectionLength > 0)
            {
                return _provider.RemoveAt(SelectionStart,
                                         SelectionStart + SelectionLength - 1);
            }
            return false;
        }

        private void RefreshText(int position)
        {
            SetText(_provider.ToDisplayString(), _provider.ToString(false, false));
            SelectionStart = position;
        }

        private void SetText(string text, string unmaskedText)
        {
            UnmaskedText = String.IsNullOrWhiteSpace(unmaskedText) ? null : unmaskedText;
            Text = String.IsNullOrWhiteSpace(text) ? null : text;
        }

        private static void OnInputMaskChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
           ((MaskedTextBox)d).RefreshProvider();
        }

        private int GetNextCharacterPosition(int startPosition, bool goForward)
        {
            var position = _provider.FindEditPositionFrom(startPosition, goForward);

            return position == -1 ? startPosition : position;
        }
    }
}