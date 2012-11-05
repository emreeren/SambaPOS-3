namespace Samba.Infrastructure.Data
{
    public interface IAbstractMapModel : IValue
    {
        int TerminalId { get; set; }
        int DepartmentId { get; set; }
        int UserRoleId { get; set; }
        int TicketTypeId { get; set; }
    }
}