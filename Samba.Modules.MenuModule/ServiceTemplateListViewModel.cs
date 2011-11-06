using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Tickets;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.MenuModule
{
    class ServiceTemplateListViewModel : EntityCollectionViewModelBase<ServiceTemplateViewModel, ServiceTemplate>
    {
        protected override ServiceTemplateViewModel CreateNewViewModel(ServiceTemplate model)
        {
            return new ServiceTemplateViewModel(model);
        }

        protected override ServiceTemplate CreateNewModel()
        {
            return new ServiceTemplate();
        }
    }
}
