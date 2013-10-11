namespace Samba.Infrastructure.Data
{
    public interface IValueClass
    {
        int Id { get; set; }
    }

    public abstract class ValueClass : IValueClass
    {
        public int Id { get; set; }
    }
}