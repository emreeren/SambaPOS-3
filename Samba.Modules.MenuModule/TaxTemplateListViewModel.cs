using System;
using Samba.Domain.Models.Menus;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.MenuModule
{
    class TaxTemplateListViewModel : EntityCollectionViewModelBase<TaxTemplateViewModel, TaxTemplate>
    {
        protected override TaxTemplateViewModel CreateNewViewModel(TaxTemplate model)
        {
            return new TaxTemplateViewModel(model);
        }

        protected override TaxTemplate CreateNewModel()
        {
            return new TaxTemplate();
        }
    }
}
