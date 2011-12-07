using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using FluentValidation;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Data;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Common.Services;

namespace Samba.Modules.TicketModule
{
    class TicketTemplateViewModel : EntityViewModelBase<TicketTemplate>
    {
        public TicketTemplateViewModel(TicketTemplate model) : base(model)
        {
            AddTicketTagGroupCommand = new CaptionCommand<string>(string.Format(Resources.Select_f, Resources.TicketTagGroup), OnAddTicketTagGroup);
            DeleteTicketTagGroupCommand = new CaptionCommand<string>(string.Format(Resources.Delete_f, Resources.TicketTagGroup), OnDeleteTicketTagGroup, CanDeleteTicketTagGroup);
            AddOrderTagGroupCommand = new CaptionCommand<string>(string.Format(Resources.Select_f, Resources.OrderTagGroup), OnAddOrderTagGroup);
            DeleteOrderTagGroupCommand = new CaptionCommand<string>(string.Format(Resources.Delete_f, Resources.OrderTagGroup), OnDeleteOrderTagGroup, CanDeleteOrderTagGroup);
            AddServiceTemplateCommand = new CaptionCommand<string>(string.Format(Resources.Select_f, Resources.ServiceTemplate), OnAddServiceTemplate);
            DeleteServiceTemplateCommand = new CaptionCommand<string>(string.Format(Resources.Delete_f, Resources.ServiceTemplate), OnDeleteServiceTempalte, CanDeleteServiceTemplate);
        }

        public ICaptionCommand AddTicketTagGroupCommand { get; set; }
        public ICaptionCommand DeleteTicketTagGroupCommand { get; set; }
        public ICaptionCommand AddOrderTagGroupCommand { get; set; }
        public ICaptionCommand DeleteOrderTagGroupCommand { get; set; }
        public ICaptionCommand AddServiceTemplateCommand { get; set; }
        public ICaptionCommand DeleteServiceTemplateCommand { get; set; }

        private IEnumerable<Numerator> _numerators;
        public IEnumerable<Numerator> Numerators { get { return _numerators ?? (_numerators = Workspace.All<Numerator>()); } set { _numerators = value; } }

        public Numerator TicketNumerator { get { return Model.TicketNumerator; } set { Model.TicketNumerator = value; } }
        public Numerator OrderNumerator { get { return Model.OrderNumerator; } set { Model.OrderNumerator = value; } }

        public IEnumerable<string> PriceTags { get { return Dao.Select<MenuItemPriceDefinition, string>(x => x.PriceTag, x => x.Id > 0); } }
        public string PriceTag { get { return Model.PriceTag; } set { Model.PriceTag = value; } }
        
        public TicketTagGroupViewModel SelectedTicketTag { get; set; }
        public OrderTagGroupViewModel SelectedOrderTagGroup { get; set; }
        public ServiceTemplateViewModel SelectedServiceTemplate { get; set; }
        
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

        private ObservableCollection<OrderTagGroupViewModel> _orderTagGroups;
        public ObservableCollection<OrderTagGroupViewModel> OrderTagGroups
        {
            get { return _orderTagGroups ?? (_orderTagGroups = new ObservableCollection<OrderTagGroupViewModel>(GetOrderTagGroups(Model))); }
        }

        private static IEnumerable<TicketTagGroupViewModel> GetTicketTags(TicketTemplate model)
        {
            return model.TicketTagGroups.OrderBy(x => x.Order).Select(x => new TicketTagGroupViewModel(x));
        }

        private static IEnumerable<OrderTagGroupViewModel> GetOrderTagGroups(TicketTemplate model)
        {
            return model.OrderTagGroups.OrderBy(x => x.Order).Select(x => new OrderTagGroupViewModel(x));
        }

        private static IEnumerable<ServiceTemplateViewModel> GetServiceTemplates(TicketTemplate model)
        {
            return model.ServiceTemplates.OrderBy(x => x.Order).Select(x => new ServiceTemplateViewModel(x));
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
                  Model.OrderTagGroups.ToList<IOrderable>(), Resources.OrderTagGroups, string.Format(Resources.ChooseServicesForDepartmentHint_f, Model.Name),
                  Resources.OrderTagGroup, Resources.OrderTagGroups);

            foreach (OrderTagGroup selectedValue in selectedValues)
            {
                if (!Model.OrderTagGroups.Contains(selectedValue))
                    Model.OrderTagGroups.Add(selectedValue);
            }

            _orderTagGroups = new ObservableCollection<OrderTagGroupViewModel>(GetOrderTagGroups(Model));

            RaisePropertyChanged(() => OrderTagGroups);
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

        public override Type GetViewType()
        {
            return typeof (TicketTemplateView);
        }

        public override string GetModelTypeString()
        {
            return Resources.TicketTemplate;
        }

        protected override AbstractValidator<TicketTemplate> GetValidator()
        {
            return new TicketTemplateValidator();
        }
    }

    internal class TicketTemplateValidator : EntityValidator<TicketTemplate>
    {
        public TicketTemplateValidator()
        {
            RuleFor(x => x.TicketNumerator).NotNull();
            RuleFor(x => x.OrderNumerator).NotNull();
        }
    }
}
