namespace Samba.Infrastructure.Data
{
    public interface IEntityClass : IValueClass
    {
        string Name { get; set; }
    }

    public abstract class EntityClass : ValueClass, IEntityClass
    {
        public string Name { get; set; }
    }
}