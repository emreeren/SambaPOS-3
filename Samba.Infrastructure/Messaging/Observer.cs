namespace Samba.Infrastructure.Messaging
{
    public interface ISubject
    {
        void Attach(IObserver observer);
        void Detach(IObserver observer);

        bool Notify(string objType, short objState);
    }

    public interface IObserver
    {
        bool Update(ISubject sender, string objType, short objState);
    }
}
