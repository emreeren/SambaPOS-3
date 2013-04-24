using System.Collections.Generic;
using System.Linq;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Settings;
using Samba.Persistance;
using Samba.Presentation.Common;
using Samba.Services;

namespace Samba.Modules.PrinterModule
{
    public class PrinterMapViewModel : ObservableObject
    {
        public PrinterMap Model { get; set; }
        private const string NullLabel = "*";
        private static readonly string NullPrinterLabel = string.Format("- {0} -", Localization.Properties.Resources.Select);
        private readonly IMenuService _menuService;
        private readonly IPrinterDao _printerDao;
        private readonly ICacheService _cacheService;

        public PrinterMapViewModel(PrinterMap model, IMenuService menuService, IPrinterDao printerDao, ICacheService cacheService)
        {
            Model = model;
            _menuService = menuService;
            _printerDao = printerDao;
            _cacheService = cacheService;
        }

        private IEnumerable<PrinterTemplate> _printerTemplates;
        public IEnumerable<PrinterTemplate> PrinterTemplates { get { return _printerTemplates ?? (_printerTemplates = _printerDao.GetPrinterTemplates()); } }

        private IEnumerable<Printer> _printers;
        public IEnumerable<Printer> Printers { get { return _printers ?? (_printers = _printerDao.GetPrinters()); } }

        public IEnumerable<MenuItem> MenuItems { get { return GetAllMenuItems(MenuItemGroupCode); } }
        public IEnumerable<string> TicketTags { get { return GetTicketTags(); } }
        public IEnumerable<string> MenuItemGroupCodes { get { return GetAllMenuItemGroupCodes(); } }


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

        public string MenuItemLabel { get { return MenuItemId > 0 ? MenuItems.Single(x => x.Id == MenuItemId).Name : NullLabel; } }
        public int MenuItemId { get { return Model.MenuItemId; } set { Model.MenuItemId = value; } }

        public string PrinterLabel { get { return PrinterId > 0 ? Printers.Single(x => x.Id == PrinterId).Name : NullPrinterLabel; } }
        public int PrinterId { get { return Model.PrinterId; } set { Model.PrinterId = value; } }
    }
}
