using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Tickets;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.TicketModule
{
    class TicketTemplateListViewModel : EntityCollectionViewModelBase<TicketTemplateViewModel, TicketTemplate>
    {
        protected override TicketTemplateViewModel CreateNewViewModel(TicketTemplate model)
        {
            return new TicketTemplateViewModel(model);
        }

        protected override TicketTemplate CreateNewModel()
        {
            return new TicketTemplate();
        }
    }
}
