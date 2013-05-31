using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using Samba.Domain.Models.Entities;
using Samba.Infrastructure.Helpers;
using Samba.Persistance;
using Samba.Presentation.Common.Widgets;
using Samba.Presentation.Controls.Converters;
using Samba.Presentation.Services;

namespace Samba.Modules.AutomationModule.WidgetCreators
{
    [Export(typeof(IWidgetCreator))]
    class AutomationButtonWidgetCreator : IWidgetCreator
    {
        private readonly IAutomationDao _automationDao;
        private readonly IValueConverter _brushConverter;

        [ImportingConstructor]
        public AutomationButtonWidgetCreator(IAutomationDao automationDao)
        {
            _automationDao = automationDao;
            _brushConverter = new NullBrushConverter();
        }

        public string GetCreatorName()
        {
            return "AutomationButtonCreator";
        }

        public string GetCreatorDescription()
        {
            return "Automation Command Button";
        }

        public FrameworkElement CreateWidgetControl(IDiagram widget, ContextMenu contextMenu)
        {
            var buttonHolder = widget as AutomationButtonWidgetViewModel;

            var ret = new FlexButton.FlexButton { DataContext = buttonHolder, ContextMenu = contextMenu, CommandParameter = buttonHolder };

            var heightBinding = new Binding("Height") { Source = buttonHolder, Mode = BindingMode.TwoWay };
            var widthBinding = new Binding("Width") { Source = buttonHolder, Mode = BindingMode.TwoWay };
            var xBinding = new Binding("X") { Source = buttonHolder, Mode = BindingMode.TwoWay };
            var yBinding = new Binding("Y") { Source = buttonHolder, Mode = BindingMode.TwoWay };
            var captionBinding = new Binding("Settings.Caption") { Source = buttonHolder, Mode = BindingMode.TwoWay };
            var radiusBinding = new Binding("CornerRadius") { Source = buttonHolder, Mode = BindingMode.TwoWay };
            var buttonColorBinding = new Binding("Settings.ButtonColor") { Source = buttonHolder, Mode = BindingMode.TwoWay };
            var commandBinding = new Binding("ItemClickedCommand") { Source = buttonHolder, Mode = BindingMode.OneWay };
            var enabledBinding = new Binding("IsEnabled") { Source = buttonHolder, Mode = BindingMode.OneWay, Converter = _brushConverter };
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

            return ret;
        }

        public Widget CreateNewWidget()
        {
            var parameters = JsonHelper.Serialize(new AutomationButtonWidgetSettings());
            var result = new Widget { Properties = parameters, CreatorName = GetCreatorName() };
            return result;
        }

        public IDiagram CreateWidgetViewModel(Widget widget, IApplicationState applicationState)
        {
            return new AutomationButtonWidgetViewModel(widget, applicationState, _automationDao);
        }
    }
}
