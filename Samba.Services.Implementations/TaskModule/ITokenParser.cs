using Samba.Domain.Models.Tasks;

namespace Samba.Services.Implementations.TaskModule
{
    public interface ITokenParser
    {
        bool Accepts(string part);
        TaskToken Parse(string part);
    }
}