using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Regions;
using Samba.Domain.Models.Accounts;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Services;

namespace Samba.Modules.CashModule
{
    [ModuleExport(typeof(CashModule))]
    public class CashModule : VisibleModuleBase
    {
        private readonly IRegionManager _regionManager;
        private readonly CashView _cashView;
        private readonly IWorkPeriodService _workPeriodService;
        private readonly IUserService _userService;
        public ICategoryCommand NavigateCashViewCommand { get; set; }

        [ImportingConstructor]
        public CashModule(IRegionManager regionManager, CashView cashView,IWorkPeriodService workPeriodService,IUserService userService)
            : base(regionManager, AppScreens.CashView)
        {
            _regionManager = regionManager;
            _cashView = cashView;
            _workPeriodService = workPeriodService;
            _userService = userService;
            SetNavigationCommand(Resources.Drawer, Resources.Common, "images/Xls.png", 70);
            PermissionRegistry.RegisterPermission(PermissionNames.NavigateCashView, PermissionCategories.Navigation, Resources.CanNavigateCash);
            PermissionRegistry.RegisterPermission(PermissionNames.MakeCashTransaction, PermissionCategories.Cash, Resources.CanMakeCashTransaction);
        }

        protected override bool CanNavigate(string arg)
        {
            return _userService.IsUserPermittedFor(PermissionNames.NavigateCashView) && _workPeriodService.CurrentWorkPeriod != null;
        }

        protected override void OnNavigate(string obj)
        {
            base.OnNavigate(obj);
            ((CashViewModel)_cashView.DataContext).SelectedAccount = null;
            ((CashViewModel)_cashView.DataContext).ActivateTransactionList();
        }

        public override object GetVisibleView()
        {
            return _cashView;
        }

        protected override void OnInitialization()
        {
            _regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(CashView));

            EventServiceFactory.EventService.GetEvent<GenericEvent<Account>>().Subscribe(
                x =>
                {
                    if (x.Topic == EventTopicNames.MakePaymentToAccount)
                    {
                        Activate();
                        ((CashViewModel)_cashView.DataContext).MakePaymentToAccount(x.Value);
                    }

                    if (x.Topic == EventTopicNames.GetPaymentFromAccount)
                    {
                        Activate();
                        ((CashViewModel)_cashView.DataContext).GetPaymentFromAccount(x.Value);
                    }

                    if (x.Topic == EventTopicNames.AddLiabilityAmount)
                    {
                        Activate();
                        ((CashViewModel)_cashView.DataContext).AddLiabilityAmount(x.Value);
                    }

                    if (x.Topic == EventTopicNames.AddReceivableAmount)
                    {
                        Activate();
                        ((CashViewModel)_cashView.DataContext).AddReceivableAmount(x.Value);
                    }
                });
        }
    }
}
