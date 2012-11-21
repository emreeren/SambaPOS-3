using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Timers;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Practices.Prism.Commands;
using Samba.Domain.Models.Resources;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
using Samba.Services;

namespace Samba.Modules.ResourceModule
{
    [Export]
    public class ResourceSearchViewModel : ObservableObject
    {
        public event EventHandler SelectedResourceTypeChanged;

        private void InvokeSelectedResourceTypeChanged(EventArgs e)
        {
            var handler = SelectedResourceTypeChanged;
            if (handler != null) handler(this, e);
        }

        private EntityOperationRequest<Resource> _currentResourceSelectionRequest;

        public DelegateCommand<string> SelectResourceCommand { get; set; }
        public DelegateCommand<string> CreateResourceCommand { get; set; }
        public DelegateCommand<string> EditResourceCommand { get; set; }
        public DelegateCommand<string> RemoveResourceCommand { get; set; }
        public ICaptionCommand DisplayAccountCommand { get; set; }

        public string SelectResourceCommandCaption { get { return string.Format(Resources.Select_f, SelectedEntityName()).Replace(" ", "\r"); } }
        public string CreateResourceCommandCaption { get { return string.Format(Resources.New_f, SelectedEntityName()).Replace(" ", "\r"); } }
        public string EditResourceCommandCaption { get { return string.Format(Resources.Edit_f, SelectedEntityName()).Replace(" ", "\r"); } }
        public string RemoveResourceCommandCaption { get { return string.Format(Resources.No_f, SelectedEntityName()).Replace(" ", "\r"); } }
        private readonly IApplicationState _applicationState;
        private readonly ICacheService _cacheService;
        private readonly IResourceService _resourceService;
        private readonly Timer _updateTimer;

        private bool _canCreateNewResource;
        public bool CanCreateNewResource
        {
            get { return _canCreateNewResource; }
            set
            {
                _canCreateNewResource = value;
                RaisePropertyChanged(() => CanCreateNewResource);
            }
        }

        [ImportingConstructor]
        public ResourceSearchViewModel(IApplicationState applicationState, ICacheService cacheService, IResourceService resourceService)
        {
            _updateTimer = new Timer(500);
            _updateTimer.Elapsed += UpdateTimerElapsed;

            _applicationState = applicationState;
            _cacheService = cacheService;
            _resourceService = resourceService;

            IsKeyboardVisible = true;
            FoundResources = new ObservableCollection<ResourceSearchResultViewModel>();

            SelectResourceCommand = new CaptionCommand<string>("", OnSelectResource, CanSelectResource);
            EditResourceCommand = new CaptionCommand<string>("", OnEditResource, CanEditResource);
            CreateResourceCommand = new CaptionCommand<string>("", OnCreateResource, CanCreateResource);
            DisplayAccountCommand = new CaptionCommand<string>(Resources.AccountDetails.Replace(" ", "\r"), OnDisplayAccount, CanDisplayAccount);
            RemoveResourceCommand = new CaptionCommand<string>("", OnRemoveResource, CanRemoveResource);
        }

        protected int StateFilter { get; set; }
        public bool IsKeyboardVisible { get; set; }

        public IEnumerable<ResourceType> ResourceTypes { get { return _cacheService.GetResourceTypes(); } }

        private ResourceType _selectedResourceType;
        public ResourceType SelectedResourceType
        {
            get { return _selectedResourceType; }
            set
            {
                _selectedResourceType = value;
                ClearSearchValues();
                RaisePropertyChanged(() => SelectedResourceType);
                RaisePropertyChanged(() => SelectResourceCommandCaption);
                RaisePropertyChanged(() => CreateResourceCommandCaption);
                RaisePropertyChanged(() => EditResourceCommandCaption);
                RaisePropertyChanged(() => RemoveResourceCommandCaption);
                InvokeSelectedResourceTypeChanged(EventArgs.Empty);
            }
        }

        private string SelectedEntityName()
        {
            return SelectedResourceType != null ? SelectedResourceType.EntityName : Resources.Resource;
        }

        public ObservableCollection<ResourceSearchResultViewModel> FoundResources { get; set; }

        public ResourceSearchResultViewModel SelectedResource
        {
            get
            {
                return FoundResources.Count == 1 ? FoundResources[0] : FocusedResource;
            }
        }

        private ResourceSearchResultViewModel _focusedResource;
        public ResourceSearchResultViewModel FocusedResource
        {
            get { return _focusedResource; }
            set
            {
                _focusedResource = value;
                RaisePropertyChanged(() => FocusedResource);
                RaisePropertyChanged(() => SelectedResource);
            }
        }

        private string _searchString;
        public string SearchString
        {
            get { return string.IsNullOrEmpty(_searchString) ? null : _searchString.TrimStart('+', '0'); }
            set
            {
                if (value != _searchString)
                {
                    _searchString = value;
                    RaisePropertyChanged(() => SearchString);
                    ResetTimer();
                }
            }
        }

