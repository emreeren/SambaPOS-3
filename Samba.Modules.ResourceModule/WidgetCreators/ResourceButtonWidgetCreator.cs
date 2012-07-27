using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using Samba.Domain.Models.Resources;
using Samba.Infrastructure;
using Samba.Presentation.Common;
using Samba.Services;

namespace Samba.Modules.ResourceModule.WidgetCreators
{
    [Export(typeof(IWidgetCreator))]
    public class ResourceButtonWidgetCreator : IWidgetCreator
    {
        private readonly ICacheService _cacheService;
        private readonly IApplicationState _applicationState;
        private readonly IResourceService _resourceService;

        [ImportingConstructor]
        public ResourceButtonWidgetCreator(ICacheService cacheService, IApplicationState applicationState, IResourceService resourceService)
        {
            _cacheService = cacheService;
            _applicationState = applicationState;
            _resourceService = resourceService;
        }

        public string GetCreatorName()
        {
            return "Resource Button";
        }

        public Widget CreateNewWidget()
        {
            var parameters = JsonHelper.Serialize(new ResourceWidgetSettings());
            var result = new Widget { Properties = parameters, CreatorName = GetCreatorName() };
            return result;
        }

        public IDiagram CreateWidgetViewModel(Widget widget)
        {
            return new ResourceButtonWidgetViewModel(widget, _cacheService, _applicationState, _resourceService);
        }

        public FrameworkElement CreateWidgetControl(IDiagram widgetViewModel, ContextMenu contextMenu)
        {
            var buttonHolder = widgetViewModel as ResourceButtonWidgetViewModel;

            var ret = new FlexButton.FlexButton { DataContext = buttonHolder, ContextMenu = contextMenu, CommandParameter = buttonHolder };

            var heightBinding = new Binding("Height") { Source = buttonHolder, Mode = BindingMode.TwoWay };
            var widthBinding = new Binding("Width") { Source = buttonHolder, Mode = BindingMode.TwoWay };
            var xBinding = new Binding("X") { Source = buttonHolder, Mode = BindingMode.TwoWay };
            var yBinding = new Binding("Y") { Source = buttonHolder, Mode = BindingMode.TwoWay };
            var captionBinding = new Binding("Settings.Caption") { Source = buttonHolder, Mode = BindingMode.TwoWay };
            var radiusBinding = new Binding("CornerRadius") { Source = buttonHolder, Mode = BindingMode.TwoWay };
            var buttonColorBinding = new Binding("ButtonColor") { Source = buttonHolder, Mode = BindingMode.TwoWay };
            var commandBinding = new Binding("ItemClickedCommand") { Source = buttonHolder, Mode = BindingMode.OneWay };
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
            //ret.SetBinding(UIElement.IsEnabledProperty, enabledBinding);

            return ret;
        }
    }
}