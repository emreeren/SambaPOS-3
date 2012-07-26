using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Samba.Presentation.Common
{
    public interface IDiagram
    {
        object SettingsObject { get; }
        string CreatorName { get; set; }
        string Caption { get; set; }
        int X { get; set; }
        int Y { get; set; }
        int Height { get; set; }
        int Width { get; set; }
        bool IsEnabled { get; set; }
        ICommand Command { get; }
        CornerRadius CornerRadius { get; set; }
        Transform RenderTransform { get; set; }
        void EditProperties();
        void EditSettings();
        void SaveSettings();
        void Refresh();
    }
}
