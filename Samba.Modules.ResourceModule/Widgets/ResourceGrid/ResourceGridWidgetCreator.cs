using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Samba.Domain.Models.Resources;
using Samba.Presentation.Common;
using Samba.Services;

namespace Samba.Modules.ResourceModule.Widgets.ResourceGrid
{
    [Export(typeof(IWidgetCreator))]
    public class ResourceGridWidgetCreator : IWidgetCreator
    {
        private readonly IApplicationState _applicationState;
        private readonly IApplicationStateSetter _applicationStateSetter;
        private readonly IResourceService _resourceService;
        private readonly IUserService _userService;
        private readonly ICacheService _cacheService;

        [ImportingConstructor]
        public ResourceGridWidgetCreator(IApplicationState applicationState, IApplicationStateSetter applicationStateSetter,
            IResourceService resourceService, IUserService userService, ICacheService cacheService)
        {
            _applicationState = applicationState;
            _applicationStateSetter = applicationStateSetter;
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
            if (widget.DesignMode)
            {
                viewModel.Refresh();
                viewModel.ResourceSelectorViewModel.ResourceScreenItems.ToList().ForEach(x => x.IsEnabled = false);
            }

            var ret = new ResourceSelectorView(viewModel.ResourceSelectorViewModel) { DataContext = viewModel.ResourceSelectorViewModel, ContextMenu = contextMenu };

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
            return new Widget { CreatorName = GetCreatorName() };
        }

        public IDiagram CreateWidgetViewModel(Widget widget)
        {
            return new ResourceGridWidgetViewModel(widget, _applicationState, _applicationStateSetter, _resourceService, _userService, _cacheService);
        }
    }
}
