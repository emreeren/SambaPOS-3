using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using Samba.Domain.Models.Accounts;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Presentation.Common;
using Samba.Presentation.ViewModels;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.AccountModule
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
            EventServiceFactory.EventService.GetEvent<GenericEvent<EntityOperationRequest<Account>>>().Subscribe(OnEditAccount);
        }

        private void OnSelectAccount(string obj)
        {
            SaveSelectedAccount();
            _operationRequest.Publish(SelectedAccount.Model);
        }

        private void OnSaveAccount(string obj)
        {
            SaveSelectedAccount();
            EventServiceFactory.EventService.PublishEvent(EventTopicNames.ActivateAccountView);
            //CommonEventPublisher.RequestNavigation(EventTopicNames.ActivateAccountView);
        }

        private void SaveSelectedAccount()
        {
            CustomDataViewModel.Update();
            Dao.SafeSave(SelectedAccount.Model);
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
            CommonEventPublisher.RequestNavigation(EventTopicNames.ActivateAccountView);
        }

        private EntityOperationRequest<Account> _operationRequest;

        private void OnEditAccount(EventParameters<EntityOperationRequest<Account>> obj)
        {
            if (obj.Topic == EventTopicNames.EditAccountDetails)
            {
                _operationRequest = obj.Value;
                var accountTemplate = _cacheService.GetAccountTemplateById(obj.Value.SelectedEntity.AccountTemplateId);
                SelectedAccount = new AccountSearchViewModel(obj.Value.SelectedEntity, accountTemplate);
                CustomDataViewModel = new AccountCustomDataViewModel(obj.Value.SelectedEntity, accountTemplate);
                RaisePropertyChanged(() => CustomDataViewModel);
            }
        }

        private AccountSearchViewModel _selectedAccount;
        public AccountSearchViewModel SelectedAccount
        {
            get { return _selectedAccount; }
            set
            {
                _selectedAccount = value;
                RaisePropertyChanged(() => SelectedAccount);
            }
        }

        public AccountCustomDataViewModel CustomDataViewModel { get; set; }

    }
}
