using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using FluentValidation;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Persistance;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Common.Services;
using Samba.Services;

namespace Samba.Modules.PrinterModule
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    class PrintJobViewModel : EntityViewModelBase<PrintJob>
    {
        private readonly IMenuService _menuService;
        private readonly IPrinterDao _printerDao;
        private readonly ICacheService _cacheService;

        [ImportingConstructor]
        public PrintJobViewModel(IMenuService menuService, IPrinterDao printerDao, ICacheService cacheService)
        {
            _newPrinterMaps = new List<PrinterMap>();
            _menuService = menuService;
            _printerDao = printerDao;
            _cacheService = cacheService;
            AddPrinterMapCommand = new CaptionCommand<string>(Resources.Add, OnAddPrinterMap);
            DeletePrinterMapCommand = new CaptionCommand<string>(Resources.Delete, OnDelete, CanDelete);
        }

        public ICaptionCommand AddPrinterMapCommand { get; set; }
        public ICaptionCommand DeletePrinterMapCommand { get; set; }

        private readonly IList<string> _whatToPrintTypes = new[] { Resources.AllLines, Resources.LastLinesByPrinterLineCount, Resources.LastPaidOrders, Resources.IndividualOrdersByQuantity, Resources.SeparatedByQuantity };
        public IList<string> WhatToPrintTypes { get { return _whatToPrintTypes; } }

        public IEnumerable<Department> Departments { get { return GetAllDepartments(); } }
        public IEnumerable<Printer> Printers { get { return Workspace.All<Printer>(); } }
        public IEnumerable<PrinterTemplate> PrinterTemplates { get { return Workspace.All<PrinterTemplate>(); } }

        private readonly IList<PrinterMap> _newPrinterMaps;

        private ObservableCollection<PrinterMapViewModel> _printerMaps;
        public ObservableCollection<PrinterMapViewModel> PrinterMaps { get { return _printerMaps ?? (_printerMaps = GetPrinterMaps()); } }

        public PrinterMapViewModel SelectedPrinterMap { get; set; }

        public string WhatToPrint { get { return _whatToPrintTypes[Model.WhatToPrint]; } set { Model.WhatToPrint = _whatToPrintTypes.IndexOf(value); } }
        public bool UseForPaidTickets { get { return Model.UseForPaidTickets; } set { Model.UseForPaidTickets = value; } }
        public bool ExcludeTax { get { return Model.ExcludeTax; } set { Model.ExcludeTax = value; } }

        private IEnumerable<Department> GetAllDepartments()
        {
            IList<Department> result = new List<Department>(Workspace.All<Department>().OrderBy(x => x.Name));
            result.Insert(0, Department.All);
            return result;
        }

        private ObservableCollection<PrinterMapViewModel> GetPrinterMaps()
        {
            return new ObservableCollection<PrinterMapViewModel>(
                    Model.PrinterMaps.Select(
                    printerMap => new PrinterMapViewModel(printerMap, _menuService, _printerDao, _cacheService)));
        }

        public override Type GetViewType()
        {
            return typeof(PrintJobView);
        }

        public override string GetModelTypeString()
        {
            return Resources.PrintJob;
        }

        protected override void OnSave(string value)
        {
            foreach (var printerMap in _printerMaps)
            {
                if (printerMap.MenuItemGroupCode == "*") printerMap.MenuItemGroupCode = null;
            }

            foreach (var newPrinterMap in _newPrinterMaps)
            {
                if (newPrinterMap.PrinterId > 0)
                {
                    if (newPrinterMap.MenuItemGroupCode == "*") newPrinterMap.MenuItemGroupCode = null;
                    Workspace.Add(newPrinterMap);
                }
            }
            base.OnSave(value);
            _newPrinterMaps.Clear();
        }

        private void OnDelete(string obj)
        {
            if (InteractionService.UserIntraction.AskQuestion(Resources.DeleteSelectedMappingQuestion))
            {
                var map = SelectedPrinterMap.Model;
                PrinterMaps.Remove(SelectedPrinterMap);
                Model.PrinterMaps.Remove(map);
                if (_newPrinterMaps.Contains(map))
                    _newPrinterMaps.Remove(map);
                else
                {
                    Workspace.Delete(map);
                    Workspace.CommitChanges();
                }
            }
        }

        private bool CanDelete(string arg)
        {
            return SelectedPrinterMap != null;
        }

        private void OnAddPrinterMap(object obj)
        {
            var map = new PrinterMap { MenuItemId = 0, MenuItemGroupCode = "*" };
            var mapModel = new PrinterMapViewModel(map, _menuService, _printerDao, _cacheService);
            Model.PrinterMaps.Add(map);
            PrinterMaps.Add(mapModel);
            _newPrinterMaps.Add(map);
        }

        protected override AbstractValidator<PrintJob> GetValidator()
        {
            return new PrintJobValidator();
        }
    }

    class PrintJobValidator : EntityValidator<PrintJob>
    {
        public PrintJobValidator()
        {
            RuleFor(x => x.PrinterMaps).Must(x => x.Count > 0).WithMessage("Add print job mapping.");
            RuleFor(x => x.PrinterMaps).Must(x => x.All(y => y.PrinterId > 0)).WithMessage("Select printer for all maps.");
            RuleFor(x => x.PrinterMaps).Must(x => x.All(y => y.PrinterTemplateId > 0)).WithMessage("Select Printer Template for all maps.");
        }
    }
}
