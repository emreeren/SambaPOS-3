using Samba.Domain.Models.Tickets;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.TicketModule
{
    public class OrderTagGroupListViewModel : EntityCollectionViewModelBase<OrderTagGroupViewModel, OrderTagGroup>
    {
        protected override OrderTagGroupViewModel CreateNewViewModel(OrderTagGroup model)
        {
            return new OrderTagGroupViewModel(model);
        }

        protected override OrderTagGroup CreateNewModel()
        {
            return new OrderTagGroup();
        }
    }
}
