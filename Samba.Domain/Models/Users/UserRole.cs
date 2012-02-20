using System.Collections.Generic;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Users
{
    public class UserRole : Entity
    {
        public UserRole()
        {
            Permissions = new List<Permission>();
        }

        public UserRole(string name)
            : this()
        {
            Name = name;
        }

        public byte[] LastUpdateTime { get; set; }
        public bool IsAdmin { get; set; }
        public int DepartmentId { get; set; }
        public virtual IList<Permission> Permissions { get; set; }

        public string UserString
        {
            get { return Name; }
        }

        private static readonly UserRole EmptyUserRole = new UserRole { Id = 0, Name = "Default UserRole" };
        public static UserRole Empty { get { return EmptyUserRole; } }
    }
}
