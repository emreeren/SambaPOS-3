using System.Windows.Input;

namespace Samba.Presentation.Common
{
    public interface ICaptionCommand : ICommand
    {
        string Caption { get; set; }
    }
}
