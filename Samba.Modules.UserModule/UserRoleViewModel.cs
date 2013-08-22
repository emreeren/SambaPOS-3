using System;
using System.Collections.Generic;
using FluentValidation;
using Samba.Domain.Models.Tickets;
using Samba.Domain.Models.Users;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Services.Common;
using Samba.Services;
using System.Linq;

namespace Samba.Modules.UserModule
{
    public class UserRoleViewModel : EntityViewModelBase<UserRole>
    {
        private IEnumerable<PermissionViewModel> _permissions;
        public IEnumerable<PermissionViewModel> Permissions
        {
            get { return _permissions ?? (_permissions = GetPermissions()); }
        }

        private IEnumerable<PermissionViewModel> GetPermissions()
        {
            var missingPermissions = Model.Permissions.Where(x => PermissionRegistry.PermissionNames.All(y => y.Key != x.Name));

            missingPermissions.ToList().ForEach(x =>
                                                    {
                                                        Model.Permissions.Remove(x);
                                                        Workspace.Delete(x);
                                                    });
           
            if (Model.Permissions.Count() < PermissionRegistry.PermissionNames.Count)
            {
                foreach (var pName in PermissionRegistry.PermissionNames.Keys.Where(pName => Model.Permissions.SingleOrDefault(x => x.Name == pName) == null).ToList())
                {
                    Model.Permissions.Add(new Permission { Name = pName, Value = 0 });
                }
            }
            return Model.Permissions.Select(x => new PermissionViewModel(x));
        }

        private IEnumerable<Department> _departments;
        public IEnumerable<Department> Departments
        {
            get { return _departments ?? (_departments = Workspace.All<Department>()); }
        }

        public int DepartmentId
        {
            get { return Model.DepartmentId; }
            set { Model.DepartmentId = value; }
        }

        public bool IsAdmin
        {
            get { return Model.IsAdmin; }
            set { Model.IsAdmin = value || Model.Id == 1; }
        }

        public override Type GetViewType()
        {
            return typeof(UserRoleView);
        }

        public override string GetModelTypeString()
        {
            return Resources.UserRole;
        }

        protected override AbstractValidator<UserRole> GetValidator()
        {
            return new UserRoleValidator();
        }
    }

    internal class UserRoleValidator : EntityValidator<UserRole>
    {
        public UserRoleValidator()
        {
            RuleFor(x => x.DepartmentId).GreaterThan(0);
        }
    }
}
