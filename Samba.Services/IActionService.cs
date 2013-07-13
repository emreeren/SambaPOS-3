using Samba.Services.Common;

namespace Samba.Services
{
    public interface IActionService
    {
        bool CanProcessAction(string actionType);
        void ProcessAction(string actionType, ActionData actionData);
        void Register();
    }
}
