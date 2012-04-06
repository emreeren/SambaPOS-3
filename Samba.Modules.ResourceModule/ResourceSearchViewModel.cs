using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Timers;
using System.Windows.Input;
using System.Windows.Threading;
using Samba.Domain.Models.Resources;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Presentation.Common;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.ResourceModule
{
    [Export]
    public class ResourceSearchViewModel : ObservableObject
    {
        public event EventHandler SelectedResourceTemplateChanged;

        private void InvokeSelectedResourceTemplateChanged(EventArgs e)
        {
            var handler = SelectedResourceTemplateChanged;
            if (handler != null) handler(this, e);
        }

        private EntityOperationRequest<Resource> _currentResourceSelectionRequest;

        public ICaptionCommand CloseScreenCommand { get; set; }
        public ICaptionCommand SelectResourceCommand { get; set; }
        public ICaptionCommand CreateResourceCommand { get; set; }
        public ICaptionCommand EditResourceCommand { get; set; }
        public ICaptionCommand DisplayAccountCommand { get; set; }

        private readonly IApplicationState _applicationState;
        private readonly ICacheService _cacheService;

        [ImportingConstructor]
        public ResourceSearchViewModel(IApplicationState applicationState, ICacheService cacheService)
        {
            _updateTimer = new Timer(500);
            _updateTimer.Elapsed += UpdateTimerElapsed;

            _applicationState = applicationState;
            _cacheService = cacheService;

            FoundResources = new ObservableCollection<ResourceSearchResultViewModel>();

            CloseScreenCommand = new CaptionCommand<string>(Resources.Close, OnCloseScreen);
            SelectResourceCommand = new CaptionCommand<string>(string.Format(Resources.Select_f, Resources.Resource).Replace(" ", "\r"), OnSelectResource, CanSelectResource);
            EditResourceCommand = new CaptionCommand<string>(string.Format(Resources.Edit_f, Resources.Resource).Replace(" ", "\r"), OnEditResource, CanEditResource);
            CreateResourceCommand = new CaptionCommand<string>(string.Format(Resources.New_f, Resources.Resource).Replace(" ", "\r"), OnCreateResource, CanCreateResource);
            DisplayAccountCommand = new CaptionCommand<string>(Resources.AccountDetails.Replace(" ", "\r"), OnDisplayAccount, CanDisplayAccount);
        }

        public IEnumerable<ResourceTemplate> ResourceTemplates { get { return _cacheService.GetResourceTemplates(); } }

        private ResourceTemplate _selectedResourceTemplate;
        public ResourceTemplate SelectedResourceTemplate
        {
            get { return _selectedResourceTemplate; }
            set
            {
                _selectedResourceTemplate = value;
                ClearSearchValues();
                RaisePropertyChanged(() => SelectedResourceTemplate);
                InvokeSelectedResourceTemplateChanged(EventArgs.Empty);
            }
        }

        private readonly Timer _updateTimer;
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

        public bool IsCloseButtonVisible { get { return _applicationState.CurrentDepartment != null; } }

        private void OnDisplayAccount(string obj)
        {
            CommonEventPublisher.PublishEntityOperation(SelectedResource.Model, EventTopicNames.DisplayAccountTransactions);
            ClearSearchValues();
        }

        private bool CanEditResource(string arg)
        {
            return SelectedResource != null;
        }

        private void OnEditResource(string obj)
        {
            var targetEvent = _currentResourceSelectionRequest != null
                                  ? _currentResourceSelectionRequest.GetExpectedEvent()
                                  : EventTopicNames.ResourceSelected;

            CommonEventPublisher.PublishEntityOperation(SelectedResource.Model,
                EventTopicNames.EditResourceDetails, targetEvent);
        }

        private bool CanCreateResource(string arg)
        {
            return SelectedResourceTemplate != null;
        }

        private void OnCreateResource(string obj)
        {
            var targetEvent = _currentResourceSelectionRequest != null
                                  ? _currentResourceSelectionRequest.GetExpectedEvent()
                                  : EventTopicNames.ResourceSelected;

            ClearSearchValues();
            CommonEventPublisher.PublishEntityOperation(new Resource { ResourceTemplateId = SelectedResourceTemplate.Id },
                EventTopicNames.EditResourceDetails, targetEvent);
        }

        private bool CanSelectResource(string arg)
        {
            return
                _applicationState.IsCurrentWorkPeriodOpen
                && _applicationState.CurrentDepartment != null
                && SelectedResource != null
                && !string.IsNullOrEmpty(SelectedResource.Name);
        }

        private bool CanDisplayAccount(string arg)
        {
            return SelectedResource != null;
        }

        private void OnSelectResource(string obj)
        {
            if (_currentResourceSelectionRequest != null)
            {
                _currentResourceSelectionRequest.Publish(SelectedResource.Model);
            }
            CommonEventPublisher.RequestNavigation(EventTopicNames.ActivateOpenTickets);
            ClearSearchValues();
        }

        private static void OnCloseScreen(string obj)
        {
            CommonEventPublisher.RequestNavigation(EventTopicNames.ActivateOpenTickets);
        }

        public void RefreshSelectedResource(EntityOperationRequest<Resource> value)
        {
            if (value != null && value.SelectedEntity != null)
            {
                if (SelectedResourceTemplate == null ||
                    SelectedResourceTemplate.Id != value.SelectedEntity.ResourceTemplateId)
                    SelectedResourceTemplate = _cacheService.GetResourceTemplateById(value.SelectedEntity.ResourceTemplateId);

                ClearSearchValues();
            }
            else if (_applicationState.CurrentDepartment != null)
            {
                var tid = _applicationState.CurrentDepartment.TicketTemplate.SaleTransactionTemplate.TargetAccountTemplateId;
                SelectedResourceTemplate = _cacheService.GetResourceTemplateById(tid);
            }

            _currentResourceSelectionRequest = value;

            if (_currentResourceSelectionRequest != null && _currentResourceSelectionRequest.SelectedEntity != null && !string.IsNullOrEmpty(_currentResourceSelectionRequest.SelectedEntity.Name))
            {
                ClearSearchValues();
                FoundResources.Add(new ResourceSearchResultViewModel(_currentResourceSelectionRequest.SelectedEntity, SelectedResourceTemplate));
            }

            RaisePropertyChanged(() => SelectedResourceTemplate);
            RaisePropertyChanged(() => SelectedResource);
            RaisePropertyChanged(() => IsCloseButtonVisible);
            RaisePropertyChanged(() => ResourceTemplates);
        }

        private void ClearSearchValues()
        {
            FoundResources.Clear();
            SearchString = "";
        }

        private void ResetTimer()
        {
            _updateTimer.Stop();

            if (!string.IsNullOrEmpty(SearchString))
            {
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
            IEnumerable<Resource> result = new List<Resource>();

            using (var worker = new BackgroundWorker())
            {
                worker.DoWork += delegate
                {
                    var defaultResourceId =
                        _applicationState.CurrentDepartment != null ? _applicationState.CurrentDepartment.TicketTemplate.SaleTransactionTemplate.DefaultTargetAccountId : 0;

                    var templateId = SelectedResourceTemplate != null ? SelectedResourceTemplate.Id : 0;

                    result = Dao.Query<Resource>(x =>
                        x.ResourceTemplateId == templateId
                        && x.Id != defaultResourceId
                        && (x.CustomData.Contains(SearchString) || x.Name.Contains(SearchString)));

                    result = result.ToList().Where(x => SelectedResourceTemplate.GetMatchingFields(x, SearchString).Any(y => !y.Hidden) || x.Name.ToLower().Contains(SearchString));
                };

                worker.RunWorkerCompleted +=
                    delegate
                    {

                        AppServices.MainDispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(
                               delegate
                               {
                                   FoundResources.Clear();
                                   FoundResources.AddRange(result.Select(x => new ResourceSearchResultViewModel(x, SelectedResourceTemplate)));

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

    }
}
