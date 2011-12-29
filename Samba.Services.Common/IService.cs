namespace Samba.Services.Common
{
    public interface IService
    {
        string TestSaveOperation<T>(T model) where T : class;
        string TestDeleteOperation<T>(T model) where T : class;
        void Reset();
    }
}
