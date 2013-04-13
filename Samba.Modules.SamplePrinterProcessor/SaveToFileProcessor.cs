using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using Samba.Domain.Models.Tickets;
using Samba.Presentation.Common.Services;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.SamplePrinterProcessor
{
    [Export(typeof(IPrinterProcessor))]
    class SaveToFileProcessor : IPrinterProcessor
    {
        private readonly ISettingService _settingService;
        private SaveToFileProcessorSettings _settings;

        [ImportingConstructor]
        public SaveToFileProcessor(ISettingService settingService)
        {
            _settingService = settingService;
        }

        protected SaveToFileProcessorSettings Settings
        {
            get { return _settings ?? (_settings = new SaveToFileProcessorSettings(_settingService)); }
        }

        public string Name { get { return "SaveToFile"; } }


        public string[] Process(Ticket ticket, IList<Order> orders, string[] formattedLines)
        {
            var fileName = Settings.FileName;
            if (!string.IsNullOrEmpty(fileName))
            {
                File.WriteAllLines(fileName, formattedLines);
            }
            return null;
        }

        public void EditSettings()
        {
            InteractionService.UserIntraction.EditProperties(Settings);
        }
    }
}
