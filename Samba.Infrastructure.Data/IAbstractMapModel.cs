namespace Samba.Infrastructure.Data
{
    public interface IAbstractMapModel : IValue
    {
        int DepartmentId { get; set; }
        int UserRoleId { get; set; }
    }
}