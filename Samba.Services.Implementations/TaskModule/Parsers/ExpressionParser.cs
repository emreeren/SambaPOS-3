using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Tasks;

namespace Samba.Services.Implementations.TaskModule.Parsers
{
    [Export(typeof(ITokenParser))]
    public class ExpressionParser : TokenParser, ITokenParser
    {
        private readonly IAutomationService _automationService;

        [ImportingConstructor]
        public ExpressionParser(IAutomationService automationService)
        {
            _automationService = automationService;
        }

        public override string GetAcceptPattern()
        {
            return "^=";
        }

        public override int GetTaskType()
        {
            return 2;
        }

        public override string GetValue(string part)
        {
            var result = _automationService.Eval(part);
            if (string.IsNullOrEmpty(result)) return "@" + part;
            if (IsDate(result)) return "@" + result;
            return result;
        }

        private bool IsDate(string result)
        {
            DateTime dt;
            return DateTime.TryParseExact(result, CultureInfo.CurrentCulture.DateTimeFormat.GetAllDateTimePatterns(), CultureInfo.CurrentCulture,
                                          DateTimeStyles.AssumeLocal, out dt);
        }
    }
}
