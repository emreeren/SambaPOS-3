using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Samba.Domain.Models.Entities;
using Samba.Infrastructure.Helpers;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Widgets;
using Samba.Presentation.Controls.Browser;
using Samba.Presentation.Services;

namespace Samba.Modules.SettingsModule.Widgets.HtmlViewer
{
    [Export(typeof(IWidgetCreator))]
    public class HtmlViewerWidgetCreator : IWidgetCreator
    {
        [ImportingConstructor]
        public HtmlViewerWidgetCreator()
        {

        }

        public string GetCreatorName()
        {
            return "HtmlViewerCreator";
        }

        public string GetCreatorDescription()
        {
            return "Html Viewer";
        }

        public Widget CreateNewWidget()
        {
            var parameters = JsonHelper.Serialize(new HtmlViewerWidgetSettings());
            var result = new Widget { Properties = parameters, CreatorName = GetCreatorName() };
            return result;
        }

        public IDiagram CreateWidgetViewModel(Widget widget, IApplicationState applicationState)
        {
            return new HtmlViewerWidgetViewModel(widget, applicationState);
        }

        public FrameworkElement CreateWidgetControl(IDiagram widgetViewModel, ContextMenu contextMenu)
        {
            var htmlViewer = widgetViewModel as HtmlViewerWidgetViewModel;

            var ret = new BrowserControl { DataContext = htmlViewer, ContextMenu = contextMenu };

            var heightBinding = new Binding("Height") { Source = htmlViewer, Mode = BindingMode.TwoWay };
            var widthBinding = new Binding("Width") { Source = htmlViewer, Mode = BindingMode.TwoWay };
            var xBinding = new Binding("X") { Source = htmlViewer, Mode = BindingMode.TwoWay };
            var yBinding = new Binding("Y") { Source = htmlViewer, Mode = BindingMode.TwoWay };
            var transformBinding = new Binding("RenderTransform") { Source = htmlViewer, Mode = BindingMode.OneWay };
            var urlBinding = new Binding("Url") { Source = htmlViewer, Mode = BindingMode.TwoWay };
            var toolbarVisibleBinding = new Binding("Settings.IsToolbarVisible") {Source = htmlViewer, Mode = BindingMode.TwoWay};

            ret.SetBinding(InkCanvas.LeftProperty, xBinding);
            ret.SetBinding(InkCanvas.TopProperty, yBinding);
            ret.SetBinding(FrameworkElement.HeightProperty, heightBinding);
            ret.SetBinding(FrameworkElement.WidthProperty, widthBinding);
            ret.SetBinding(UIElement.RenderTransformProperty, transformBinding);
            ret.SetBinding(BrowserControl.ActiveUrlProperty, urlBinding);
            ret.SetBinding(BrowserControl.IsToolbarVisibleProperty, toolbarVisibleBinding);

            return ret;
        }
    }
}