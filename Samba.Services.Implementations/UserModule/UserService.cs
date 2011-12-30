using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using Samba.Domain.Models.Tickets;
using Samba.Domain.Models.Users;
using Samba.Infrastructure.Data;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Services.Common;

namespace Samba.Services.Implementations.UserModule
{
    [Export(typeof(IUserService))]
    public class UserService : AbstractService, IUserService
    {
        private readonly IApplicationState _applicationState;
        private readonly IApplicationStateSetter _applicationStateSetter;
        private readonly IDepartmentService _departmentService;
        private readonly IAutomationService _automationService;

        [ImportingConstructor]
        public UserService(IApplicationState applicationState, IApplicationStateSetter applicationStateSetter,
            IDepartmentService departmentService, IAutomationService automationService)
        {
            _applicationState = applicationState;
            _applicationStateSetter = applicationStateSetter;
            _departmentService = departmentService;
            _automationService = automationService;

            ValidatorRegistry.RegisterDeleteValidator(new UserDeleteValidator());
            ValidatorRegistry.RegisterDeleteValidator(new UserRoleDeleteValidator());
            ValidatorRegistry.RegisterSaveValidator(new UserSaveValidator());
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
                _automationService.NotifyEvent(RuleEventNames.UserLoggedIn, new { User = user, RoleName = user.UserRole.Name });
            }
            return user;
        }

        public void LogoutUser(bool resetCache = true)
        {
            var user = _applicationState.CurrentLoggedInUser;
            Debug.Assert(user != User.Nobody);
            user.PublishEvent(EventTopicNames.UserLoggedOut);
            _automationService.NotifyEvent(RuleEventNames.UserLoggedOut, new { User = user, RoleName = user.UserRole.Name });
            _applicationStateSetter.SetCurrentLoggedInUser(User.Nobody);
            EventServiceFactory.EventService.PublishEvent(EventTopicNames.ResetCache, true);
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

    public class UserSaveValidator : SpecificationValidator<User>
    {
        public override string GetErrorMessage(User model)
        {
            if (Dao.Exists<User>(x => x.PinCode == model.PinCode && x.Id != model.Id))
                return Resources.SaveErrorThisPinCodeInUse;
            return "";
        }
    }

    public class UserRoleDeleteValidator : SpecificationValidator<UserRole>
    {
        public override string GetErrorMessage(UserRole model)
        {
            if (Dao.Exists<User>(x => x.UserRole.Id == model.Id))
                return Resources.DeleteErrorThisRoleUsedInAUserAccount;
            return "";
        }
    }

    public class UserDeleteValidator : SpecificationValidator<User>
    {
        public override string GetErrorMessage(User model)
        {
            if (model.UserRole.IsAdmin) return Resources.DeleteErrorAdminUser;
            if (Dao.Count<User>() == 1) return Resources.DeleteErrorLastUser;
            if (Dao.Exists<Order>(x => x.CreatingUserName == model.Name))
                return Resources.DeleteErrorUserDidTicketOperation;
            return "";
        }
    }
}
