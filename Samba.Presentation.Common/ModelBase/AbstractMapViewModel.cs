using System.Collections.Generic;
using System.Linq;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Domain.Models.Users;
using Samba.Infrastructure.Data;
using Samba.Presentation.Services;
using Samba.Services;

namespace Samba.Presentation.Common.ModelBase
{
    public class AbstractMapViewModel<TModel> : ObservableObject
        where TModel : IAbstractMapModel
    {
        public IUserService UserService { get; set; }
        public IDepartmentService DepartmentService { get; set; }
        public ISettingService SettingService { get; set; }
        public ICacheService CacheService { get; set; }

        public TModel Model;

        private const string NullLabel = "*";

        public IEnumerable<string> GetItemSelectionList(IEnumerable<string> source)
        {
            var result = new List<string> { NullLabel };
            result.AddRange(source);
            return result;
        }

        private IEnumerable<Terminal> _terminals;
        public IEnumerable<Terminal> Terminals
        {
            get { return _terminals ?? (_terminals = SettingService.GetTerminals()); }
        }

        public IEnumerable<string> TerminalNames { get { return GetItemSelectionList(Terminals.Select(x => x.Name)); } }

        public string TerminalName
        {
            get { return Model.TerminalId > 0 ? Terminals.Single(x => x.Id == Model.TerminalId).Name : NullLabel; }
            set { Model.TerminalId = value != NullLabel ? Terminals.Single(x => x.Name == value).Id : 0; }
        }

        private IEnumerable<UserRole> _userRoles;
        public IEnumerable<UserRole> UserRoles
        {
            get { return _userRoles ?? (_userRoles = UserService.GetUserRoles()); }
        }

        public IEnumerable<string> UserRoleNames { get { return GetItemSelectionList(UserRoles.Select(x => x.Name)); } }

        public string UserRoleName
        {
            get { return Model.UserRoleId > 0 ? UserRoles.Single(x => x.Id == Model.UserRoleId).Name : NullLabel; }
            set { Model.UserRoleId = value != NullLabel ? UserRoles.Single(x => x.Name == value).Id : 0; }
        }

        private IEnumerable<Department> _departments;
        public IEnumerable<Department> Departments
        {
            get { return _departments ?? (_departments = DepartmentService.GetDepartments()); }
        }

        public IEnumerable<string> DepartmentNames { get { return GetItemSelectionList(Departments.Select(x => x.Name)); } }

        public string DepartmentName
        {
            get { return Model.DepartmentId > 0 ? Departments.Single(x => x.Id == Model.DepartmentId).Name : NullLabel; }
            set { Model.DepartmentId = value != NullLabel ? Departments.Single(x => x.Name == value).Id : 0; }
        }

        private IEnumerable<TicketType> _ticketTypes;
        public IEnumerable<TicketType> TicketTypes
        {
            get { return _ticketTypes ?? (_ticketTypes = CacheService.GetTicketTypes()); }
        }

        public IEnumerable<string> TicketTypeNames { get { return GetItemSelectionList(TicketTypes.Select(x => x.Name)); } }

        public string TicketTypeName
        {
            get { return Model.TicketTypeId > 0 ? TicketTypes.Single(x => x.Id == Model.TicketTypeId).Name : NullLabel; }
            set { Model.TicketTypeId = value != NullLabel ? TicketTypes.Single(x => x.Name == value).Id : 0; }
        }

        public int Id
        {
            get { return Model.Id; }
        }
    }

}
