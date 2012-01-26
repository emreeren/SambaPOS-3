using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Regions;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Tickets;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Interaction;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.DeliveryModule
{
    [ModuleExport(typeof(DeliveryModule))]
    public class DeliveryModule : VisibleModuleBase
    {
        private readonly IRegionManager _regionManager;
        private readonly AccountSelectorView _accountSelectorView;
        private readonly AccountEditorView _accountEditorView;
        private readonly DeliveryView _deliveryView;
        private readonly AccountTransactionsView _accountTransactionsView;

        [ImportingConstructor]
        public DeliveryModule(IRegionManager regionManager, DeliveryView deliveryView,
            AccountSelectorView accountSelectorView,
            AccountEditorView accountEditorView,
            AccountTransactionsView accountTransactionsView)
            : base(regionManager, AppScreens.AccountList)
        {
            _regionManager = regionManager;
            _accountSelectorView = accountSelectorView;
            _deliveryView = deliveryView;
            _accountTransactionsView = accountTransactionsView;
            _accountEditorView = accountEditorView;
        }

        public override object GetVisibleView()
        {
            return _deliveryView;
        }

        protected override void OnInitialization()
        {
            _regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(DeliveryView));
            _regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(AccountTransactionsView));
            _regionManager.RegisterViewWithRegion(RegionNames.AccountDisplayRegion, typeof(AccountSelectorView));
            _regionManager.RegisterViewWithRegion(RegionNames.AccountDisplayRegion, typeof(AccountEditorView));

            EventServiceFactory.EventService.GetEvent<GenericEvent<Department>>().Subscribe(x =>
            {
                if (x.Topic == EventTopicNames.SelectAccount)
                    ActivateAccountSelector();
            });

            EventServiceFactory.EventService.GetEvent<GenericEvent<EventAggregator>>().Subscribe(x =>
            {
                if (x.Topic == EventTopicNames.ActivateAccountView)
                    ActivateAccountSelector();
            });

            EventServiceFactory.EventService.GetEvent<GenericEvent<Account>>().Subscribe(x =>
            {
                if (x.Topic == EventTopicNames.DisplayAccountTransactions)
                    ActivateAccountTransactions();
                else if (x.Topic == EventTopicNames.EditAccountDetails)
                    ActivateAccountEditor();
            });

            EventServiceFactory.EventService.GetEvent<GenericEvent<PopupData>>().Subscribe(
                x =>
                {
                    if (x.Topic == EventTopicNames.PopupClicked && x.Value.EventMessage == EventTopicNames.SelectAccount)
                    {
                        Activate();
                        ((DeliveryViewModel)_accountSelectorView.DataContext).SearchAccount(x.Value.DataObject as string);
                    }
                });
        }

        private void ActivateAccountEditor()
        {
            Activate();
            _regionManager.Regions[RegionNames.AccountDisplayRegion].Activate(_accountEditorView);
        }

        private void ActivateAccountTransactions()
        {
            _regionManager.Regions[RegionNames.MainRegion].Activate(_accountTransactionsView);
        }

        private void ActivateAccountSelector()
        {
            Activate();
            ((AccountSelectorViewModel)_accountSelectorView.DataContext).RefreshSelectedAccount();
            _regionManager.Regions[RegionNames.AccountDisplayRegion].Activate(_accountSelectorView);
        }
    }
}