        private void OnDisplayAccount(string obj)
        {
            CommonEventPublisher.PublishEntityOperation(new AccountData { AccountId = SelectedResource.Model.AccountId }, EventTopicNames.DisplayAccountTransactions, EventTopicNames.SelectResource);
        }

        private bool CanDisplayAccount(string arg)
        {
            return SelectedResource != null && SelectedResource.Model.AccountId > 0;
        }

        private bool CanEditResource(string arg)
        {
            return SelectedResource != null;
        }

        private void OnEditResource(string obj)
        {
            var targetEvent = _currentResourceSelectionRequest != null
                                  ? _currentResourceSelectionRequest.GetExpectedEvent()
                                  : EventTopicNames.SelectResource;

            CommonEventPublisher.PublishEntityOperation(SelectedResource.Model,
                EventTopicNames.EditResourceDetails, targetEvent);
        }

        private void OnRemoveResource(string obj)
        {
            _currentResourceSelectionRequest.Publish(Resource.GetNullResource(SelectedResourceType.Id));
        }

        private bool CanRemoveResource(string arg)
        {
            return _applicationState.IsLocked && _currentResourceSelectionRequest != null &&
                   _currentResourceSelectionRequest.SelectedEntity != null &&
                   ((SelectedResource != null && _currentResourceSelectionRequest.SelectedEntity.Id == SelectedResource.Id) ||
                    _currentResourceSelectionRequest.SelectedEntity.Id == 0);
        }

        private bool CanCreateResource(string arg)
        {
            return SelectedResourceType != null && CanCreateNewResource;
        }

        private void OnCreateResource(string obj)
        {
            var targetEvent = _currentResourceSelectionRequest != null
                                  ? _currentResourceSelectionRequest.GetExpectedEvent()
                                  : EventTopicNames.SelectResource;
            var newResource = new Resource { ResourceTypeId = SelectedResourceType.Id, Name = SearchString };
            ClearSearchValues();
            CommonEventPublisher.PublishEntityOperation(newResource, EventTopicNames.EditResourceDetails, targetEvent);
        }

        private bool CanSelectResource(string arg)
        {
            return
                _applicationState.IsCurrentWorkPeriodOpen
                && _applicationState.CurrentDepartment != null
                && SelectedResource != null
                && !string.IsNullOrEmpty(SelectedResource.Name);
        }

        private void OnSelectResource(string obj)
        {
            if (_currentResourceSelectionRequest != null)
            {
                _currentResourceSelectionRequest.Publish(SelectedResource.Model);
            }
            ClearSearchValues();
        }

        public void RefreshSelectedResource(EntityOperationRequest<Resource> value)
        {
            ClearSearchValues();
            _currentResourceSelectionRequest = value;

            if (_currentResourceSelectionRequest != null && _currentResourceSelectionRequest.SelectedEntity != null && !string.IsNullOrEmpty(_currentResourceSelectionRequest.SelectedEntity.Name))
            {
                ClearSearchValues();
                if (_currentResourceSelectionRequest.SelectedEntity.Name != "*")
                {
                    FoundResources.Add(new ResourceSearchResultViewModel(_currentResourceSelectionRequest.SelectedEntity, SelectedResourceType));
                }
            }

            RaisePropertyChanged(() => SelectedResourceType);
            RaisePropertyChanged(() => SelectedResource);
            RaisePropertyChanged(() => ResourceTypes);
        }

        private void ClearSearchValues()
        {
            FoundResources.Clear();
            SearchString = "";
        }

        private void ResetTimer()
        {
            CanCreateNewResource = true;
            _updateTimer.Stop();
            if (!string.IsNullOrEmpty(SearchString))
            {
                CanCreateNewResource = false;
                _updateTimer.Start();
            }
            else FoundResources.Clear();
        }

        void UpdateTimerElapsed(object sender, ElapsedEventArgs e)
        {
            _updateTimer.Stop();
            UpdateFoundResources();
        }

        private void UpdateFoundResources()
        {
            CanCreateNewResource = true;
            var result = new List<Resource>();

            using (var worker = new BackgroundWorker())
            {
                worker.DoWork += delegate
                                     {
                                         result = _resourceService.SearchResources(SearchString, SelectedResourceType, StateFilter);
                                     };

                worker.RunWorkerCompleted +=
                    delegate
                    {

                        AppServices.MainDispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(
                               delegate
                               {
                                   FoundResources.Clear();
                                   FoundResources.AddRange(result.Select(x => new ResourceSearchResultViewModel(x, SelectedResourceType)));

                                   if (SelectedResource != null && SearchString == SelectedResource.PhoneNumber)
                                   {
                                       SelectedResource.UpdateDetailedInfo();
                                   }

                                   RaisePropertyChanged(() => SelectedResource);
                                   CommandManager.InvalidateRequerySuggested();
                               }));

                    };

                worker.RunWorkerAsync();
            }
        }

        public void Refresh(int resourceType, int stateFilter, EntityOperationRequest<Resource> currentOperationRequest)
        {
            StateFilter = stateFilter;
            SelectedResourceType = _cacheService.GetResourceTypeById(resourceType);
            RefreshSelectedResource(currentOperationRequest);
        }
    }
}
