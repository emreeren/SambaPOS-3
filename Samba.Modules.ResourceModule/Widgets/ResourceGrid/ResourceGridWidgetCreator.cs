using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Samba.Domain.Models.Resources;
using Samba.Infrastructure.Helpers;
using Samba.Presentation.Common;
using Samba.Presentation.Services;
using Samba.Services;

namespace Samba.Modules.ResourceModule.Widgets.ResourceGrid
{
    [Export(typeof(IWidgetCreator))]
    public class ResourceGridWidgetCreator : IWidgetCreator
    {
        private readonly IResourceService _resourceService;
        private readonly IUserService _userService;
        private readonly ICacheService _cacheService;

        [ImportingConstructor]
        public ResourceGridWidgetCreator(IResourceService resourceService, IUserService userService, ICacheService cacheService)
        {
            _resourceService = resourceService;
            _userService = userService;
            _cacheService = cacheService;
        }

        public string GetCreatorName()
        {
            return "ResourceGrid";
        }

        public string GetCreatorDescription()
        {
            return "Resource Grid";
        }

        public FrameworkElement CreateWidgetControl(IDiagram widget, ContextMenu contextMenu)
        {
            var viewModel = widget as ResourceGridWidgetViewModel;
            Debug.Assert(viewModel != null);

            if (widget.DesignMode)
            {
                viewModel.Refresh();
                viewModel.ResourceSelectorViewModel.ResourceScreenItems.ToList().ForEach(x => x.IsEnabled = false);
            }

            var ret = new ResourceSelectorView(viewModel.ResourceSelectorViewModel) { DataContext = viewModel.ResourceSelectorViewModel, ContextMenu = contextMenu, Tag = widget };

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
            var parameters = JsonHelper.Serialize(new ResourceGridWidgetSettings());
            return new Widget { Properties = parameters, CreatorName = GetCreatorName() };
        }

        public IDiagram CreateWidgetViewModel(Widget widget, IApplicationState applicationState)
        {
            return new ResourceGridWidgetViewModel(widget, applicationState, _resourceService, _userService, _cacheService);
        }
    }
}
