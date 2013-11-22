namespace Samba.Domain
{
    public interface ICustomDataProvider
    {
        string GetCustomData(string fieldName);
    }
}