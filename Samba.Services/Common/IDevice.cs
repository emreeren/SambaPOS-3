namespace Samba.Services.Common
{
    public interface IDevice
    {
        string Name { get; }
        void InitializeDevice();
        void FinalizeDevice();
        DeviceType DeviceType { get; }
        object GetSettingsObject();
        void SaveSettings();
    }

    public enum DeviceType
    {
        CallerId
    }
}