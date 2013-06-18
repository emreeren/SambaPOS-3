using Samba.Presentation.Common;
using Samba.Presentation.Common.Commands;

namespace Samba.Presentation.ViewModels
{
    public class CommandButtonViewModel<T> : ObservableObject
    {
        public ICaptionCommand Command { get; set; }

        private string _caption;
        public string Caption
        {
            get { return _caption; }
            set
            {
                _caption = value;
                RaisePropertyChanged(() => Caption);
            }
        }

        public T Parameter { get; set; }
        public string Color { get; set; }
        public int FontSize { get; set; }
    }
}
