using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.SettingsModule
{
    public class NumeratorListViewModel : EntityCollectionViewModelBase<NumeratorViewModel, Numerator>
    {
        protected override NumeratorViewModel CreateNewViewModel(Numerator model)
        {
            return new NumeratorViewModel(model);
        }

        protected override Numerator CreateNewModel()
        {
            return new Numerator();
        }

        protected override string CanDeleteItem(Numerator model)
        {
            var count = Dao.Count<TicketTemplate>(x => x.OrderNumerator.Id == model.Id);
            if (count > 0) return Resources.DeleteErrorNumeratorIsOrderNumerator;
            count = Dao.Count<TicketTemplate>(x => x.TicketNumerator.Id == model.Id);
            if (count > 0) return Resources.DeleteErrorNumeratorIsTicketNumerator;
            count = Dao.Count<TicketTagGroup>(x => x.Numerator.Id == model.Id);
            if (count > 0) return Resources.DeleteErrorNumeratorUsedInTicket;
            return base.CanDeleteItem(model);
        }
    }
}
