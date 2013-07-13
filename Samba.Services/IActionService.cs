using Samba.Services.Common;

namespace Samba.Services
{
    public interface IActionService
    {
        void ProcessAction(string actionType, ActionData actionData);
        void Register();
    }
}
