using System.Collections.Generic;
using System.Linq;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Presentation.Common;
using Samba.Services;

namespace Samba.Modules.PrinterModule
{
    public class PrinterMapViewModel : ObservableObject
    {
        public PrinterMap Model { get; set; }
        private const string NullLabel = "*";
        private static readonly string NullPrinterLabel = string.Format("- {0} -", Localization.Properties.Resources.Select);
        private readonly IDepartmentService _departmentService;
        private readonly IMenuService _menuService;
        private readonly IPrinterService _printerService;
        private readonly ICacheService _cacheService;

        public PrinterMapViewModel(PrinterMap model, IDepartmentService departmentService,
            IMenuService menuService, IPrinterService printerService, ICacheService cacheService)
        {
            Model = model;
            _departmentService = departmentService;
            _menuService = menuService;
            _printerService = printerService;
            _cacheService = cacheService;
        }

        public IEnumerable<Department> Departments { get { return GetAllDepartments(); } }
        public IEnumerable<MenuItem> MenuItems { get { return GetAllMenuItems(MenuItemGroupCode); } }
        public IEnumerable<Printer> Printers { get { return _printerService.GetPrinters(); } }
        public IEnumerable<string> TicketTags { get { return GetTicketTags(); } }
        public IEnumerable<string> MenuItemGroupCodes { get { return GetAllMenuItemGroupCodes(); } }
        public IEnumerable<PrinterTemplate> PrinterTemplates { get { return _printerService.GetAllPrinterTemplates(); } }

        private IEnumerable<string> GetAllMenuItemGroupCodes()
        {
            IList<string> result = new List<string>(_menuService.GetMenuItemGroupCodes().OrderBy(x => x));
            result.Insert(0, NullLabel);
            return result;
        }

        private IEnumerable<string> GetTicketTags()
        {
            IList<string> result = new List<string>(_cacheService.GetTicketTagGroupNames().OrderBy(x => x));
            result.Insert(0, NullLabel);
            return result;
        }

        private IEnumerable<Department> GetAllDepartments()
        {
            IList<Department> result = new List<Department>(_departmentService.GetDepartments().OrderBy(x => x.Name));
            result.Insert(0, Department.All);
            return result;
        }

        private IEnumerable<MenuItem> GetAllMenuItems(string groupCode)
        {
            IList<MenuItem> result = string.IsNullOrEmpty(groupCode) || groupCode == NullLabel
                                         ? new List<MenuItem>(
                                              _menuService.GetMenuItems().OrderBy(x => x.Name))
                                         : new List<MenuItem>(
                                               _menuService.GetMenuItemsByGroupCode(MenuItemGroupCode).OrderBy(x => x.Name));

            result.Insert(0, MenuItem.All);
            return result;
        }

        public string PrinterTemplateLabel { get { return PrinterTemplateId > 0 ? PrinterTemplates.Single(x => x.Id == PrinterTemplateId).Name : NullPrinterLabel; } }
        public int PrinterTemplateId { get { return Model.PrinterTemplateId; } set { Model.PrinterTemplateId = value; } }

        public string DepartmentLabel { get { return DepartmentId > 0 ? Departments.Single(x => x.Id == DepartmentId).Name : NullLabel; } }
        public int DepartmentId { get { return Model.DepartmentId; } set { Model.DepartmentId = value; } }

        public string MenuItemGroupCodeLabel
        {
            get { return string.IsNullOrEmpty(MenuItemGroupCode) ? NullLabel : MenuItemGroupCode; }
        }

        public string MenuItemGroupCode
        {
            get { return Model.MenuItemGroupCode ?? NullLabel; }
            set
            {
                Model.MenuItemGroupCode = value;
                RaisePropertyChanged(() => MenuItemGroupCode);
                RaisePropertyChanged(() => MenuItemGroupCodeLabel);
                RaisePropertyChanged(() => MenuItems);
            }
        }

        public string TicketTagLabel
        {
            get
            {
                return TicketTag ?? NullLabel;
            }
        }

        public string TicketTag
        {
            get
            {
                return Model.TicketTag ?? NullLabel;
            }
            set
            {
                Model.TicketTag = value;
                RaisePropertyChanged(() => TicketTag);
                RaisePropertyChanged(() => TicketTagLabel);
            }
        }

        public string MenuItemLabel { get { return MenuItemId > 0 ? MenuItems.Single(x => x.Id == MenuItemId).Name : NullLabel; } }
        public int MenuItemId { get { return Model.MenuItemId; } set { Model.MenuItemId = value; } }

        public string PrinterLabel { get { return PrinterId > 0 ? Printers.Single(x => x.Id == PrinterId).Name : NullPrinterLabel; } }
        public int PrinterId { get { return Model.PrinterId; } set { Model.PrinterId = value; } }
    }
}
