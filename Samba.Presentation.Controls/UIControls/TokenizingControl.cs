using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Samba.Presentation.Controls.UIControls
{
    public class TokenizingControl : RichTextBox
    {
        private bool _preventCallback;

        public static readonly DependencyProperty SplitterProperty =
            DependencyProperty.Register("Splitter", typeof(char), typeof(TokenizingControl));

        public static readonly DependencyProperty AltSplitterProperty =
            DependencyProperty.Register("AltSplitter", typeof(char), typeof(TokenizingControl));

        public static readonly DependencyProperty TokenTemplateProperty =
            DependencyProperty.Register("TokenTemplate", typeof(DataTemplate), typeof(TokenizingControl));

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(TokenizingControl), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTextChanged));


        //new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnPasswordPropertyChanged)
        public DataTemplate TokenTemplate
        {
            get { return (DataTemplate)GetValue(TokenTemplateProperty); }
            set { SetValue(TokenTemplateProperty, value); }
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public char Splitter
        {
            get { return (char)GetValue(SplitterProperty); }
            set { SetValue(SplitterProperty, value); }
        }

        public char AltSplitter
        {
            get { return (char)GetValue(AltSplitterProperty); }
            set { SetValue(AltSplitterProperty, value); }
        }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var tc = d as TokenizingControl;
            if (tc != null && !tc._preventCallback)
            {
                if (tc.CaretPosition.Paragraph != null) 
                    tc.CaretPosition.Paragraph.Inlines.Clear();
                if (e.NewValue != null)
                {
                    e.NewValue.ToString().Split(tc.Splitter).ToList().ForEach(tc.CreateToken);
                }
            }
        }

        private void CreateToken(string obj)
        {
            var tokenContainer = CreateTokenContainer(obj);
            if (CaretPosition.Paragraph != null) CaretPosition.Paragraph.Inlines.Add(tokenContainer);
        }

        private Func<string, object> _tokenMatcher;
        public Func<string, object> TokenMatcher
        {
            get { return _tokenMatcher ?? (_tokenMatcher = GetDefaultTokenMatcher); }
            set { _tokenMatcher = value; }
        }

        private object GetDefaultTokenMatcher(string text)
        {
            text = text.Replace(AltSplitter, Splitter);
            if (text.EndsWith(Splitter.ToString()))
            {
                return text.Substring(0, text.Length - 1).Trim();
            }
            return null;
        }

        public TokenizingControl()
        {
            TextChanged += OnTokenTextChanged;
            Splitter = ';';
            AltSplitter = ',';
        }

        private void OnTokenTextChanged(object sender, TextChangedEventArgs e)
        {
            var text = CaretPosition.GetTextInRun(LogicalDirection.Backward);
            if (TokenMatcher != null)
            {
                var token = TokenMatcher(text);
                if (token != null)
                {
                    ReplaceTextWithToken(text, token);

                }
                if (CaretPosition.Paragraph != null)
                {
                    _preventCallback = true;
                    Text = string.Join(Splitter.ToString(), CaretPosition.Paragraph.Inlines.Where(x => x is InlineUIContainer).Cast
                                           <InlineUIContainer>().Select(
                                               x =>
                                               {
                                                   var contentPresenter = x.Child as ContentPresenter;
                                                   return contentPresenter != null ? contentPresenter.Content.ToString() : null;
                                               }).ToArray());
                    _preventCallback = false;
                }
            }

        }

        private void ReplaceTextWithToken(string inputText, object token)
        {
            TextChanged -= OnTokenTextChanged;

            var paragraph = CaretPosition.Paragraph;
            Debug.Assert(paragraph != null);

            var matchedRun = FindRun(inputText, paragraph);
            if (matchedRun != null)
            {
                var tokenContainer = CreateTokenContainer(token);
                if (inputText.Trim().Length > 1)
                    paragraph.Inlines.InsertBefore(matchedRun, tokenContainer);
                if (matchedRun.Text == inputText)
                {
                    paragraph.Inlines.Remove(matchedRun);
                }
            }

            TextChanged += OnTokenTextChanged;
        }

        private static Run FindRun(string inputText, Paragraph paragraph)
        {
            return paragraph.Inlines.FirstOrDefault(inline =>
                                                   {
                                                       var run = inline as Run;
                                                       return (run != null && run.Text.EndsWith(inputText));
                                                   }) as Run;
        }

        private InlineUIContainer CreateTokenContainer(object token)
        {
            var presenter = new ContentPresenter()
            {
                Content = token,
                ContentTemplate = TokenTemplate,
            };
            return new InlineUIContainer(presenter) { BaselineAlignment = BaselineAlignment.Center };
        }
    }
}