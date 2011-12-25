using System.ComponentModel.Composition;
using Samba.Domain.Models.Settings;
using Samba.Presentation.Common.ModelBase;
using Samba.Services;

namespace Samba.Modules.AutomationModule
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    class TriggerListViewModel : EntityCollectionViewModelBase<TriggerViewModel, Trigger>
    {
        private readonly ITriggerService _triggerService;

        [ImportingConstructor]
        public TriggerListViewModel(ITriggerService triggerService)
        {
            _triggerService = triggerService;
        }

        protected override void OnDeleteItem(object obj)
        {
            base.OnDeleteItem(obj);
            MethodQueue.Queue("UpdateCronObjects", _triggerService.UpdateCronObjects);
        }
    }
}
