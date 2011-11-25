using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Tickets;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.TicketModule
{
    class OrderTagTemplateListViewModel : EntityCollectionViewModelBase<OrderTagTemplateViewModel, OrderTagTemplate>
    {
        protected override OrderTagTemplateViewModel CreateNewViewModel(OrderTagTemplate model)
        {
            return new OrderTagTemplateViewModel(model);
        }

        protected override OrderTagTemplate CreateNewModel()
        {
            return new OrderTagTemplate();
        }
    }
}
