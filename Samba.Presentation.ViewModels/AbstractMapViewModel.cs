using System.Collections.Generic;
using System.Linq;
using Samba.Domain.Models.Tickets;
using Samba.Domain.Models.Users;
using Samba.Infrastructure.Data;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Services;

namespace Samba.Presentation.ViewModels
{
    public class AbstractMapViewModel:ObservableObject
    {
        private readonly IUserService _userService;
        private readonly IDepartmentService _departmentService;
        private readonly IAbstractMapModel _model;
        private const string NullLabel = "*";

        public AbstractMapViewModel(IAbstractMapModel model, IUserService userService, IDepartmentService departmentService)
        {
            _userService = userService;
            _departmentService = departmentService;
            _model = model;
        }

        public IEnumerable<string> GetItemSelectionList(IEnumerable<string> source)
        {
            var result = new List<string> {NullLabel};
            result.AddRange(source);
            return result;
        }

        private IEnumerable<UserRole> _userRoles;
        public IEnumerable<UserRole> UserRoles
        {
            get { return _userRoles ?? (_userRoles = _userService.GetUserRoles()); }
        }

        public IEnumerable<string> UserRoleNames { get { return GetItemSelectionList(UserRoles.Select(x => x.Name)); } }

        public string UserRoleName
        {
            get { return _model.UserRoleId > 0 ? UserRoles.Single(x => x.Id == _model.UserRoleId).Name : NullLabel; }
            set { _model.UserRoleId = value != NullLabel ? UserRoles.Single(x => x.Name == value).Id : 0; }
        }

        private IEnumerable<Department> _departments;
        public IEnumerable<Department> Departments
        {
            get { return _departments ?? (_departments = _departmentService.GetDepartments()); }
        }

        public IEnumerable<string> DepartmentNames { get { return GetItemSelectionList(Departments.Select(x => x.Name)); } }

        public string DepartmentName
        {
            get { return _model.DepartmentId > 0 ? Departments.Single(x => x.Id == _model.DepartmentId).Name : NullLabel; }
            set { _model.DepartmentId = value != NullLabel ? Departments.Single(x => x.Name == value).Id : 0; }
        }

        public int Id
        {
            get { return _model.Id; }
        }
    }

}
