using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using FluentValidation;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tables;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Data;
using Samba.Localization.Properties;
using Samba.Modules.TicketModule;
using Samba.Persistance.Data;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Common.Services;

namespace Samba.Modules.MenuModule
{
    public class DepartmentViewModel : EntityViewModelBase<Department>
    {
        private IEnumerable<ScreenMenu> _screenMenus;
        public IEnumerable<ScreenMenu> ScreenMenus
        {
            get { return _screenMenus ?? (_screenMenus = Dao.Query<ScreenMenu>()); }
            set { _screenMenus = value; }
        }

        private IEnumerable<TableScreen> _tableScreens;
        public IEnumerable<TableScreen> TableScreens
        {
            get { return _tableScreens ?? (_tableScreens = Dao.Query<TableScreen>()); }
            set { _tableScreens = value; }
        }

        private ObservableCollection<TicketTagGroupViewModel> _ticketTagGroups;
        public ObservableCollection<TicketTagGroupViewModel> TicketTagGroups
        {
            get { return _ticketTagGroups ?? (_ticketTagGroups = new ObservableCollection<TicketTagGroupViewModel>(GetTicketTags(Model))); }
        }

        private ObservableCollection<ServiceTemplateViewModel> _serviceTemplates;
        public ObservableCollection<ServiceTemplateViewModel> ServiceTemplates
        {
            get { return _serviceTemplates ?? (_serviceTemplates = new ObservableCollection<ServiceTemplateViewModel>(GetServiceTemplates(Model))); }
        }

        private ObservableCollection<TableScreen> _posTableScreens;
        public ObservableCollection<TableScreen> PosTableScreens
        {
            get { return _posTableScreens ?? (_posTableScreens = new ObservableCollection<TableScreen>(Model.PosTableScreens.OrderBy(x => x.Order))); }
        }

        private ObservableCollection<TableScreen> _terminalTableScreens;
        public ObservableCollection<TableScreen> TerminalTableScreens
        {
            get { return _terminalTableScreens ?? (_terminalTableScreens = new ObservableCollection<TableScreen>(Model.TerminalTableScreens.OrderBy(x => x.Order))); }
        }

        private ObservableCollection<OrderTagGroupViewModel> _orderTagGroups;
        public ObservableCollection<OrderTagGroupViewModel> OrderTagGroups
        {
            get { return _orderTagGroups ?? (_orderTagGroups = new ObservableCollection<OrderTagGroupViewModel>(GetOrderTagGroups(Model))); }
        }

        private IEnumerable<Numerator> _numerators;
        public IEnumerable<Numerator> Numerators { get { return _numerators ?? (_numerators = Workspace.All<Numerator>()); } set { _numerators = value; } }

        public int ScreenMenuId { get { return Model.ScreenMenuId; } set { Model.ScreenMenuId = value; } }
        public int TerminalScreenMenuId { get { return Model.TerminalScreenMenuId; } set { Model.TerminalScreenMenuId = value; } }

        public Numerator TicketNumerator { get { return Model.TicketNumerator; } set { Model.TicketNumerator = value; } }
        public Numerator OrderNumerator { get { return Model.OrderNumerator; } set { Model.OrderNumerator = value; } }

        public int OpenTicketViewColumnCount { get { return Model.OpenTicketViewColumnCount; } set { Model.OpenTicketViewColumnCount = value; } }

        public bool IsFastFood
        {
            get { return Model.IsFastFood; }
            set { Model.IsFastFood = value; }
        }

        public bool IsAlaCarte
        {
            get { return Model.IsAlaCarte; }
            set { Model.IsAlaCarte = value; }
        }

        public bool IsTakeAway
        {
            get { return Model.IsTakeAway; }
            set { Model.IsTakeAway = value; }
        }

        public IEnumerable<string> PriceTags { get { return Dao.Select<MenuItemPriceDefinition, string>(x => x.PriceTag, x => x.Id > 0); } }
        public string PriceTag { get { return Model.PriceTag; } set { Model.PriceTag = value; } }

        public TicketTagGroupViewModel SelectedTicketTag { get; set; }
        public ServiceTemplateViewModel SelectedServiceTemplate { get; set; }
        public TableScreen SelectedPosTableScreen { get; set; }
        public TableScreen SelectedTerminalTableScreen { get; set; }
        public OrderTagGroupViewModel SelectedOrderTagGroup { get; set; }

