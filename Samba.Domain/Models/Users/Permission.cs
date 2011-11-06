using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Users
{
    public enum PermissionValue
    {
        Enabled,
        Disabled,
        Invisible
    }

    public class Permission : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Value { get; set; }
        public int UserRoleId { get; set; }
    }
}
