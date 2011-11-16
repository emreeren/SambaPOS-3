using System;
using System.Collections.Generic;
using System.Linq;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Data;
using Samba.Persistance.Data;
using Samba.Presentation.Common;
using Samba.Services;

namespace Samba.Modules.SettingsModule
{
    public class PrinterMapViewModel : ObservableObject
    {
        public PrinterMap Model { get; set; }
        private const string NullLabel = "*";
        private static readonly string NullPrinterLabel = string.Format("- {0} -", Localization.Properties.Resources.Select);
        private readonly IWorkspace _workspace;

        public PrinterMapViewModel(PrinterMap model, IWorkspace workspace)
        {
            Model = model;
            _workspace = workspace;
        }

        public IEnumerable<Department> Departments { get { return GetAllDepartments(); } }
        public IEnumerable<MenuItem> MenuItems { get { return GetAllMenuItems(MenuItemGroupCode); } }
        public IEnumerable<Printer> Printers { get { return _workspace.All<Printer>(); } }
        public IEnumerable<string> TicketTags { get { return GetTicketTags(); } }

        public IEnumerable<string> MenuItemGroupCodes { get { return GetAllMenuItemGroupCodes(); } }
        public IEnumerable<PrinterTemplate> PrinterTemplates { get { return _workspace.All<PrinterTemplate>(); } }

        private static IEnumerable<string> GetAllMenuItemGroupCodes()
        {
            IList<string> result = new List<string>(Dao.Distinct<MenuItem>(x => x.GroupCode).OrderBy(x => x));
            result.Insert(0, NullLabel);
            return result;
        }

        private static IEnumerable<string> GetTicketTags()
        {
            IList<string> result = new List<string>(Dao.Distinct<TicketTagGroup>(x => x.Name).OrderBy(x => x));
            result.Insert(0, NullLabel);
            return result;
        }

        private IEnumerable<Department> GetAllDepartments()
        {
            IList<Department> result = new List<Department>(_workspace.All<Department>().OrderBy(x => x.Name));
            result.Insert(0, Department.All);
            return result;
        }

        private IEnumerable<MenuItem> GetAllMenuItems(string groupCode)
        {
            IList<MenuItem> result = string.IsNullOrEmpty(groupCode) || groupCode == NullLabel
                                         ? new List<MenuItem>(
                                              _workspace.All<MenuItem>().OrderBy(x => x.Name))
                                         : new List<MenuItem>(
                                               _workspace.All<MenuItem>(x => x.GroupCode == groupCode).OrderBy(x => x.Name));

            result.Insert(0, MenuItem.All);
            return result;
        }

        public string PrinterTemplateLabel { get { return PrinterTemplate != null ? PrinterTemplate.Name : NullPrinterLabel; } }

        public PrinterTemplate PrinterTemplate
        {
            get { return Model.PrinterTemplate; }
            set
            {
                Model.PrinterTemplate = value;
                RaisePropertyChanged(() => PrinterTemplate);
                RaisePropertyChanged(() => PrinterTemplateLabel);
            }
        }

        public string DepartmentLabel { get { return Department != null ? Department.Name : NullLabel; } }

        public Department Department
        {
            get { return Model.Department ?? Department.All; }
            set
            {
                Model.Department = value;
                RaisePropertyChanged(() => Department);
                RaisePropertyChanged(() => DepartmentLabel);
            }
        }

        public int DepartmentId { get { return Model.Department.Id; } set { } }

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

        public string MenuItemLabel { get { return MenuItem != null ? MenuItem.Name : NullLabel; } }

        public MenuItem MenuItem
        {
            get { return Model.MenuItem ?? MenuItem.All; }
            set
            {
                Model.MenuItem = value;
                RaisePropertyChanged(() => MenuItem);
                RaisePropertyChanged(() => MenuItemLabel);
            }
        }

        public string PrinterLabel { get { return Printer != null ? Printer.Name : NullPrinterLabel; } }

        public Printer Printer
        {
            get { return Model.Printer; }
            set
            {
                Model.Printer = value;
                RaisePropertyChanged(() => Printer);
                RaisePropertyChanged(() => PrinterLabel);
            }
        }
    }
}
