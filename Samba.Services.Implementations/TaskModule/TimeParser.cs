using System.ComponentModel.Composition;
using Samba.Domain.Models.Tasks;

namespace Samba.Services.Implementations.TaskModule
{
    [Export(typeof(ITokenParser))]
    public class TimeParser : ITokenParser
    {
        public bool Accepts(string part)
        {
            return part.Contains(":");
        }

        public TaskToken Parse(string part)
        {
            return new TaskToken { Caption = part.Trim(), Value = part, Type = 1 };
        }
    }
}