        public ICaptionCommand AddTicketTagGroupCommand { get; set; }
        public ICaptionCommand DeleteTicketTagGroupCommand { get; set; }
        public ICaptionCommand AddServiceTemplateCommand { get; set; }
        public ICaptionCommand DeleteServiceTemplateCommand { get; set; }
        public ICaptionCommand AddPosTableScreenCommand { get; set; }
        public ICaptionCommand DeletePosTableScreenCommand { get; set; }
        public ICaptionCommand AddTerminalTableScreenCommand { get; set; }
        public ICaptionCommand DeleteTerminalTableScreenCommand { get; set; }
        public ICaptionCommand AddOrderTagGroupCommand { get; set; }
        public ICaptionCommand DeleteOrderTagGroupCommand { get; set; }

        public DepartmentViewModel(Department model)
            : base(model)
        {
            AddTicketTagGroupCommand = new CaptionCommand<string>(string.Format(Resources.Add_f, Resources.TagGroup), OnAddTicketTagGroup);
            DeleteTicketTagGroupCommand = new CaptionCommand<string>(string.Format(Resources.Delete_f, Resources.TagGroup), OnDeleteTicketTagGroup, CanDeleteTicketTagGroup);
            AddServiceTemplateCommand = new CaptionCommand<string>(string.Format(Resources.Add_f, Resources.ServiceTemplate), OnAddServiceTemplate);
            DeleteServiceTemplateCommand = new CaptionCommand<string>(string.Format(Resources.Delete_f, Resources.ServiceTemplate), OnDeleteServiceTempalte, CanDeleteServiceTemplate);
            AddPosTableScreenCommand = new CaptionCommand<string>(string.Format(Resources.Add_f, Resources.TableScreen), OnAddPosTableScreen);
            DeletePosTableScreenCommand = new CaptionCommand<string>(string.Format(Resources.Delete_f, Resources.TableScreen), OnDeletePosTableScreen, CanDeletePosTableScreen);
            AddTerminalTableScreenCommand = new CaptionCommand<string>(string.Format(Resources.Add_f, Resources.TableScreen), OnAddTerminalTableScreen);
            DeleteTerminalTableScreenCommand = new CaptionCommand<string>(string.Format(Resources.Delete_f, Resources.TableScreen), OnDeleteTerminalTableScreen, CanDeleteTerminalTableScreen);
            AddOrderTagGroupCommand = new CaptionCommand<string>(string.Format(Resources.Add_f, Resources.OrderTagGroup), OnAddOrderTagGroup);
            DeleteOrderTagGroupCommand = new CaptionCommand<string>(string.Format(Resources.Delete_f, Resources.OrderTagGroup), OnDeleteOrderTagGroup, CanDeleteOrderTagGroup);
        }

        private bool CanDeleteOrderTagGroup(string arg)
        {
            return SelectedOrderTagGroup != null;
        }

        private void OnDeleteOrderTagGroup(string obj)
        {
            Model.OrderTagGroups.Remove(SelectedOrderTagGroup.Model);
            OrderTagGroups.Remove(SelectedOrderTagGroup);
        }

        private void OnAddOrderTagGroup(string obj)
        {
            var selectedValues =
                  InteractionService.UserIntraction.ChooseValuesFrom(Workspace.All<OrderTagGroup>().ToList<IOrderable>(),
                  Model.TerminalTableScreens.ToList<IOrderable>(), Resources.OrderTagGroups, string.Format(Resources.ChooseServicesForDepartmentHint_f, Model.Name),
                  Resources.OrderTagGroup, Resources.OrderTagGroups);

            foreach (OrderTagGroup selectedValue in selectedValues)
            {
                if (!Model.OrderTagGroups.Contains(selectedValue))
                    Model.OrderTagGroups.Add(selectedValue);
            }

            _orderTagGroups = new ObservableCollection<OrderTagGroupViewModel>(GetOrderTagGroups(Model));

            RaisePropertyChanged(() => OrderTagGroups);
        }

        private bool CanDeleteTerminalTableScreen(string arg)
        {
            return SelectedTerminalTableScreen != null;
        }

        private void OnDeleteTerminalTableScreen(string obj)
        {
            Model.TerminalTableScreens.Remove(SelectedTerminalTableScreen);
            TerminalTableScreens.Remove(SelectedTerminalTableScreen);
        }

        private void OnAddTerminalTableScreen(string obj)
        {
            var selectedValues =
                  InteractionService.UserIntraction.ChooseValuesFrom(Workspace.All<TableScreen>().ToList<IOrderable>(),
                  Model.TerminalTableScreens.ToList<IOrderable>(), Resources.TableScreens, string.Format(Resources.ChooseServicesForDepartmentHint_f, Model.Name),
                  Resources.TableScreen, Resources.TableScreens);

            foreach (TableScreen selectedValue in selectedValues)
            {
                if (!Model.TerminalTableScreens.Contains(selectedValue))
                    Model.TerminalTableScreens.Add(selectedValue);
            }

            _terminalTableScreens = new ObservableCollection<TableScreen>(Model.TerminalTableScreens.OrderBy(x => x.Order));

            RaisePropertyChanged(() => TerminalTableScreens);
        }

