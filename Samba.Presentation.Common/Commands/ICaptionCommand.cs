using System.Windows.Input;

namespace Samba.Presentation.Common.Commands
{
    public interface ICaptionCommand : ICommand
    {
        string Caption { get; set; }
    }
}
