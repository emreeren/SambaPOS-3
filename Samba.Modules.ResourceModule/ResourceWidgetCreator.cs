using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using Samba.Presentation.Common;

namespace Samba.Modules.ResourceModule
{
    public class ResourceWidgetCreator : IWidgetCreator
    {
        public Type GetWidgetType()
        {
            return typeof(ResourceScreenItemWidget);
        }

        public ContentControl CreateWidget(IDiagram widgetContainer, ContextMenu contextMenu)
        {
            var buttonHolder = widgetContainer;

            var ret = new FlexButton.FlexButton { DataContext = buttonHolder, ContextMenu = contextMenu };
            ret.CommandParameter = buttonHolder;

            var heightBinding = new Binding("Height") { Source = buttonHolder, Mode = BindingMode.TwoWay };
            var widthBinding = new Binding("Width") { Source = buttonHolder, Mode = BindingMode.TwoWay };
            var xBinding = new Binding("X") { Source = buttonHolder, Mode = BindingMode.TwoWay };
            var yBinding = new Binding("Y") { Source = buttonHolder, Mode = BindingMode.TwoWay };
            var captionBinding = new Binding("Caption") { Source = buttonHolder, Mode = BindingMode.TwoWay };
            var radiusBinding = new Binding("CornerRadius") { Source = buttonHolder, Mode = BindingMode.TwoWay };
            var buttonColorBinding = new Binding("ButtonColor") { Source = buttonHolder, Mode = BindingMode.TwoWay };
            var commandBinding = new Binding("Command") { Source = buttonHolder, Mode = BindingMode.OneWay };
            var enabledBinding = new Binding("IsEnabled") { Source = buttonHolder, Mode = BindingMode.OneWay };
            var transformBinding = new Binding("RenderTransform") { Source = buttonHolder, Mode = BindingMode.OneWay };

            ret.SetBinding(InkCanvas.LeftProperty, xBinding);
            ret.SetBinding(InkCanvas.TopProperty, yBinding);
            ret.SetBinding(FrameworkElement.HeightProperty, heightBinding);
            ret.SetBinding(FrameworkElement.WidthProperty, widthBinding);
            ret.SetBinding(ContentControl.ContentProperty, captionBinding);
            ret.SetBinding(FlexButton.FlexButton.CornerRadiusProperty, radiusBinding);
            ret.SetBinding(FlexButton.FlexButton.ButtonColorProperty, buttonColorBinding);
            ret.SetBinding(ButtonBase.CommandProperty, commandBinding);
            ret.SetBinding(UIElement.RenderTransformProperty, transformBinding);
            ret.SetBinding(UIElement.IsEnabledProperty, enabledBinding);

            return ret;
        }
    }
}