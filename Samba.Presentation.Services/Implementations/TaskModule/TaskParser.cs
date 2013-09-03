using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Tasks;

namespace Samba.Presentation.Services.Implementations.TaskModule
{
    [Export]
    public class TaskParser
    {
        [ImportMany(typeof(ITokenParser))]
        public IEnumerable<ITokenParser> Parsers { get; set; }

        public IEnumerable<TaskToken> Parse(Task task)
        {
            if (string.IsNullOrEmpty(task.Content)) return null;
            return task.Content.Split(',')
                .Select(ParseToken)
                .Where(token => token != null)
                .ToList();
        }

        private TaskToken ParseToken(string part)
        {
            var parser = Parsers.FirstOrDefault(x => x.Accepts(part));
            if (parser == null) return null;
            var result = new ParseResult { Value = part };
            while (parser != null)
            {
                result = parser.Parse(result.Value);
                parser = Parsers.FirstOrDefault(x => x.Accepts(result.Value));
            }
            if (string.IsNullOrEmpty(result.Value)) return null;
            return new TaskToken { Caption = result.Value, Value = part, Type = result.TaskType };
        }
    }
}