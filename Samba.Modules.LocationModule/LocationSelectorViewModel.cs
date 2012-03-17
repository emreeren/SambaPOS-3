using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using Microsoft.Practices.Prism.Commands;
using Samba.Domain.Models.Accounts;
using Samba.Infrastructure;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Services;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.LocationModule
{
    [Export]
    public class LocationSelectorViewModel : ObservableObject
    {
        public DelegateCommand<AccountButtonViewModel> LocationSelectionCommand { get; set; }
        public DelegateCommand<AccountScreen> SelectLocationCategoryCommand { get; set; }
        public ICaptionCommand CloseScreenCommand { get; set; }
        public ICaptionCommand EditSelectedLocationScreenPropertiesCommand { get; set; }
        public ICaptionCommand IncPageNumberCommand { get; set; }
        public ICaptionCommand DecPageNumberCommand { get; set; }

        public ObservableCollection<IDiagram> Locations { get; set; }

        public AccountScreen SelectedLocationScreen { get { return _applicationState.SelectedLocationScreen; } }
        public IEnumerable<AccountScreen> LocationScreens { get { return _applicationState.CurrentDepartment != null ? _applicationState.CurrentDepartment.LocationScreens : null; } }

        public bool CanDesignLocations { get { return _applicationState.CurrentLoggedInUser.UserRole.IsAdmin; } }
        public int CurrentPageNo { get; set; }

        public bool IsPageNavigatorVisible { get { return SelectedLocationScreen != null && SelectedLocationScreen.PageCount > 1; } }
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

        public VerticalAlignment LocationsVerticalAlignment { get { return SelectedLocationScreen != null && SelectedLocationScreen.ButtonHeight > 0 ? VerticalAlignment.Top : VerticalAlignment.Stretch; } }

        private readonly IApplicationState _applicationState;
        private readonly ILocationService _locationService;
        private readonly IUserService _userService;
        private readonly IApplicationStateSetter _applicationStateSetter;
        private readonly ICacheService _cacheService;
        private EntityOperationRequest<AccountScreenItem> _currentOperationRequest;

        [ImportingConstructor]
        public LocationSelectorViewModel(IApplicationState applicationState, IApplicationStateSetter applicationStateSetter,
            ILocationService locationService, IUserService userService, ICacheService cacheService)
        {
            _applicationState = applicationState;
            _applicationStateSetter = applicationStateSetter;
            _locationService = locationService;
            _userService = userService;
            _cacheService = cacheService;
            SelectLocationCategoryCommand = new DelegateCommand<AccountScreen>(OnSelectLocationCategoryExecuted);
            LocationSelectionCommand = new DelegateCommand<AccountButtonViewModel>(OnSelectLocationExecuted);
            CloseScreenCommand = new CaptionCommand<string>(Resources.Close, OnCloseScreenExecuted);
            EditSelectedLocationScreenPropertiesCommand = new CaptionCommand<string>(Resources.Properties, OnEditSelectedLocationScreenProperties, CanEditSelectedLocationScreenProperties);
            IncPageNumberCommand = new CaptionCommand<string>(Resources.NextPage + " >>", OnIncPageNumber, CanIncPageNumber);
            DecPageNumberCommand = new CaptionCommand<string>("<< " + Resources.PreviousPage, OnDecPageNumber, CanDecPageNumber);

            EventServiceFactory.EventService.GetEvent<GenericEvent<Message>>().Subscribe(
                x =>
                {
                    if (_applicationState.ActiveAppScreen == AppScreens.LocationList
                        && x.Topic == EventTopicNames.MessageReceivedEvent
                        && x.Value.Command == Messages.TicketRefreshMessage)
                    {
                        RefreshLocations();
                    }
                });

            EventServiceFactory.EventService.GetEvent<GenericEvent<EntityOperationRequest<AccountScreenItem>>>().Subscribe(
                x =>
                {
                    if (x.Topic == EventTopicNames.SelectLocation)
                    {
                        _currentOperationRequest = x.Value;
                        UpdateLocations(_applicationState.SelectedLocationScreen ?? _applicationState.CurrentDepartment.LocationScreens[0]);
                    }
                });
        }

        public void RefreshLocations()
        {
            if (SelectedLocationScreen == null && LocationScreens.Count() > 0)
                _applicationStateSetter.SetSelectedLocationScreen(LocationScreens.First());
            if (SelectedLocationScreen != null)
                UpdateLocations(SelectedLocationScreen);
        }

        private bool CanDecPageNumber(string arg)
        {
            return SelectedLocationScreen != null && CurrentPageNo > 0;
        }

        private void OnDecPageNumber(string obj)
        {
            CurrentPageNo--;
            RefreshLocations();
        }

        private bool CanIncPageNumber(string arg)
        {
            return SelectedLocationScreen != null && CurrentPageNo < SelectedLocationScreen.PageCount - 1;
        }

        private void OnIncPageNumber(string obj)
        {
            CurrentPageNo++;
            RefreshLocations();
        }

        private bool CanEditSelectedLocationScreenProperties(string arg)
        {
            return SelectedLocationScreen != null;
        }

        private void OnEditSelectedLocationScreenProperties(string obj)
        {
            if (SelectedLocationScreen != null)
                InteractionService.UserIntraction.EditProperties(SelectedLocationScreen);
        }

        private void OnCloseScreenExecuted(string obj)
        {
            _applicationState.CurrentDepartment.PublishEvent(EventTopicNames.ActivateOpenTickets);
        }

        private void OnSelectLocationCategoryExecuted(AccountScreen obj)
        {
            UpdateLocations(obj);
        }

        private void OnSelectLocationExecuted(AccountButtonViewModel obj)
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

        private void UpdateLocations(AccountScreen locationScreen)
        {
            Feedback = "";
            var locationData = _locationService.GetCurrentLocations(locationScreen, CurrentPageNo).OrderBy(x => x.Order).ToList();

            if (Locations != null && (Locations.Count() == 0 || Locations.Count != locationData.Count() || Locations.First().Caption != locationData.First().Name)) Locations = null;

            if (Locations == null)
            {
                Locations = new ObservableCollection<IDiagram>();
                Locations.AddRange(locationData.Select(x =>
                    new AccountButtonViewModel(x,
                        SelectedLocationScreen,
                        LocationSelectionCommand,
                        _currentOperationRequest.SelectedEntity != null,
                        _userService.IsUserPermittedFor(PermissionNames.MergeTickets), _cacheService.GetAccountStateById(x.AccountStateId))));
            }
            else
            {
                for (var i = 0; i < locationData.Count(); i++)
                {
                    var acs = ((AccountButtonViewModel)Locations[i]).AccountState;
                    if (acs == null || acs.Id != locationData.ElementAt(i).AccountStateId)
                        ((AccountButtonViewModel)Locations[i]).AccountState =
                            _cacheService.GetAccountStateById(locationData.ElementAt(i).AccountStateId);

                    ((AccountButtonViewModel)Locations[i]).Model = locationData.ElementAt(i);
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

            RaisePropertyChanged(() => Locations);
            RaisePropertyChanged(() => LocationScreens);
            RaisePropertyChanged(() => SelectedLocationScreen);
            RaisePropertyChanged(() => IsPageNavigatorVisible);
            RaisePropertyChanged(() => LocationsVerticalAlignment);
        }

        public void LoadTrackableLocations()
        {
            Locations = new ObservableCollection<IDiagram>(
                _locationService.LoadLocations(SelectedLocationScreen.Name)
                .Select<AccountScreenItem, IDiagram>(x => new AccountButtonViewModel(x, SelectedLocationScreen)));
            RaisePropertyChanged(() => Locations);
        }

        public void SaveTrackableLocations()
        {
            _locationService.SaveLocations();
            UpdateLocations(SelectedLocationScreen);
        }
    }
}
