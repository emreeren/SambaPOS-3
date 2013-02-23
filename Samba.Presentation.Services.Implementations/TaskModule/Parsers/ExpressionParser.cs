using System;
using System.ComponentModel.Composition;
using System.Globalization;
using Samba.Services;

namespace Samba.Presentation.Services.Implementations.TaskModule.Parsers
{
    [Export(typeof(ITokenParser))]
    public class ExpressionParser : TokenParser, ITokenParser
    {
        private readonly IExpressionService _expressionService;

        [ImportingConstructor]
        public ExpressionParser(IExpressionService expressionService)
        {
            _expressionService = expressionService;
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
            var result = _expressionService.Eval(part);
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
