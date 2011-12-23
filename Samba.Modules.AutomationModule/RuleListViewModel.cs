using Samba.Domain.Models.Actions;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.AutomationModule
{
    class RuleListViewModel : EntityCollectionViewModelBase<RuleViewModel, AppRule>
    {
        protected override RuleViewModel CreateNewViewModel(AppRule model)
        {
            return new RuleViewModel(model);
        }

        protected override AppRule CreateNewModel()
        {
            return new AppRule();
        }
    }
}
