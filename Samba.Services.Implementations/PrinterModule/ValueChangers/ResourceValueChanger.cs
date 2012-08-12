using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;

namespace Samba.Services.Implementations.PrinterModule.ValueChangers
{
    public class ResourceValueChanger : AbstractValueChanger<TicketResource>
    {
        public override string GetTargetTag()
        {
            return "RESOURCES";
        }

        protected override string GetModelName(TicketResource model)
        {
            var resourceTemplate = CacheService.GetResourceTemplateById(model.ResourceTemplateId);
            return resourceTemplate == null ? "" : resourceTemplate.EntityName;
        }

        protected override string ReplaceValues(string templatePart, TicketResource model, PrinterTemplate template)
        {
            var result = templatePart;
            if (model != null)
            {
                result = FormatData(result, "{RESOURCE NAME}", () => model.ResourceName);
                result = FormatDataIf(model.AccountId > 0, result, "{RESOURCE BALANCE}", () => AccountService.GetAccountBalance(model.AccountId).ToString("#,#0.00"));
                if (result.Contains("{RESOURCE DATA:"))
                {
                    const string resourceDataPattern = "{RESOURCE DATA:" + "[^}]+}";
                    while (Regex.IsMatch(result, resourceDataPattern))
                    {
                        var value = Regex.Match(result, resourceDataPattern).Groups[0].Value;
                        try
                        {
                            var tag = value.Trim('{', '}').Split(':')[1];
                            result = FormatData(result.Trim('\r'), value, () => string.Join("\r", tag.Split(',').Select(x => model.GetCustomDataFormat(x, x + ": {0}"))));
                        }
                        catch (Exception)
                        {
                            result = FormatData(result, value, () => "");
                        }
                    }
                }
                return result;
            }
            return "";
        }
    }
}
