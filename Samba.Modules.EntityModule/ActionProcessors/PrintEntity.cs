using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Entities;
using Samba.Localization.Properties;
using Samba.Presentation.Services.Common;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.EntityModule.ActionProcessors
{
    [Export(typeof(IActionType))]
    class PrintEntity : ActionType
    {
        private readonly ICacheService _cacheService;
        private readonly IPrinterService _printerService;
        private readonly IEntityService _entityService;

        [ImportingConstructor]
        public PrintEntity(ICacheService cacheService, IPrinterService printerService,IEntityService entityService)
        {
            _cacheService = cacheService;
            _printerService = printerService;
            _entityService = entityService;
        }

        protected override object GetDefaultData()
        {
            return new { EntityId = 0, EntityName = "", PrinterName = "", PrinterTemplateName = "" };
        }

        protected override string GetActionName()
        {
            return string.Format(Resources.Print_f, Resources.Entity);
        }

        protected override string GetActionKey()
        {
            return ActionNames.PrintEntity;
        }

        public override void Process(ActionData actionData)
        {
            var entityId = actionData.GetAsInteger("EntityId");
            var entity = entityId > 0 ? _cacheService.GetEntityById(entityId) : actionData.GetDataValue<Entity>("Entity");
            if (entity == null) return;
            var printerName = actionData.GetAsString("PrinterName");
            var printerTemplateName = actionData.GetAsString("PrinterTemplateName");
            var printer = _cacheService.GetPrinters().FirstOrDefault(x => x.Name == printerName);
            var printerTemplate = _cacheService.GetPrinterTemplates().FirstOrDefault(y => y.Name == printerTemplateName);
            if (printer == null) return;
            if (printerTemplate == null) return;
            _printerService.PrintObject(entity, printer, printerTemplate);
        }
    }
}
