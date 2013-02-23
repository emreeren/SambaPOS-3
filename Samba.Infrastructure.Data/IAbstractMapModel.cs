namespace Samba.Infrastructure.Data
{
    public interface IAbstractMapModel : IValueClass
    {
        int TerminalId { get; set; }
        int DepartmentId { get; set; }
        int UserRoleId { get; set; }
        int TicketTypeId { get; set; }
    }
}