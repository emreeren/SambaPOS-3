using System;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;

namespace Samba.Presentation.Common
{
    public class CaptionCommand<T> : DelegateCommand<T>, ICaptionCommand
    {
        public CaptionCommand(string caption, Action<T> executeMethod)
            : base(executeMethod)
        {
            Caption = caption;
        }

        public CaptionCommand(string caption, Action<T> executeMethod, Func<T, bool> canExecuteMethod)
            : base(executeMethod, canExecuteMethod)
        {
            Caption = caption;
        }

        public string Caption { get; set; }

        public new event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
