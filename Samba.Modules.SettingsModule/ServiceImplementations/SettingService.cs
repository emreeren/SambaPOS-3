using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Menus;
using Samba.Persistance.Data;
using Samba.Presentation.Common.Services;
using Samba.Services;

namespace Samba.Modules.SettingsModule.ServiceImplementations
{
    [Export(typeof(ISettingService))]
    class SettingService : AbstractService, ISettingService
    {
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

        public override void Reset()
        {
            _taxTemplates = null;
            _serviceTemplates = null;
        }

    }
}
