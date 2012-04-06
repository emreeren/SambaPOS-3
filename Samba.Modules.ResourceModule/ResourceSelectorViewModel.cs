using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using Microsoft.Practices.Prism.Commands;
using Samba.Domain.Models.Resources;
using Samba.Infrastructure;
using Samba.Localization.Properties;
using Samba.Modules.LocationModule;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Services;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.ResourceModule
{
    [Export]
    public class ResourceSelectorViewModel : ObservableObject
    {
        public DelegateCommand<ResourceButtonViewModel> ResourceSelectionCommand { get; set; }
        public DelegateCommand<ResourceScreen> SelectResourceCategoryCommand { get; set; }
        public ICaptionCommand CloseScreenCommand { get; set; }
        public ICaptionCommand EditSelectedResourceScreenPropertiesCommand { get; set; }
        public ICaptionCommand IncPageNumberCommand { get; set; }
        public ICaptionCommand DecPageNumberCommand { get; set; }

        public ObservableCollection<IDiagram> ResourceScreenItems { get; set; }

        public ResourceScreen SelectedResourceScreen { get { return _applicationState.SelectedResourceScreen; } }
        public IEnumerable<ResourceScreen> ResourceScreens { get { return _applicationState.CurrentDepartment != null ? _applicationState.CurrentDepartment.LocationScreens : null; } }

        public bool CanDesignResourceScreenItems { get { return _applicationState.CurrentLoggedInUser.UserRole.IsAdmin; } }
        public int CurrentPageNo { get; set; }

        public bool IsPageNavigatorVisible { get { return SelectedResourceScreen != null && SelectedResourceScreen.PageCount > 1; } }
        public bool IsFeedbackVisible { get { return !string.IsNullOrEmpty(Feedback); } }
        private string _feedback;
        public string Feedback
        {
            get { return _feedback; }
            set
            {
                _feedback = value;
                RaisePropertyChanged(() => Feedback);
                RaisePropertyChanged(() => IsFeedbackVisible);
            }
        }

        private string _feedbackColor;
        public string FeedbackColor
        {
            get { return _feedbackColor; }
            set { _feedbackColor = value; RaisePropertyChanged(() => FeedbackColor); }
        }

        private string _feedbackForeground;
        public string FeedbackForeground
        {
            get { return _feedbackForeground; }
            set
            {
                _feedbackForeground = value;
                RaisePropertyChanged(() => FeedbackForeground);
            }
        }

        public VerticalAlignment ScreenVerticalAlignment { get { return SelectedResourceScreen != null && SelectedResourceScreen.ButtonHeight > 0 ? VerticalAlignment.Top : VerticalAlignment.Stretch; } }

        private readonly IApplicationState _applicationState;
        private readonly IResourceService _resourceService;
        private readonly IUserService _userService;
        private readonly IApplicationStateSetter _applicationStateSetter;
        private readonly ICacheService _cacheService;
        private EntityOperationRequest<ResourceScreenItem> _currentOperationRequest;

        [ImportingConstructor]
        public ResourceSelectorViewModel(IApplicationState applicationState, IApplicationStateSetter applicationStateSetter,
            IResourceService resourceService, IUserService userService, ICacheService cacheService)
        {
            _applicationState = applicationState;
            _applicationStateSetter = applicationStateSetter;
            _resourceService = resourceService;
            _userService = userService;
            _cacheService = cacheService;
            SelectResourceCategoryCommand = new DelegateCommand<ResourceScreen>(OnSelectResourceCategoryExecuted);
            ResourceSelectionCommand = new DelegateCommand<ResourceButtonViewModel>(OnSelectResourceExecuted);
            CloseScreenCommand = new CaptionCommand<string>(Resources.Close, OnCloseScreenExecuted);
            EditSelectedResourceScreenPropertiesCommand = new CaptionCommand<string>(Resources.Properties, OnEditSelectedResourceScreenProperties, CanEditSelectedResourceScreenProperties);
            IncPageNumberCommand = new CaptionCommand<string>(Resources.NextPage + " >>", OnIncPageNumber, CanIncPageNumber);
            DecPageNumberCommand = new CaptionCommand<string>("<< " + Resources.PreviousPage, OnDecPageNumber, CanDecPageNumber);

            EventServiceFactory.EventService.GetEvent<GenericEvent<Message>>().Subscribe(
                x =>
                {
                    if (_applicationState.ActiveAppScreen == AppScreens.LocationList
                        && x.Topic == EventTopicNames.MessageReceivedEvent
                        && x.Value.Command == Messages.TicketRefreshMessage)
                    {
                        RefreshResourceScreenItems();
                    }
                });

            EventServiceFactory.EventService.GetEvent<GenericEvent<EntityOperationRequest<ResourceScreenItem>>>().Subscribe(
                x =>
                {
                    if (x.Topic == EventTopicNames.SelectLocation)
                    {
                        _currentOperationRequest = x.Value;
                        UpdateResourceScreenItems(_applicationState.SelectedResourceScreen ?? _applicationState.CurrentDepartment.LocationScreens[0]);
                    }
                });
        }

        public void RefreshResourceScreenItems()
        {
            if (SelectedResourceScreen == null && ResourceScreens.Count() > 0)
                _applicationStateSetter.SetSelectedResourceScreen(ResourceScreens.First());
            if (SelectedResourceScreen != null)
                UpdateResourceScreenItems(SelectedResourceScreen);
        }

        private bool CanDecPageNumber(string arg)
        {
            return SelectedResourceScreen != null && CurrentPageNo > 0;
        }

        private void OnDecPageNumber(string obj)
        {
            CurrentPageNo--;
            RefreshResourceScreenItems();
        }

        private bool CanIncPageNumber(string arg)
        {
            return SelectedResourceScreen != null && CurrentPageNo < SelectedResourceScreen.PageCount - 1;
        }

        private void OnIncPageNumber(string obj)
        {
            CurrentPageNo++;
            RefreshResourceScreenItems();
        }

        private bool CanEditSelectedResourceScreenProperties(string arg)
        {
            return SelectedResourceScreen != null;
        }

        private void OnEditSelectedResourceScreenProperties(string obj)
        {
            if (SelectedResourceScreen != null)
                InteractionService.UserIntraction.EditProperties(SelectedResourceScreen);
        }

        private void OnCloseScreenExecuted(string obj)
        {
            _applicationState.CurrentDepartment.PublishEvent(EventTopicNames.ActivateOpenTickets);
        }

        private void OnSelectResourceCategoryExecuted(ResourceScreen obj)
        {
            UpdateResourceScreenItems(obj);
        }

        private void OnSelectResourceExecuted(ResourceButtonViewModel obj)
        {
            //var location = new LocationData
            //                   {
            //                       LocationId = obj.Model.Id,
            //                       LocationName = obj.Model.Name,
            //                       TicketId = obj.Model.TicketId,
            //                       Caption = obj.Caption
            //                   };
            //location.PublishEvent(EventTopicNames.LocationSelectedForTicket);

            _currentOperationRequest.Publish(obj.Model);
        }

        private void UpdateResourceScreenItems(ResourceScreen resourceScreen)
        {
            Feedback = "";
            var resourceData = _resourceService.GetCurrentResourceScreenItems(resourceScreen, CurrentPageNo).OrderBy(x => x.Order).ToList();

            if (ResourceScreenItems != null && (ResourceScreenItems.Count() == 0 || ResourceScreenItems.Count != resourceData.Count() || ResourceScreenItems.First().Caption != resourceData.First().Name)) ResourceScreenItems = null;

            if (ResourceScreenItems == null)
            {
                ResourceScreenItems = new ObservableCollection<IDiagram>();
                ResourceScreenItems.AddRange(resourceData.Select(x =>
                    new ResourceButtonViewModel(x,
                        SelectedResourceScreen,
                        ResourceSelectionCommand,
                        _currentOperationRequest.SelectedEntity != null,
                        _userService.IsUserPermittedFor(PermissionNames.MergeTickets), _cacheService.GetResourceStateById(x.ResourceStateId))));
            }
            else
            {
                for (var i = 0; i < resourceData.Count(); i++)
                {
                    var acs = ((ResourceButtonViewModel)ResourceScreenItems[i]).AccountState;
                    if (acs == null || acs.Id != resourceData.ElementAt(i).ResourceStateId)
                        ((ResourceButtonViewModel)ResourceScreenItems[i]).AccountState =
                            _cacheService.GetResourceStateById(resourceData.ElementAt(i).ResourceStateId);

                    ((ResourceButtonViewModel)ResourceScreenItems[i]).Model = resourceData.ElementAt(i);
                }
            }

            if (_currentOperationRequest.SelectedEntity != null)
            {
                FeedbackColor = "Red";
                FeedbackForeground = "White";
                Feedback = string.Format(Resources.SelectLocationThatYouWantToMoveTicket_f, _currentOperationRequest.SelectedEntity.Name);
            }
            else
            {
                FeedbackColor = "LightYellow";
                FeedbackForeground = "Black";
                Feedback = Resources.SelectLocationForOperation;
            }

            RaisePropertyChanged(() => ResourceScreenItems);
            RaisePropertyChanged(() => ResourceScreens);
            RaisePropertyChanged(() => SelectedResourceScreen);
            RaisePropertyChanged(() => IsPageNavigatorVisible);
            RaisePropertyChanged(() => ScreenVerticalAlignment);
        }

        public void LoadTrackableResourceScreenItems()
        {
            ResourceScreenItems = new ObservableCollection<IDiagram>(
                _resourceService.LoadResourceScreenItems(SelectedResourceScreen.Name)
                .Select<ResourceScreenItem, IDiagram>(x => new ResourceButtonViewModel(x, SelectedResourceScreen)));
            RaisePropertyChanged(() => ResourceScreenItems);
        }

        public void SaveTrackableResourceScreenItems()
        {
            _resourceService.SaveResourceScreenItems();
            UpdateResourceScreenItems(SelectedResourceScreen);
        }
    }
}
