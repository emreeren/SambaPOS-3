using System;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;

namespace Samba.Presentation.Common.ModelBase
{
    public abstract class VisibleViewModelBase : ViewModelBase
    {
        public abstract Type GetViewType();

        DelegateCommand<object> _closeCommand;
        public DelegateCommand<object> CloseCommand
        {
            get { return _closeCommand ?? (_closeCommand = new DelegateCommand<object>(OnRequestClose, CanClose)); }
        }

        protected virtual bool CanClose(object arg)
        {
            return true;
        }

        private void PublishClose()
        {
            CommonEventPublisher.PublishViewClosedEvent(this);
        }

        private void OnRequestClose(object obj)
        {
            PublishClose();
        }

        public VisibleViewModelBase CallingView { get; set; }

        public virtual void OnClosed()
        {
            //override if needed
        }

        public virtual void OnShown()
        {
            //override if needed    
        }
    }
}
