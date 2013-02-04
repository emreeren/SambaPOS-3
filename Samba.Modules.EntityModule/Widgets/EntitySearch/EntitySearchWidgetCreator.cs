using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Samba.Domain.Models.Entities;
using Samba.Infrastructure.Helpers;
using Samba.Presentation.Common;
using Samba.Presentation.Services;
using Samba.Services;

namespace Samba.Modules.EntityModule.Widgets.EntitySearch
{
    [Export(typeof(IWidgetCreator))]
    class EntitySearchWidgetCreator : IWidgetCreator
    {
        private readonly IEntityService _resourceService;
        private readonly ICacheService _cacheService;

        [ImportingConstructor]
        public EntitySearchWidgetCreator(IApplicationState applicationState, IEntityService resourceService,
            ICacheService cacheService)
        {
            _resourceService = resourceService;
            _cacheService = cacheService;
        }

        public string GetCreatorName()
        {
            return "ResourceSearch";
        }

        public string GetCreatorDescription()
        {
            return "Resource Search";
        }

        public FrameworkElement CreateWidgetControl(IDiagram widget, ContextMenu contextMenu)
        {
            var viewModel = widget as EntitySearchWidgetViewModel;
            Debug.Assert(viewModel != null);

            var ret = new EntitySearchView(viewModel.EntitySearchViewModel) { DataContext = viewModel.EntitySearchViewModel, ContextMenu = contextMenu, Tag = widget };

            var heightBinding = new Binding("Height") { Source = viewModel, Mode = BindingMode.TwoWay };
            var widthBinding = new Binding("Width") { Source = viewModel, Mode = BindingMode.TwoWay };
            var xBinding = new Binding("X") { Source = viewModel, Mode = BindingMode.TwoWay };
            var yBinding = new Binding("Y") { Source = viewModel, Mode = BindingMode.TwoWay };

            ret.SetBinding(InkCanvas.LeftProperty, xBinding);
            ret.SetBinding(InkCanvas.TopProperty, yBinding);
            ret.SetBinding(FrameworkElement.HeightProperty, heightBinding);
            ret.SetBinding(FrameworkElement.WidthProperty, widthBinding);

            return ret;
        }

        public Widget CreateNewWidget()
        {
            var parameters = JsonHelper.Serialize(new EntitySearchWidgetSettings());
            return new Widget { Properties = parameters, CreatorName = GetCreatorName() };
        }

        public IDiagram CreateWidgetViewModel(Widget widget, IApplicationState applicationState)
        {
            return new EntitySearchWidgetViewModel(widget, applicationState, _cacheService, _resourceService);
        }
    }
}
