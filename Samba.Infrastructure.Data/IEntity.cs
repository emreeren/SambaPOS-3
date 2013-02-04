using System;
using System.Collections.Generic;

namespace Samba.Infrastructure.Data
{
    public interface IValueClass
    {
        int Id { get; set; }
    }

    public interface IEntityClass : IValueClass
    {
        string Name { get; set; }
    }

    public abstract class ValueClass : IValueClass
    {
        public int Id { get; set; }
    }

    public abstract class EntityClass : ValueClass, IEntityClass
    {
        public string Name { get; set; }
    }

    public abstract class AbstractMap : ValueClass, IAbstractMapModel
    {
        public int TerminalId { get; set; }
        public int DepartmentId { get; set; }
        public int UserRoleId { get; set; }
        public int TicketTypeId { get; set; }
    }
}
