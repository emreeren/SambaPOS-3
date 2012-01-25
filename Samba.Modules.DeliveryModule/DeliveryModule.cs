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

        [ImportingConstructor]
        public DeliveryModule(IRegionManager regionManager, AccountSelectorView accountSelectorView)
            : base(regionManager, AppScreens.AccountList)
        {
            _regionManager = regionManager;
            _accountSelectorView = accountSelectorView;
        }

        public override object GetVisibleView()
        {
            return _accountSelectorView;
        }

        protected override void OnInitialization()
        {
            _regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(AccountSelectorView));
            _regionManager.RegisterViewWithRegion(RegionNames.AccountSearchRegion, typeof(AccountSearcherView));
            _regionManager.RegisterViewWithRegion(RegionNames.NewAccountRegion, typeof(AccountEditorView));

            EventServiceFactory.EventService.GetEvent<GenericEvent<Department>>().Subscribe(x =>
            {
                if (x.Topic == EventTopicNames.SelectAccount)
                {
                    Activate();
                    ((AccountSelectorViewModel)_accountSelectorView.DataContext).RefreshSelectedAccount();
                }
            });

            EventServiceFactory.EventService.GetEvent<GenericEvent<EventAggregator>>().Subscribe(x =>
            {
                if (x.Topic == EventTopicNames.ActivateAccountView) Activate();
                ((AccountSelectorViewModel)_accountSelectorView.DataContext).RefreshSelectedAccount();
            });

            EventServiceFactory.EventService.GetEvent<GenericEvent<Account>>().Subscribe(x =>
            {
                if (x.Topic == EventTopicNames.ActivateAccount)
                {
                    Activate();
                    ((AccountSelectorViewModel)_accountSelectorView.DataContext).DisplayAccount(x.Value);
                }
            });

            EventServiceFactory.EventService.GetEvent<GenericEvent<PopupData>>().Subscribe(
                x =>
                {
                    if (x.Topic == EventTopicNames.PopupClicked && x.Value.EventMessage == EventTopicNames.SelectAccount)
                    {
                        Activate();
                        ((AccountSelectorViewModel)_accountSelectorView.DataContext).SearchAccount(x.Value.DataObject as string);
                    }
                });
        }
    }
}
