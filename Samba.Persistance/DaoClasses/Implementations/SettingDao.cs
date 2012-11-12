using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Data;
using Samba.Localization.Properties;
using Samba.Persistance.Data;

namespace Samba.Persistance.DaoClasses.Implementations
{
    [Export(typeof(ISettingDao))]
    class SettingDao : ISettingDao
    {
        [ImportingConstructor]
        public SettingDao()
        {
            ValidatorRegistry.RegisterDeleteValidator(new NumeratorDeleteValidator());
        }

        public IEnumerable<TaxTemplate> GetTaxTemplates()
        {
            return Dao.Query<TaxTemplate>();
        }

        public IEnumerable<CalculationType> GetCalculationTypes()
        {
            return Dao.Query<CalculationType>();
        }

        public string GetNextString(int numeratorId)
        {
            using (var workspace = WorkspaceFactory.Create())
            {
                var numerator = workspace.Single<Numerator>(x => x.Id == numeratorId);
                numerator.Number++;
                try
                {
                    workspace.CommitChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    return GetNextString(numeratorId);
                }
                return numerator.GetNumber();
            }
        }

        public int GetNextNumber(int numeratorId)
        {
            using (var workspace = WorkspaceFactory.Create())
            {
                var numerator = workspace.Single<Numerator>(x => x.Id == numeratorId);
                numerator.Number++;
                try
                {
                    workspace.CommitChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    return GetNextNumber(numeratorId);
                }
                return numerator.Number;
            }
        }

        public IEnumerable<Terminal> GetTerminals()
        {
            return Dao.Query<Terminal>(x => x.ReportPrinter);
        }
    }

    internal class NumeratorDeleteValidator : SpecificationValidator<Numerator>
    {
        public override string GetErrorMessage(Numerator model)
        {
            if (Dao.Exists<TicketType>(x => x.OrderNumerator.Id == model.Id))
                return string.Format(Resources.DeleteErrorUsedBy_f, Resources.Numerator, Resources.TicketType);
            if (Dao.Exists<TicketType>(x => x.TicketNumerator.Id == model.Id))
                return string.Format(Resources.DeleteErrorUsedBy_f, Resources.Numerator, Resources.TicketType);
            return "";
        }
    }
}
