using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace Samba.Presentation.Common
{
    /// <summary>
    /// InputMask for Textbox with 2 Properties: <see cref="InputMask"/>, <see cref="PromptChar"/>.
    /// </summary>
    public class TextBoxInputMaskBehavior : Behavior<TextBox>
    {
        #region DependencyProperties

        public static readonly DependencyProperty InputMaskProperty =
          DependencyProperty.Register("InputMask", typeof(string), typeof(TextBoxInputMaskBehavior), null);

        public string InputMask
        {
            get { return (string)GetValue(InputMaskProperty); }
            set { SetValue(InputMaskProperty, value); }
        }

        public static readonly DependencyProperty PromptCharProperty =
           DependencyProperty.Register("PromptChar", typeof(char), typeof(TextBoxInputMaskBehavior), 
                                        new PropertyMetadata('_'));

        public char PromptChar
        {
            get { return (char)GetValue(PromptCharProperty); }
            set { SetValue(PromptCharProperty, value); }
        }

        #endregion

        public MaskedTextProvider Provider { get; private set; }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += AssociatedObjectLoaded;
            AssociatedObject.PreviewTextInput += AssociatedObjectPreviewTextInput;
            AssociatedObject.PreviewKeyDown += AssociatedObjectPreviewKeyDown;

            DataObject.AddPastingHandler(AssociatedObject, Pasting);
        }


        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Loaded -= AssociatedObjectLoaded;
            AssociatedObject.PreviewTextInput -= AssociatedObjectPreviewTextInput;
            AssociatedObject.PreviewKeyDown -= AssociatedObjectPreviewKeyDown;

            DataObject.RemovePastingHandler(AssociatedObject, Pasting);
        }

        /*
        Mask Character  Accepts  Required?  
        0  Digit (0-9)  Required  
        9  Digit (0-9) or space  Optional  
        #  Digit (0-9) or space  Required  
        L  Letter (a-z, A-Z)  Required  
        ?  Letter (a-z, A-Z)  Optional  
        &  Any character  Required  
        C  Any character  Optional  
        A  Alphanumeric (0-9, a-z, A-Z)  Required  
        a  Alphanumeric (0-9, a-z, A-Z)  Optional  
           Space separator  Required 
        .  Decimal separator  Required  
        ,  Group (thousands) separator  Required  
        :  Time separator  Required  
        /  Date separator  Required  
        $  Currency symbol  Required  

        In addition, the following characters have special meaning:

        Mask Character  Meaning  
        <  All subsequent characters are converted to lower case  
        >  All subsequent characters are converted to upper case  
        |  Terminates a previous < or >  
        \  Escape: treat the next character in the mask as literal text rather than a mask symbol  

        */

        void AssociatedObjectLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Provider = new MaskedTextProvider(InputMask, CultureInfo.CurrentCulture);
            this.Provider.Set(AssociatedObject.Text);
            this.Provider.PromptChar = this.PromptChar;
            AssociatedObject.Text = this.Provider.ToDisplayString();

            //seems the only way that the text is formatted correct, when source is updated
            var textProp = DependencyPropertyDescriptor.FromProperty(TextBox.TextProperty, typeof(TextBox));
            if (textProp != null)
            {
                textProp.AddValueChanged(AssociatedObject, (s, args) => this.UpdateText());
            }
        }

        void AssociatedObjectPreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            this.TreatSelectedText();

            var position = this.GetNextCharacterPosition(AssociatedObject.SelectionStart);

            if (Keyboard.IsKeyToggled(Key.Insert))
            {
                if (this.Provider.Replace(e.Text, position))
                    position++;
            }
            else
            {
                if (this.Provider.InsertAt(e.Text, position))
                    position++;
            }

            position = this.GetNextCharacterPosition(position);

            this.RefreshText(position);

            e.Handled = true;
        }

        void AssociatedObjectPreviewKeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Space)//handle the space
            {
                this.TreatSelectedText();

                var position = this.GetNextCharacterPosition(AssociatedObject.SelectionStart);

                if (this.Provider.InsertAt(" ", position))
                    this.RefreshText(position);

                e.Handled = true;
            }

            if (e.Key == Key.Back)//handle the back space
            {
                if (this.TreatSelectedText())
                {
                    this.RefreshText(AssociatedObject.SelectionStart);
                }
                else
                {
                    if (AssociatedObject.SelectionStart != 0)
                    {
                        if (this.Provider.RemoveAt(AssociatedObject.SelectionStart - 1))
                            this.RefreshText(AssociatedObject.SelectionStart - 1);
                    }
                }

                e.Handled = true;
            }

            if (e.Key == Key.Delete)//handle the delete key
            {
                //treat selected text
                if (this.TreatSelectedText())
                {
                    this.RefreshText(AssociatedObject.SelectionStart);
                }
                else
                {

                    if (this.Provider.RemoveAt(AssociatedObject.SelectionStart))
                        this.RefreshText(AssociatedObject.SelectionStart);

                }

                e.Handled = true;
            }

        }

        /// <summary>
        /// Pasting prüft ob korrekte Daten reingepastet werden
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                var pastedText = (string)e.DataObject.GetData(typeof(string));

                this.TreatSelectedText();

                var position = GetNextCharacterPosition(AssociatedObject.SelectionStart);

                if (this.Provider.InsertAt(pastedText, position))
                {
                    this.RefreshText(position);
                }
            }

            e.CancelCommand();
        }

        private void UpdateText()
        {
            //check Provider.Text + TextBox.Text
            if (this.Provider.ToDisplayString().Equals(AssociatedObject.Text))
                return;

            //use provider to format
            var success = this.Provider.Set(AssociatedObject.Text);

            //ui and mvvm/codebehind should be in sync
            this.SetText(success ? this.Provider.ToDisplayString() : AssociatedObject.Text);
        }

        /// <summary>
        /// Falls eine Textauswahl vorliegt wird diese entsprechend behandelt.
        /// </summary>
        /// <returns>true Textauswahl behandelt wurde, ansonsten falls </returns>
        private bool TreatSelectedText()
        {
            if (AssociatedObject.SelectionLength > 0)
            {
                return this.Provider.RemoveAt(AssociatedObject.SelectionStart, 
                                              AssociatedObject.SelectionStart + AssociatedObject.SelectionLength - 1);
            }
            return false;
        }

        private void RefreshText(int position)
        {
            SetText(this.Provider.ToDisplayString());
            AssociatedObject.SelectionStart = position;
        }

        private void SetText(string text)
        {
            AssociatedObject.Text = String.IsNullOrWhiteSpace(text) ? String.Empty : text;
        }

        private int GetNextCharacterPosition(int startPosition)
        {
            var position = this.Provider.FindEditPositionFrom(startPosition, true);

            if (position == -1)
                return startPosition;
            else
                return position;
        }
    }
}
