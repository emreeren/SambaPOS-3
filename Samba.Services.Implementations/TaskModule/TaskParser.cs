using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Tasks;

namespace Samba.Services.Implementations.TaskModule
{
    [Export]
    public class TaskParser
    {
        [ImportMany(typeof(ITokenParser))]
        public IEnumerable<ITokenParser> Parsers { get; set; }

        public IEnumerable<TaskToken> Parse(Task task)
        {
            return task.Content.Split(',')
                .Select(ParseToken)
                .Where(token => token != null)
                .ToList();
        }

        private TaskToken ParseToken(string part)
        {
            var parser = Parsers.FirstOrDefault(x => x.Accepts(part));
            return parser != null ? parser.Parse(part) : null;
        }
    }
}