using Microsoft.Practices.ServiceLocation;

namespace Samba.Services.Implementations.AutomationModule.Accessors
{
    public static class LocatorAccessor
    {
        private static IApplicationState _applicationState;
        private static ITicketService _ticketService;
        private static IDepartmentService _departmentService;
        private static IUserService _userService;
        private static IMenuService _menuService;
        private static IPrinterService _printerService;
        private static ICacheService _cacheService;

        public static IApplicationState ApplicationState
        {
            get { return _applicationState ?? (_applicationState = ServiceLocator.Current.GetInstance<IApplicationState>()); }
        }

        public static ITicketService TicketService
        {
            get { return _ticketService ?? (_ticketService = ServiceLocator.Current.GetInstance<ITicketService>()); }
        }

        public static IDepartmentService DepartmentService
        {
            get { return _departmentService ?? (_departmentService = ServiceLocator.Current.GetInstance<IDepartmentService>()); }
        }

        public static IUserService UserService
        {
            get { return _userService ?? (_userService = ServiceLocator.Current.GetInstance<IUserService>()); }
        }

        public static IMenuService MenuService
        {
            get { return _menuService ?? (_menuService = ServiceLocator.Current.GetInstance<IMenuService>()); }
        }

        public static IPrinterService PrinterService
        {
            get { return _printerService ?? (_printerService = ServiceLocator.Current.GetInstance<IPrinterService>()); }
        }

        public static ICacheService CacheService
        {
            get { return _cacheService ?? (_cacheService = ServiceLocator.Current.GetInstance<ICacheService>()); }
        }
    }
}