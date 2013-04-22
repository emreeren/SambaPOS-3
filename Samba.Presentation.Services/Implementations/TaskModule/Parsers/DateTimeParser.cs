using System;
using System.ComponentModel.Composition;

namespace Samba.Presentation.Services.Implementations.TaskModule.Parsers
{
    [Export(typeof(ITokenParser))]
    public class DateTimeParser : TokenParser, ITokenParser
    {
        public override string GetValue(string part)
        {
            var dt = ParseTime(part);
            return dt == DateTime.MinValue ? "" : string.Format("{0} {1}", dt.ToShortDateString(), dt.ToShortTimeString());
        }

        private DateTime ParseTime(string part)
        {
            DateTime dt;
            return !DateTime.TryParse(part, out dt) ? DateTime.MinValue : dt;
        }

        public override string GetAcceptPattern()
        {
            return "^@";
        }

        public override int GetTaskType()
        {
            return 1;
        }
    }
}