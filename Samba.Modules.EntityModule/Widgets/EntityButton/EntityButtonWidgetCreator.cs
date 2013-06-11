using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using Samba.Domain.Models.Entities;
using Samba.Infrastructure.Helpers;
using Samba.Localization.Properties;
using Samba.Presentation.Common.Widgets;
using Samba.Presentation.Services;
using Samba.Services;

namespace Samba.Modules.EntityModule.Widgets.EntityButton
{
    [Export(typeof(IWidgetCreator))]
    public class EntityButtonWidgetCreator : IWidgetCreator
    {
        private readonly ICacheService _cacheService;
        private readonly IEntityService _entityService;

        [ImportingConstructor]
        public EntityButtonWidgetCreator(ICacheService cacheService, IEntityService entityService)
        {
            _cacheService = cacheService;
            _entityService = entityService;
        }

        public string GetCreatorName()
        {
            return "EntityButtonCreator";
        }

        public string GetCreatorDescription()
        {
            return Resources.EntityButton;
        }

        public Widget CreateNewWidget()
        {
            var parameters = JsonHelper.Serialize(new EntityButtonWidgetSettings());
            var result = new Widget { Properties = parameters, CreatorName = GetCreatorName() };
            return result;
        }

        public IDiagram CreateWidgetViewModel(Widget widget, IApplicationState applicationState)
        {
            return new EntityButtonWidgetViewModel(widget, _cacheService, applicationState, _entityService);
        }

        public FrameworkElement CreateWidgetControl(IDiagram widgetViewModel, ContextMenu contextMenu)
        {
            var buttonHolder = widgetViewModel as EntityButtonWidgetViewModel;

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
            var rotateTransform = new Binding("RotateTransform") { Source = buttonHolder, Mode = BindingMode.OneWay };

            ret.SetBinding(InkCanvas.LeftProperty, xBinding);
            ret.SetBinding(InkCanvas.TopProperty, yBinding);
            ret.SetBinding(FrameworkElement.HeightProperty, heightBinding);
            ret.SetBinding(FrameworkElement.WidthProperty, widthBinding);
            ret.SetBinding(ContentControl.ContentProperty, captionBinding);
            ret.SetBinding(FlexButton.FlexButton.CornerRadiusProperty, radiusBinding);
            ret.SetBinding(FlexButton.FlexButton.ButtonColorProperty, buttonColorBinding);
            ret.SetBinding(ButtonBase.CommandProperty, commandBinding);
            ret.SetBinding(FrameworkElement.LayoutTransformProperty, rotateTransform);
            //ret.SetBinding(UIElement.IsEnabledProperty, enabledBinding);

            return ret;
        }
    }
}