using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using FluentValidation;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Resources;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Data;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Common.Services;
using Samba.Services;

namespace Samba.Modules.DepartmentModule
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class DepartmentViewModel : EntityViewModelBase<Department>
    {
        private readonly IMenuService _menuService;
        private readonly IPriceListService _priceListService;

        [ImportingConstructor]
        public DepartmentViewModel(IMenuService menuService, IPriceListService priceListService)
        {
            _menuService = menuService;
            _priceListService = priceListService;
            AddResourceScreenCommand = new CaptionCommand<string>(string.Format(Resources.Select_f, Resources.ResourceScreen), OnAddResourceScreen);
            DeleteResourceScreenCommand = new CaptionCommand<string>(string.Format(Resources.Delete_f, Resources.ResourceScreen), OnDeleteResourceScreen, CanDeleteResourceScreen);
        }

        private readonly IList<string> _ticketCreationMethods = new[] { string.Format(Resources.Select_f, Resources.Resource), string.Format(Resources.Create_f, Resources.Ticket) };
        public IList<string> TicketCreationMethods { get { return _ticketCreationMethods; } }
        public string TicketCreationMethod { get { return _ticketCreationMethods[Model.TicketCreationMethod]; } set { Model.TicketCreationMethod = _ticketCreationMethods.IndexOf(value); } }

        public int ScreenMenuId { get { return Model.ScreenMenuId; } set { Model.ScreenMenuId = value; } }

        private IEnumerable<ScreenMenu> _screenMenus;
        public IEnumerable<ScreenMenu> ScreenMenus
        {
            get { return _screenMenus ?? (_screenMenus = _menuService.GetScreenMenus()); }
            set { _screenMenus = value; }
        }

        private ObservableCollection<ResourceScreen> _resourceScreens;
        public ObservableCollection<ResourceScreen> ResourceScreens
        {
            get { return _resourceScreens ?? (_resourceScreens = new ObservableCollection<ResourceScreen>(Model.ResourceScreens.OrderBy(x => x.Order))); }
        }

        private IEnumerable<TicketTemplate> _ticketTemplates;
        public IEnumerable<TicketTemplate> TicketTemplates
        {
            get { return _ticketTemplates ?? (_ticketTemplates = Workspace.All<TicketTemplate>()); }
        }
        public TicketTemplate TicketTemplate { get { return Model.TicketTemplate; } set { Model.TicketTemplate = value; } }

        public IEnumerable<string> PriceTags { get { return _priceListService.GetTags(); } }
        public string PriceTag { get { return Model.PriceTag; } set { Model.PriceTag = value; } }

        public ResourceScreen SelectedResourceScreen { get; set; }

        public ICaptionCommand AddResourceScreenCommand { get; set; }
        public ICaptionCommand DeleteResourceScreenCommand { get; set; }

        private bool CanDeleteResourceScreen(string arg)
        {
            return SelectedResourceScreen != null;
        }

        private void OnDeleteResourceScreen(string obj)
        {
            Model.ResourceScreens.Remove(SelectedResourceScreen);
            ResourceScreens.Remove(SelectedResourceScreen);
        }

        private void OnAddResourceScreen(string obj)
        {
            var selectedValues =
                  InteractionService.UserIntraction.ChooseValuesFrom(Workspace.All<ResourceScreen>().ToList<IOrderable>(),
                  Model.ResourceScreens.ToList<IOrderable>(), Resources.ResourceScreens, string.Format(Resources.Select_f, Resources.ResourceScreens, Model.Name, Resources.Department),
                  Resources.ResourceScreen, Resources.ResourceScreens);

            foreach (ResourceScreen selectedValue in selectedValues)
            {
                if (!Model.ResourceScreens.Contains(selectedValue))
                    Model.ResourceScreens.Add(selectedValue);
            }

            _resourceScreens = new ObservableCollection<ResourceScreen>(Model.ResourceScreens.OrderBy(x => x.Order));

            RaisePropertyChanged(() => ResourceScreens);
        }

        public override Type GetViewType()
        {
            return typeof(DepartmentView);
        }

        public override string GetModelTypeString()
        {
            return Resources.Department;
        }

        protected override AbstractValidator<Department> GetValidator()
        {
            return new DepartmentValidator();
        }
    }

    internal class DepartmentValidator : EntityValidator<Department>
    {
        public DepartmentValidator()
        {
            RuleFor(x => x.TicketTemplate).NotNull();
        }
    }
}
