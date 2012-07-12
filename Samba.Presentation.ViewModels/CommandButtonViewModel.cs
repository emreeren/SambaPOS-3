using Samba.Presentation.Common;

namespace Samba.Presentation.ViewModels
{
    public class CommandButtonViewModel<T> : ObservableObject
    {
        public CommandButtonViewModel()
        {
            Color = "Gainsboro";
        }

        public ICaptionCommand Command { get; set; }
        public string Caption { get; set; }
        public T Parameter { get; set; }
        public string Color { get; set; }
    }
}
