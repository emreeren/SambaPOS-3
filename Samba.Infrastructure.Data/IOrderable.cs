namespace Samba.Infrastructure.Data
{
    public interface IOrderable
    {
        string Name { get; }
        int Order { get; set; }
        string UserString { get; }
    }
}
