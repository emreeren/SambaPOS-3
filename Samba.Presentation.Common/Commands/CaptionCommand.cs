using System;
using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using Samba.Presentation.Common.Annotations;

namespace Samba.Presentation.Common.Commands
{
    public class CaptionCommand<T> : DelegateCommand<T>, ICaptionCommand, INotifyPropertyChanged
    {
        private string _caption;

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

        public string Caption
        {
            get { return _caption; }
            set { _caption = value; OnPropertyChanged("Caption"); }
        }

        public new event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
