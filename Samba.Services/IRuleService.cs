namespace Samba.Services
{
    public interface IRuleService
    {
        void NotifyEvent(string eventName, object dataObject);
    }
}
