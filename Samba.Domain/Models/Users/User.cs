using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Users
{
    public class User : EntityClass
    {
        public User()
        {

        }

        public User(string name, string pinCode)
        {
            Name = name;
            PinCode = pinCode;
            _userRole = UserRole.Empty;
        }

        public string PinCode { get; set; }
        
        private UserRole _userRole;
        public virtual UserRole UserRole
        {
            get { return _userRole; }
            set { _userRole = value; }
        }

        private static readonly User _nobody = new User("*", "");
        public static User Nobody { get { return _nobody; } }

        public string UserString
        {
            get { return Name; }
        }
    }
}
