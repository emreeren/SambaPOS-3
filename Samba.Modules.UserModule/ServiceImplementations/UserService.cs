using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Tickets;
using Samba.Domain.Models.Users;
using Samba.Infrastructure.Data;
using Samba.Persistance.Data;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Services;
using Samba.Services;

namespace Samba.Modules.UserModule.ServiceImplementations
{
    [Export(typeof(IUserService))]
    public class UserService : AbstractService, IUserService
    {
        private readonly IApplicationState _applicationState;
        private readonly IApplicationStateSetter _applicationStateSetter;
        private readonly IDepartmentService _departmentService;
        private IRuleService _ruleService;

        [ImportingConstructor]
        public UserService(IApplicationState applicationState, IApplicationStateSetter applicationStateSetter,
            IDepartmentService departmentService, IRuleService ruleService)
        {
            _applicationState = applicationState;
            _applicationStateSetter = applicationStateSetter;
            _departmentService = departmentService;
            _ruleService = ruleService;
        }

        private static IWorkspace _workspace;
        public static IWorkspace Workspace
        {
            get { return _workspace ?? (_workspace = WorkspaceFactory.Create()); }
            set { _workspace = value; }
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

        public IEnumerable<string> GetUserNames()
        {
            return Users.Select(x => x.Name);
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
                _ruleService.NotifyEvent(RuleEventNames.UserLoggedIn, new { User = user, RoleName = user.UserRole.Name });
            }
            return user;
        }

        public void LogoutUser(bool resetCache = true)
        {
            var user = _applicationState.CurrentLoggedInUser;
            Debug.Assert(user != User.Nobody);
            user.PublishEvent(EventTopicNames.UserLoggedOut);
            _ruleService.NotifyEvent(RuleEventNames.UserLoggedOut, new { User = user, RoleName = user.UserRole.Name });
            _applicationStateSetter.SetCurrentLoggedInUser(User.Nobody);
            EventServiceFactory.EventService._PublishEvent(EventTopicNames.ResetCache);
        }

        private static User GetUserByPinCode(string pinCode)
        {
            return Workspace.All<User>(x => x.PinCode == pinCode).FirstOrDefault();
        }

        private static LoginStatus CheckPinCodeStatus(string pinCode)
        {
            var users = Workspace.All<User>(x => x.PinCode == pinCode);
            return users.Count() == 0 ? LoginStatus.PinNotFound : LoginStatus.CanLogin;
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

        public override void Reset()
        {
            _users = null;
            _permittedDepartments = null;
        }
    }
}
