using System;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;

namespace Samba.Presentation.Common
{
    public class CustomCommand : DelegateCommand<object>, ICaptionCommand
    {
        public CustomCommand(string caption, object dataObject, Action<object> executeMethod)
            : base(executeMethod)
        {
            Caption = caption;
            DataObject = dataObject;
        }

        public CustomCommand(string caption, Action<object> executeMethod, object dataObject, Func<object, bool> canExecuteMethod)
            : base(executeMethod, canExecuteMethod)
        {
            Caption = caption;
            DataObject = dataObject;
        }

        public string Caption { get; set; }
        public object DataObject { get; set; }

        public new event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
