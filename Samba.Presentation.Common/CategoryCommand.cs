using System;

namespace Samba.Presentation.Common
{
    public class CategoryCommand<T> : CaptionCommand<T>, ICategoryCommand
    {
        public CategoryCommand(string caption, string category, Action<T> executeMethod)
            : base(caption, executeMethod)
        {
            Caption = caption;
            Category = category;
        }

        public CategoryCommand(string caption, string category, Action<T> executeMethod, Func<T, bool> canExecuteMethod)
            : this(caption, category, "", executeMethod, canExecuteMethod)
        {
        }

        public CategoryCommand(string caption, string category, string imageSource, Action<T> executeMethod)
            : base(caption, executeMethod)
        {
            Category = category;
            ImageSource = imageSource;
        }

        public CategoryCommand(string caption, string category, string imageSource, Action<T> executeMethod, Func<T, bool> canExecuteMethod)
            : base(caption, executeMethod, canExecuteMethod)
        {
            Category = category;
            ImageSource = imageSource;
        }

        public string Category { get; set; }
        public string ImageSource { get; set; }
        public int Order { get; set; }
    }
}
