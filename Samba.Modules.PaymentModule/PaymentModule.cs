using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Regions;
using Samba.Presentation.Common;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.PaymentModule
{
    [ModuleExport(typeof(PaymentModule))]
    class PaymentModule : VisibleModuleBase
    {
        private readonly IRegionManager _regionManager;
        private readonly PaymentEditorView _paymentEditorView;

        [ImportingConstructor]
        public PaymentModule(IRegionManager regionManager, PaymentEditorView paymentEditorView)
            : base(regionManager, AppScreens.Payment)
        {
            _regionManager = regionManager;
            _paymentEditorView = paymentEditorView;
        }

        public override object GetVisibleView()
        {
            return _paymentEditorView;
        }

        protected override void OnInitialization()
        {
            _regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(PaymentEditorView));

            EventServiceFactory.EventService.GetEvent<GenericEvent<EventAggregator>>().Subscribe(
             x =>
             {
                 if (x.Topic == EventTopicNames.MakePayment)
                 {
                     ((PaymentEditorViewModel)_paymentEditorView.DataContext).Prepare();
                     Activate();
                 }
             });
        }
    }
}
