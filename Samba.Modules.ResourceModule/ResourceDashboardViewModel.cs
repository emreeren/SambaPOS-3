using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Resources;
using Samba.Infrastructure;
using Samba.Infrastructure.Messaging;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Widgets;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;

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

            EventServiceFactory.EventService.GetEvent<GenericEvent<Message>>().Subscribe(
            x =>
            {
                if (x.Topic == EventTopicNames.MessageReceivedEvent && x.Value.Command == Messages.WidgetRefreshMessage)
                {
                    Widgets.Where(y => y.IsVisible && y.CreatorName == x.Value.Data).ToList().ForEach(y => y.Refresh());
                }
            });

        }

        public void RemoveWidget(IDiagram viewModel)
        {
            if (viewModel != null)
            {
                Widgets.Remove(viewModel);
                _resourceService.RemoveWidget(viewModel.GetWidget());
                RaisePropertyChanged(() => Widgets);
                SaveTrackableResourceScreenItems();
                LoadTrackableResourceScreenItems();
            }
        }

        public ResourceScreen SelectedResourceScreen { get { return _applicationState.SelectedResourceScreen; } }
        public bool CanDesignResourceScreenItems { get { return _applicationState.CurrentLoggedInUser.UserRole.IsAdmin; } }

        private bool _isDesignModeActive;
        private ResourceScreen _currentResourceScreen;

        public bool IsDesignModeActive
        {
            get { return _isDesignModeActive; }
            set { _isDesignModeActive = value; RaisePropertyChanged(() => IsDesignModeActive); }
        }

        public ObservableCollection<IDiagram> Widgets { get; set; }

        public void Refresh(ResourceScreen resourceScreen, EntityOperationRequest<Resource> currentOperationRequest)
        {
            _resourceService.UpdateResourceScreen(resourceScreen);
            if (_currentResourceScreen != resourceScreen || Widgets == null)
            {
                _currentResourceScreen = resourceScreen;
                Widgets = new ObservableCollection<IDiagram>(resourceScreen.Widgets.Select(x => WidgetCreatorRegistry.CreateWidgetViewModel(x, _applicationState)));
            }
            Widgets.Where(x => x.AutoRefresh).ToList().ForEach(x => x.Refresh());
            RaisePropertyChanged(() => Widgets);
        }

        public void AddWidget(string creatorName)
        {
            var widget = WidgetCreatorRegistry.CreateWidgetFor(creatorName);
            _resourceService.AddWidgetToResourceScreen(SelectedResourceScreen.Name, widget);
            widget.Height = 100;
            widget.Width = 100;
            widget.AutoRefresh = true;
            Widgets.Add(WidgetCreatorRegistry.CreateWidgetViewModel(widget, _applicationState));
        }

        public void LoadTrackableResourceScreenItems()
        {
            IsDesignModeActive = true;
            Widgets = new ObservableCollection<IDiagram>(_resourceService.LoadWidgets(SelectedResourceScreen.Name).Select(x => WidgetCreatorRegistry.CreateWidgetViewModel(x, _applicationState)));
            Widgets.ToList().ForEach(x => x.DesignMode = true);
            RaisePropertyChanged(() => Widgets);
        }

        public void SaveTrackableResourceScreenItems()
        {
            _applicationState.ResetState();
            EventServiceFactory.EventService.PublishEvent(EventTopicNames.ResetCache, true);
            IsDesignModeActive = false;
            Widgets.ToList().ForEach(x => x.SaveSettings());
            Widgets.ToList().ForEach(x => x.DesignMode = false);
            _resourceService.SaveResourceScreenItems();
            Widgets.Where(x => x.AutoRefresh).ToList().ForEach(x => x.Refresh());
            RaisePropertyChanged(() => Widgets);
        }
    }
}
