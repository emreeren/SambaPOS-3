using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Resources;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Widgets;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.ResourceModule
{
    [Export]
    public class ResourceDashboardViewModel : ObservableObject
    {
        private readonly IApplicationState _applicationState;
        private readonly IResourceService _resourceService;

        [ImportingConstructor]
        public ResourceDashboardViewModel(IApplicationState applicationState, IResourceService resourceService)
        {
            _applicationState = applicationState;
            _resourceService = resourceService;
        }

        public void RemoveWidget(WidgetViewModel viewModel)
        {
            if (viewModel != null)
            {
                Widgets.Remove(viewModel);
                _resourceService.RemoveWidget(viewModel.Model);
                RaisePropertyChanged(() => Widgets);
                SaveTrackableResourceScreenItems();
                LoadTrackableResourceScreenItems();
            }
        }

        public ResourceScreen SelectedResourceScreen { get { return _applicationState.SelectedResourceScreen; } }
        public bool CanDesignResourceScreenItems { get { return _applicationState.CurrentLoggedInUser.UserRole.IsAdmin; } }
        public ObservableCollection<IDiagram> Widgets { get; set; }

        public void Refresh(ResourceScreen resourceScreen, EntityOperationRequest<Resource> currentOperationRequest)
        {
            _resourceService.UpdateResourceScreen(resourceScreen);
            Widgets = new ObservableCollection<IDiagram>(resourceScreen.Widgets.Select(WidgetCreatorRegistry.CreateWidgetViewModel));
            Widgets.ToList().ForEach(x => x.Refresh());
            RaisePropertyChanged(() => Widgets);
        }

        public void AddWidget()
        {
            var widget = WidgetCreatorRegistry.CreateWidgetFor("Resource Button");
            _resourceService.AddWidgetToResourceScreen(SelectedResourceScreen.Name, widget);
            widget.Name = "New Widget";
            widget.Height = 100;
            widget.Width = 100;
            Widgets.Add(WidgetCreatorRegistry.CreateWidgetViewModel(widget));
        }

        public void LoadTrackableResourceScreenItems()
        {
            Widgets = new ObservableCollection<IDiagram>(_resourceService.LoadWidgets(SelectedResourceScreen.Name).Select(WidgetCreatorRegistry.CreateWidgetViewModel));
            RaisePropertyChanged(() => Widgets);
        }

        public void SaveTrackableResourceScreenItems()
        {
            Widgets.ToList().ForEach(x => x.SaveSettings());
            _resourceService.SaveResourceScreenItems();
            Widgets.ToList().ForEach(x => x.Refresh());
        }
    }
}
