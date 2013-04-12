using System.Windows;
using System.Windows.Controls;
using Samba.Domain.Models.Entities;
using Samba.Presentation.Services;

namespace Samba.Presentation.Common.Widgets
{
    public interface IWidgetCreator
    {
        string GetCreatorName();
        string GetCreatorDescription();
        FrameworkElement CreateWidgetControl(IDiagram widget, ContextMenu contextMenu);
        Widget CreateNewWidget();
        IDiagram CreateWidgetViewModel(Widget widget, IApplicationState applicationState);
    }
}