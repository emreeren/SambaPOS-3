using System.Text.RegularExpressions;

namespace Samba.Presentation.Services.Implementations.TaskModule
{
    public interface ITokenParser
    {
        string AcceptPattern { get; }
        bool Accepts(string part);
        ParseResult Parse(string part);
    }

    public class ParseResult
    {
        public string Value { get; set; }
        public int TaskType { get; set; }
    }

    public abstract class TokenParser
    {
        public string AcceptPattern { get { return GetAcceptPattern(); } }
        public abstract string GetAcceptPattern();
        public abstract int GetTaskType();

        public bool Accepts(string part)
        {
            return Regex.IsMatch(part, AcceptPattern);
        }

        public ParseResult Parse(string part)
        {
            var trimmedPart = Regex.Replace(part, GetAcceptPattern(), "");
            var value = GetValue(trimmedPart);
            return new ParseResult { Value = value, TaskType = GetTaskType() };
        }

        public abstract string GetValue(string part);

    }
}