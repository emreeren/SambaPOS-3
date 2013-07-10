using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Entities;
using Samba.Infrastructure.Messaging;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Services;
using Samba.Presentation.Common.Widgets;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
using Samba.Services;

namespace Samba.Modules.EntityModule
{
    [Export]
    public class EntityDashboardViewModel : ObservableObject
    {
        private readonly IApplicationState _applicationState;
        private readonly IEntityService _entityService;

        [ImportingConstructor]
        public EntityDashboardViewModel(IApplicationState applicationState, IEntityService entityService)
        {
            _applicationState = applicationState;
            _entityService = entityService;

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
            if (viewModel != null && InteractionService.UserIntraction.AskQuestion("Delete Widget?"))
            {
                Widgets.Remove(viewModel);
                _entityService.RemoveWidget(viewModel.GetWidget());
                RaisePropertyChanged(() => Widgets);
                SaveTrackableEntityScreenItems();
                LoadTrackableEntityScreenItems();
            }
        }

        public EntityScreen SelectedEntityScreen { get { return _applicationState.SelectedEntityScreen; } }
        public bool CanDesignEntityScreenItems { get { return _applicationState.CurrentLoggedInUser.UserRole.IsAdmin; } }

        private bool _isDesignModeActive;
        private EntityScreen _currentEntityScreen;

        public bool IsDesignModeActive
        {
            get { return _isDesignModeActive; }
            set
            {
                _isDesignModeActive = value;
                RaisePropertyChanged(() => IsDesignModeActive);
            }
        }

        public ObservableCollection<IDiagram> Widgets { get; set; }

        public void Refresh(EntityScreen entityScreen, OperationRequest<Entity> currentOperationRequest)
        {
            _entityService.UpdateEntityScreen(entityScreen);
            if (_currentEntityScreen != entityScreen || Widgets == null)
            {
                _currentEntityScreen = entityScreen;
                Widgets = new ObservableCollection<IDiagram>(entityScreen.Widgets.Select(x => WidgetCreatorRegistry.CreateWidgetViewModel(x, _applicationState)));
            }
            Widgets.Where(x => x.AutoRefresh).ToList().ForEach(x => x.Refresh());
            RaisePropertyChanged(() => Widgets);
            RaisePropertyChanged(() => SelectedEntityScreen);
        }

        public void AddWidget(string creatorName)
        {
            var widget = WidgetCreatorRegistry.CreateWidgetFor(creatorName);
            _entityService.AddWidgetToEntityScreen(SelectedEntityScreen.Name, widget);
            widget.Height = 100;
            widget.Width = 100;
            widget.AutoRefresh = true;
            Widgets.Add(WidgetCreatorRegistry.CreateWidgetViewModel(widget, _applicationState));
        }

        public void LoadTrackableEntityScreenItems()
        {
            IsDesignModeActive = true;
            Widgets = new ObservableCollection<IDiagram>(_entityService.LoadWidgets(SelectedEntityScreen.Name).Select(x => WidgetCreatorRegistry.CreateWidgetViewModel(x, _applicationState)));
            Widgets.ToList().ForEach(x => x.DesignMode = true);
            RaisePropertyChanged(() => Widgets);
        }

        public void SaveTrackableEntityScreenItems()
        {
            _applicationState.ResetState();
            EventServiceFactory.EventService.PublishEvent(EventTopicNames.ResetCache, true);
            IsDesignModeActive = false;
            Widgets.ToList().ForEach(x => x.SaveSettings());
            Widgets.ToList().ForEach(x => x.DesignMode = false);
            _entityService.SaveEntityScreenItems();
            Widgets.ToList().ForEach(x => x.Refresh());
            RaisePropertyChanged(() => Widgets);
        }
    }
}
