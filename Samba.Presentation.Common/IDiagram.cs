using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Samba.Presentation.Common
{
    public interface IDiagram
    {
        string Caption { get; set; }
        int X { get; set; }
        int Y { get; set; }
        int Height { get; set; }
        int Width { get; set; }
        bool IsEnabled { get; set; }
        string ButtonColor { get; set; }
        ICommand Command { get; }
        CornerRadius CornerRadius { get; set; }
        Transform RenderTransform { get; set; }
        void EditProperties();
    }
}
