namespace Samba.Services.Common
{
    public interface IDevice
    {
        string Name { get; }
        void Initialize();
    }
}