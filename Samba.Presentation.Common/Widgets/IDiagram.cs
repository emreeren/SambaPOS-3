using System.Windows;
using System.Windows.Media;
using Samba.Domain.Models.Entities;

namespace Samba.Presentation.Common.Widgets
{
    public interface IDiagram
    {
        object SettingsObject { get; }
        string CreatorName { get; set; }
        int X { get; set; }
        int Y { get; set; }
        int Height { get; set; }
        int Width { get; set; }
        bool IsEnabled { get; set; }
        CornerRadius CornerRadius { get; set; }
        Transform RotateTransform { get; set; }
        Transform ScaleTransform { get; set; }
        Widget GetWidget();
        bool DesignMode { get; set; }
        bool AutoRefresh { get; set; }
        bool IsVisible { get; }
        void EditProperties();
        void EditSettings();
        void SaveSettings();
        void Refresh();
    }
}
