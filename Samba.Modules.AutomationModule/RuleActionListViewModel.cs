using Samba.Domain.Models.Actions;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.AutomationModule
{
    class RuleActionListViewModel: EntityCollectionViewModelBase<RuleActionViewModel, AppAction>
    {
        protected override RuleActionViewModel CreateNewViewModel(AppAction model)
        {
            return new RuleActionViewModel(model);
        }

        protected override AppAction CreateNewModel()
        {
            return new AppAction();
        }
    }
}
