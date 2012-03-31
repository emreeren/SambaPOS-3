using System.ComponentModel.Composition;
using Samba.Domain.Models.Resources;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Presentation.Common;
using Samba.Presentation.ViewModels;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.ResourceModule
{
    [Export]
    public class AccountEditorViewModel : ObservableObject
    {
        private readonly ICacheService _cacheService;
        public ICaptionCommand CloseScreenCommand { get; set; }
        public ICaptionCommand SaveAccountCommand { get; set; }
        public ICaptionCommand SelectAccountCommand { get; set; }

        [ImportingConstructor]
        public AccountEditorViewModel(ICacheService cacheService)
        {
            _cacheService = cacheService;
            CloseScreenCommand = new CaptionCommand<string>(Resources.Close, OnCloseScreen);
            SaveAccountCommand = new CaptionCommand<string>(Resources.Save, OnSaveAccount);
            SelectAccountCommand = new CaptionCommand<string>(Resources.SelectAccount.Replace(" ", "\r"), OnSelectAccount);
            EventServiceFactory.EventService.GetEvent<GenericEvent<EntityOperationRequest<Resource>>>().Subscribe(OnEditAccount);
        }

        private void OnSelectAccount(string obj)
        {
            SaveSelectedAccount();
            _operationRequest.Publish(SelectedAccount.Model);
        }

        private void OnSaveAccount(string obj)
        {
            SaveSelectedAccount();
            EventServiceFactory.EventService.PublishEvent(EventTopicNames.ActivateResourceView);
            //CommonEventPublisher.RequestNavigation(EventTopicNames.ActivateAccountView);
        }

        private void SaveSelectedAccount()
        {
            CustomDataViewModel.Update();
            Dao.Save(SelectedAccount.Model);
            //using (var ws = WorkspaceFactory.Create())
            //{
            //    if (!SelectedAccount.IsNotNew)
            //    {
            //        ws.Add(SelectedAccount.Model);
            //        ws.CommitChanges();

            //    }
            //    else
            //    {
            //        var result = ws.Single<Account>(
            //            x => x.Id == SelectedAccount.Id
            //                && x.Name == SelectedAccount.Name
            //                && x.CustomData == SelectedAccount.Model.CustomData);

            //        if (result == null)
            //        {
            //            result = ws.Single<Account>(x => x.Id == SelectedAccount.Id);
            //            Debug.Assert(result != null);
            //            result.Name = SelectedAccount.Name;
            //            result.CustomData = SelectedAccount.Model.CustomData;
            //            ws.CommitChanges();
            //        }
            //    }
            //}
        }

        private static void OnCloseScreen(string obj)
        {
            CommonEventPublisher.RequestNavigation(EventTopicNames.ActivateResourceView);
        }

        private EntityOperationRequest<Resource> _operationRequest;

        private void OnEditAccount(EventParameters<EntityOperationRequest<Resource>> obj)
        {
            if (obj.Topic == EventTopicNames.EditResourceDetails)
            {
                _operationRequest = obj.Value;
                var accountTemplate = _cacheService.GetResourceTemplateById(obj.Value.SelectedEntity.ResourceTemplateId);
                SelectedAccount = new ResourceSearchResultViewModel(obj.Value.SelectedEntity, accountTemplate);
                CustomDataViewModel = new ResourceCustomDataViewModel(obj.Value.SelectedEntity, accountTemplate);
                RaisePropertyChanged(() => CustomDataViewModel);
            }
        }

        private ResourceSearchResultViewModel _selectedAccount;
        public ResourceSearchResultViewModel SelectedAccount
        {
            get { return _selectedAccount; }
            set
            {
                _selectedAccount = value;
                RaisePropertyChanged(() => SelectedAccount);
            }
        }

        public ResourceCustomDataViewModel CustomDataViewModel { get; set; }

    }
}
