namespace Samba.Infrastructure.Data
{
    public interface IAbstractMapModel : IValueClass
    {
        int TerminalId { get; set; }
        int DepartmentId { get; set; }
        int UserRoleId { get; set; }
        int TicketTypeId { get; set; }

        void Initialize();
    }

    public abstract class AbstractMap : ValueClass, IAbstractMapModel
    {
        public int TerminalId { get; set; }
        public int DepartmentId { get; set; }
        public int UserRoleId { get; set; }
        public int TicketTypeId { get; set; }

        public virtual void Initialize()
        {
            // Override for default values;
        }
    }
}