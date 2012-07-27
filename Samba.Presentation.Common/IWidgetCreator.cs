using System.Windows;
using System.Windows.Controls;
using Samba.Domain.Models.Resources;

namespace Samba.Presentation.Common
{
    public interface IWidgetCreator
    {
        string GetCreatorName();
        FrameworkElement CreateWidgetControl(IDiagram widget, ContextMenu contextMenu);
        Widget CreateNewWidget();
        IDiagram CreateWidgetViewModel(Widget widget);
    }
}