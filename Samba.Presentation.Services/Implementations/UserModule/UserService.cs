using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using Samba.Domain.Models.Tickets;
using Samba.Domain.Models.Users;
using Samba.Persistance;
using Samba.Persistance.Data;
using Samba.Presentation.Services.Common;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Presentation.Services.Implementations.UserModule
{
    [Export(typeof(IUserService))]
    public class UserService : AbstractService, IUserService
    {
        private readonly IUserDao _userDao;
        private readonly IApplicationState _applicationState;
        private readonly IApplicationStateSetter _applicationStateSetter;
        private readonly IDepartmentService _departmentService;

        [ImportingConstructor]
        public UserService(IUserDao userDao, IApplicationState applicationState, IApplicationStateSetter applicationStateSetter,
            IDepartmentService departmentService)
        {
            _userDao = userDao;
            _applicationState = applicationState;
            _applicationStateSetter = applicationStateSetter;
            _departmentService = departmentService;
        }

        private IEnumerable<User> _users;
        public IEnumerable<User> Users { get { return _users ?? (_users = Dao.Query<User>(x => x.UserRole)); } }

        private IEnumerable<Department> _permittedDepartments;
        public IEnumerable<Department> PermittedDepartments
        {
            get
            {
                return _permittedDepartments ?? (
                       _permittedDepartments = _departmentService.GetDepartments().Where(
                         x => IsUserPermittedFor(PermissionNames.UseDepartment + x.Id)));
            }
        }

        public bool ContainsUser(int userId)
        {
            return Users.FirstOrDefault(x => x.Id == userId) != null;
        }

        public bool IsDefaultUserConfigured
        {
            get { return Users.Count() == 1 && Users.ElementAt(0).PinCode == "1234"; }
        }

        public string GetUserName(int userId)
        {
            return userId > 0 ? Users.Single(x => x.Id == userId).Name : "-";
        }

        public User LoginUser(string pinValue)
        {
            Debug.Assert(_applicationState.CurrentLoggedInUser == User.Nobody);
            var user = CheckPinCodeStatus(pinValue) == LoginStatus.CanLogin ? GetUserByPinCode(pinValue) : User.Nobody;
            _applicationStateSetter.SetCurrentLoggedInUser(user);
            Reset();
            if (user != User.Nobody)
            {
                user.PublishEvent(EventTopicNames.UserLoggedIn);
                _applicationState.NotifyEvent(RuleEventNames.UserLoggedIn, new { User = user, UserName = user.Name, RoleName = user.UserRole.Name });
            }
            return user;
        }

        public void LogoutUser(bool resetCache = true)
        {
            var user = _applicationState.CurrentLoggedInUser;
            Debug.Assert(user != User.Nobody);
            user.PublishEvent(EventTopicNames.UserLoggedOut);
            _applicationState.NotifyEvent(RuleEventNames.UserLoggedOut, new { User = user, UserName = user.Name, RoleName = user.UserRole.Name });
            _applicationStateSetter.SetCurrentLoggedInUser(User.Nobody);
            EventServiceFactory.EventService.PublishEvent(EventTopicNames.ResetCache, true);
        }

        private User GetUserByPinCode(string pinCode)
        {
            return _userDao.GetUserByPinCode(pinCode);
        }

        private LoginStatus CheckPinCodeStatus(string pinCode)
        {
            var userExists = _userDao.GetIsUserExists(pinCode);
            return userExists ? LoginStatus.CanLogin : LoginStatus.PinNotFound;
        }

        public bool IsUserPermittedFor(string p)
        {
            var user = _applicationState.CurrentLoggedInUser;
            if (user == User.Nobody) return false;
            if (user.UserRole.IsAdmin) return true;
            if (user.UserRole.Id == 0) return false;
            var permission = user.UserRole.Permissions.SingleOrDefault(x => x.Name == p);
            if (permission == null) return false;
            return permission.Value == (int)PermissionValue.Enabled;
        }

        public IEnumerable<UserRole> GetUserRoles()
        {
            return _userDao.GetUserRoles();
        }

        public override void Reset()
        {
            _users = null;
            _permittedDepartments = null;
        }
    }
}
