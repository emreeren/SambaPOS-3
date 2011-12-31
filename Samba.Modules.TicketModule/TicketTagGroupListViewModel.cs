using System.ComponentModel.Composition;
using Samba.Domain.Models.Tickets;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.TicketModule
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class TicketTagGroupListViewModel : EntityCollectionViewModelBase<TicketTagGroupViewModel, TicketTagGroup>
    {
    }
}
