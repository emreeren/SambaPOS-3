using System.ComponentModel.Composition;
using System.Linq;
using Samba.Localization.Properties;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.AccountModule.ActionProcessors
{
    [Export(typeof(IActionType))]
    class PrintAccountTransactionDocument : ActionType
    {
        private readonly IAccountService _accountService;
        private readonly ICacheService _cacheService;
        private readonly IApplicationState _applicationState;
        private readonly IPrinterService _printerService;

        [ImportingConstructor]
        public PrintAccountTransactionDocument(IAccountService accountService, ICacheService cacheService, IApplicationState applicationState,
            IPrinterService printerService)
        {
            _accountService = accountService;
            _cacheService = cacheService;
            _applicationState = applicationState;
            _printerService = printerService;
        }

        protected override object GetDefaultData()
        {
            return new { DocumentId = 0, PrinterName = "", PrinterTemplateName = "" };
        }

        protected override string GetActionName()
        {
            return Resources.PrintAccountTransactionDocument;
        }

        protected override string GetActionKey()
        {
            return ActionNames.PrintAccountTransactionDocument;
        }

        public override void Process(ActionData actionData)
        {
            var documentId = actionData.GetAsInteger("DocumentId");
            var document = _accountService.GetAccountTransactionDocumentById(documentId);
            if (document == null) return;
            var printerName = actionData.GetAsString("PrinterName");
            var printerTemplateName = actionData.GetAsString("PrinterTemplateName");
            var printer = _cacheService.GetPrinters().FirstOrDefault(x => x.Name == printerName);
            var printerTemplate = _cacheService.GetPrinterTemplates().FirstOrDefault(y => y.Name == printerTemplateName);
            if (printer == null)
            {
                printer = _applicationState.GetTransactionPrinter();
            }
            if (printerTemplate == null)
            {
                var documentType = _cacheService.GetAccountTransactionDocumentTypeById(document.DocumentTypeId);
                printerTemplate = _cacheService.GetPrinterTemplates().First(x => x.Id == documentType.PrinterTemplateId);
            }
            if (printer == null) return;
            if (printerTemplate == null) return;
            _printerService.PrintObject(document, printer, printerTemplate);
        }
    }
}
