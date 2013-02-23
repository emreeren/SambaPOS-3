using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Samba.Domain.Models.Entities;
using Samba.Presentation.Services;

namespace Samba.Presentation.Common.Widgets
{
    public class DefaultWidgetViewModel : WidgetViewModel
    {
        public DefaultWidgetViewModel(Widget model, IApplicationState applicationState)
            : base(model, applicationState)
        {
        }

        protected override object CreateSettingsObject()
        {
            return null;
        }

        public override void Refresh()
        {

        }
    }

    public class DefaultWidgetCreator : IWidgetCreator
    {
        public string GetCreatorName()
        {
            return "";
        }

        public string GetCreatorDescription()
        {
            return "Default Widget Creator";
        }

        public FrameworkElement CreateWidgetControl(IDiagram widgetViewModel, ContextMenu contextMenu)
        {
            var buttonHolder = widgetViewModel as DefaultWidgetViewModel;
            var brd = new Border
                          {
                              DataContext = buttonHolder,
                              ContextMenu = contextMenu,
                              BorderBrush = System.Windows.Media.Brushes.Gray,
                              Background = System.Windows.Media.Brushes.White
                          };

            var ret = new Button { DataContext = buttonHolder, ContextMenu = contextMenu, Content = "New Widget" };

            brd.Child = ret;

            var heightBinding = new Binding("Height") { Source = buttonHolder, Mode = BindingMode.TwoWay };
            var widthBinding = new Binding("Width") { Source = buttonHolder, Mode = BindingMode.TwoWay };
            var xBinding = new Binding("X") { Source = buttonHolder, Mode = BindingMode.TwoWay };
            var yBinding = new Binding("Y") { Source = buttonHolder, Mode = BindingMode.TwoWay };
            var transformBinding = new Binding("RenderTransform") { Source = buttonHolder, Mode = BindingMode.OneWay };

            brd.SetBinding(InkCanvas.LeftProperty, xBinding);
            brd.SetBinding(InkCanvas.TopProperty, yBinding);
            brd.SetBinding(FrameworkElement.HeightProperty, heightBinding);
            brd.SetBinding(FrameworkElement.WidthProperty, widthBinding);
            brd.SetBinding(UIElement.RenderTransformProperty, transformBinding);

            return brd;
        }

        public Widget CreateNewWidget()
        {
            return new Widget { CreatorName = GetCreatorName() };
        }

        public IDiagram CreateWidgetViewModel(Widget widget, IApplicationState applicationState)
        {
            return new DefaultWidgetViewModel(widget, applicationState);
        }
    }
}