        private bool CanDeletePosTableScreen(string arg)
        {
            return SelectedPosTableScreen != null;
        }

        private void OnDeletePosTableScreen(string obj)
        {
            Model.PosTableScreens.Remove(SelectedPosTableScreen);
            PosTableScreens.Remove(SelectedPosTableScreen);
        }

        private void OnAddPosTableScreen(string obj)
        {
            var selectedValues =
                  InteractionService.UserIntraction.ChooseValuesFrom(Workspace.All<TableScreen>().ToList<IOrderable>(),
                  Model.PosTableScreens.ToList<IOrderable>(), Resources.TableScreens, string.Format(Resources.ChooseServicesForDepartmentHint_f, Model.Name),
                  Resources.TableScreen, Resources.TableScreens);

            foreach (TableScreen selectedValue in selectedValues)
            {
                if (!Model.PosTableScreens.Contains(selectedValue))
                    Model.PosTableScreens.Add(selectedValue);
            }

            _posTableScreens = new ObservableCollection<TableScreen>(Model.PosTableScreens.OrderBy(x => x.Order));

            RaisePropertyChanged(() => PosTableScreens);
        }

        private bool CanDeleteServiceTemplate(string arg)
        {
            return SelectedServiceTemplate != null;
        }

        private void OnDeleteServiceTempalte(string obj)
        {
            Model.ServiceTemplates.Remove(SelectedServiceTemplate.Model);
            ServiceTemplates.Remove(SelectedServiceTemplate);
        }

        private void OnAddServiceTemplate(string obj)
        {
            var selectedValues =
              InteractionService.UserIntraction.ChooseValuesFrom(Workspace.All<ServiceTemplate>().ToList<IOrderable>(),
              Model.ServiceTemplates.ToList<IOrderable>(), Resources.ServiceTemplates, string.Format(Resources.ChooseServicesForDepartmentHint_f, Model.Name),
              Resources.ServiceTemplate, Resources.ServiceTemplates);

            foreach (ServiceTemplate selectedValue in selectedValues)
            {
                if (!Model.ServiceTemplates.Contains(selectedValue))
                    Model.ServiceTemplates.Add(selectedValue);
            }

            _serviceTemplates = new ObservableCollection<ServiceTemplateViewModel>(GetServiceTemplates(Model));

            RaisePropertyChanged(() => ServiceTemplates);
        }

        private bool CanDeleteTicketTagGroup(string arg)
        {
            return SelectedTicketTag != null;
        }

        private void OnDeleteTicketTagGroup(string obj)
        {
            Model.TicketTagGroups.Remove(SelectedTicketTag.Model);
            TicketTagGroups.Remove(SelectedTicketTag);
        }

        private void OnAddTicketTagGroup(string obj)
        {
            var selectedValues =
                InteractionService.UserIntraction.ChooseValuesFrom(Workspace.All<TicketTagGroup>().ToList<IOrderable>(),
                Model.TicketTagGroups.ToList<IOrderable>(), Resources.TicketTags, string.Format(Resources.ChooseTagsForDepartmentHint, Model.Name),
                Resources.TicketTag, Resources.TicketTags);

            foreach (TicketTagGroup selectedValue in selectedValues)
            {
                if (!Model.TicketTagGroups.Contains(selectedValue))
                    Model.TicketTagGroups.Add(selectedValue);
            }

            _ticketTagGroups = new ObservableCollection<TicketTagGroupViewModel>(GetTicketTags(Model));

            RaisePropertyChanged(() => TicketTagGroups);
        }

        private static IEnumerable<OrderTagGroupViewModel> GetOrderTagGroups(Department model)
        {
            return model.OrderTagGroups.OrderBy(x => x.Order).Select(x => new OrderTagGroupViewModel(x));
        }

        private static IEnumerable<ServiceTemplateViewModel> GetServiceTemplates(Department model)
        {
            return model.ServiceTemplates.OrderBy(x => x.Order).Select(x => new ServiceTemplateViewModel(x));
        }

        private static IEnumerable<TicketTagGroupViewModel> GetTicketTags(Department model)
        {
            return model.TicketTagGroups.OrderBy(x => x.Order).Select(x => new TicketTagGroupViewModel(x));
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
            RuleFor(x => x.TicketNumerator).NotNull();
            RuleFor(x => x.OrderNumerator).NotNull();
        }
    }
}
