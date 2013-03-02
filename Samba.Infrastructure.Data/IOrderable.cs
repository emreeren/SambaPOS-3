namespace Samba.Infrastructure.Data
{
    public interface IOrderable
    {
        string Name { get; }
        int SortOrder { get; set; }
        string UserString { get; }
    }
}
