using System.Linq;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.TicketModule
{
    public class TicketTagGroupListViewModel : EntityCollectionViewModelBase<TicketTagGroupViewModel, TicketTagGroup>
    {
        protected override TicketTagGroupViewModel CreateNewViewModel(TicketTagGroup model)
        {
            return new TicketTagGroupViewModel(model);
        }

        protected override TicketTagGroup CreateNewModel()
        {
            return new TicketTagGroup();
        }

        protected override string CanDeleteItem(TicketTagGroup model)
        {
            var count = Dao.Query<TicketTemplate>(x => x.TicketTagGroups.Select(y => y.Id).Contains(model.Id), x => x.TicketTagGroups).Count();
            if (count > 0) return Resources.DeleteErrorTagUsedInDepartment;
            return base.CanDeleteItem(model);
        }
    }
}
