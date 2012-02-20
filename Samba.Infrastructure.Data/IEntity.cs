using System;

namespace Samba.Infrastructure.Data
{
    public interface IValue
    {
        int Id { get; set; }
    }

    public interface IEntity : IValue
    {
        string Name { get; set; }
    }

    public abstract class Value : IValue
    {
        public int Id { get; set; }
    }

    public abstract class Entity : Value, IEntity
    {
        public string Name { get; set; }
    }
}
