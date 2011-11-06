using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Actions;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.SettingsModule
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
