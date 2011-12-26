using System.ComponentModel.Composition;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;
using Samba.Services;

namespace Samba.Modules.DepartmentModule
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class DepartmentListViewModel : EntityCollectionViewModelBase<DepartmentViewModel, Department>
    {
        private readonly IDepartmentService _departmentService;

        [ImportingConstructor]
        public DepartmentListViewModel(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        protected override string CanDeleteItem(Department model)
        {
            var count = _departmentService.GetUserRoleCount(model.Id);
            if (count > 0) return Resources.DeleteErrorDepartmentUsedInRole;
            return base.CanDeleteItem(model);
        }
    }
}
