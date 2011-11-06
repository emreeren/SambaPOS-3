namespace Samba.Infrastructure.Data
{
    public interface IEntity
    {
        int Id { get; set; }
        string Name { get; set; }
    }
}
