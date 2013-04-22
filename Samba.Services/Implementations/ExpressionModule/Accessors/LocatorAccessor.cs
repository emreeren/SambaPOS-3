using Microsoft.Practices.ServiceLocation;

namespace Samba.Services.Implementations.ExpressionModule.Accessors
{
    public static class LocatorAccessor
    {
        private static IDepartmentService _departmentService;
        private static IMenuService _menuService;

        public static IDepartmentService DepartmentService
        {
            get { return _departmentService ?? (_departmentService = ServiceLocator.Current.GetInstance<IDepartmentService>()); }
        }

        public static IMenuService MenuService
        {
            get { return _menuService ?? (_menuService = ServiceLocator.Current.GetInstance<IMenuService>()); }
        }
    }
}