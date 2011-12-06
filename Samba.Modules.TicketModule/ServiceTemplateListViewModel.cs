using Samba.Domain.Models.Menus;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.TicketModule
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
