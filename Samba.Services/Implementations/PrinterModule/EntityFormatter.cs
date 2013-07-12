using System;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Entities;
using Samba.Domain.Models.Settings;
using Samba.Services.Implementations.PrinterModule.ValueChangers;

namespace Samba.Services.Implementations.PrinterModule
{
    [Export]
    public class EntityFormatter
    {
        private readonly IExpressionService _expressionService;
        private readonly ISettingService _settingService;
        private readonly EntityValueChanger _valueChanger;

        [ImportingConstructor]
        public EntityFormatter(IExpressionService expressionService, ISettingService settingService,
            EntityValueChanger valueChanger)
        {
            _expressionService = expressionService;
            _settingService = settingService;
            _valueChanger = valueChanger;
        }

        public string[] GetFormattedDocument(Entity entity, PrinterTemplate printerTemplate)
        {
            var content = _valueChanger.GetValue(printerTemplate, entity);
            content = UpdateExpressions(content);
            return content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
        }

        private string UpdateExpressions(string data)
        {
            data = _expressionService.ReplaceExpressionValues(data);
            data = _settingService.ReplaceSettingValues(data);
            return data;
        }
    }
}