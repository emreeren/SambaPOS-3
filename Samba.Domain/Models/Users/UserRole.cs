using System.Collections.Generic;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Users
{
    public class UserRole : EntityClass
    {
        public UserRole()
        {
            _permissions = new List<Permission>();
        }

        public UserRole(string name)
            : this()
        {
            Name = name;
        }

        public bool IsAdmin { get; set; }
        public int DepartmentId { get; set; }

        private IList<Permission> _permissions;
        public virtual IList<Permission> Permissions
        {
            get { return _permissions; }
            set { _permissions = value; }
        }

        public string UserString
        {
            get { return Name; }
        }

        private static readonly UserRole EmptyUserRole = new UserRole { Id = 0, Name = "Default UserRole" };
        public static UserRole Empty { get { return EmptyUserRole; } }
    }
}
