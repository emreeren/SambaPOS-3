using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text.RegularExpressions;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Data;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Services.Common;

namespace Samba.Services.Implementations.SettingsModule
{
    [Export(typeof(ISettingService))]
    class SettingService : AbstractService, ISettingService
    {
        [ImportingConstructor]
        public SettingService()
        {
            ValidatorRegistry.RegisterDeleteValidator(new NumeratorDeleteValidator());
        }

        private readonly ProgramSettings _globalSettings = new ProgramSettings();

        private static IWorkspace _workspace;
        public static IWorkspace Workspace
        {
            get { return _workspace ?? (_workspace = WorkspaceFactory.Create()); }
            set { _workspace = value; }
        }

        public IProgramSetting ReadSetting(string settingName)
        {
            return _globalSettings.ReadSetting(settingName);
        }

        public ISettingReplacer GetSettingReplacer()
        {
            return new SettingReplacer(_globalSettings);
        }

        private static IEnumerable<Terminal> _terminals;
        public static IEnumerable<Terminal> Terminals { get { return _terminals ?? (_terminals = Workspace.All<Terminal>()); } }

        private IEnumerable<TaxTemplate> _taxTemplates;
        public IEnumerable<TaxTemplate> TaxTemplates
        {
            get { return _taxTemplates ?? (_taxTemplates = Dao.Query<TaxTemplate>()); }
        }

        private IEnumerable<ServiceTemplate> _serviceTemplates;
        public IEnumerable<ServiceTemplate> ServiceTemplates
        {
            get { return _serviceTemplates ?? (_serviceTemplates = Dao.Query<ServiceTemplate>()); }
        }

        public ServiceTemplate GetServiceTemplateById(int id)
        {
            return ServiceTemplates.FirstOrDefault(x => x.Id == id);
        }

        public ServiceTemplate GetServiceTemplateByName(string name)
        {
            return ServiceTemplates.FirstOrDefault(y => y.Name == name);
        }

        public TaxTemplate GetTaxTemplateById(int id)
        {
            return TaxTemplates.FirstOrDefault(x => x.Id == id);
        }

        public TaxTemplate GetTaxTemplateByName(string name)
        {
            return TaxTemplates.FirstOrDefault(y => y.Name == name);
        }

        public Terminal GetTerminalByName(string name)
        {
            return Terminals.SingleOrDefault(x => x.Name == name);
        }

        public Terminal GetDefaultTerminal()
        {
            return Terminals.SingleOrDefault(x => x.IsDefault);
        }

        public IEnumerable<string> GetTerminalNames()
        {
            return Terminals.Select(x => x.Name);
        }

        public IProgramSettings ProgramSettings
        {
            get { return _globalSettings; }
        }

        public IProgramSetting GetProgramSetting(string settingName)
        {
            return _globalSettings.GetSetting(settingName);
        }

        public void SaveProgramSettings()
        {
            _globalSettings.SaveChanges();
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

        public override void Reset()
        {
            _taxTemplates = null;
            _serviceTemplates = null;
            _terminals = null;
            _globalSettings.ResetCache();
            Workspace = WorkspaceFactory.Create();
        }
    }

    internal class NumeratorDeleteValidator : SpecificationValidator<Numerator>
    {
        public override string GetErrorMessage(Numerator model)
        {
            if (Dao.Exists<TicketTemplate>(x => x.OrderNumerator.Id == model.Id))
                return Resources.DeleteErrorNumeratorIsOrderNumerator;
            if (Dao.Exists<TicketTemplate>(x => x.TicketNumerator.Id == model.Id))
                return Resources.DeleteErrorNumeratorIsTicketNumerator;
            if (Dao.Exists<TicketTagGroup>(x => x.Numerator.Id == model.Id))
                return Resources.DeleteErrorNumeratorUsedInTicket;
            return "";
        }
    }
}
