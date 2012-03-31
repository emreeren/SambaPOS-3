using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using FluentValidation;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Resources;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Data;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Common.Services;

namespace Samba.Modules.TicketModule
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    class TicketTemplateViewModel : EntityViewModelBase<TicketTemplate>
    {
        [ImportingConstructor]
        public TicketTemplateViewModel()
        {
            AddTicketTagGroupCommand = new CaptionCommand<string>(string.Format(Resources.Select_f, Resources.TicketTagGroup), OnAddTicketTagGroup);
            DeleteTicketTagGroupCommand = new CaptionCommand<string>(string.Format(Resources.Delete_f, Resources.TicketTagGroup), OnDeleteTicketTagGroup, CanDeleteTicketTagGroup);
            AddOrderTagGroupCommand = new CaptionCommand<string>(string.Format(Resources.Select_f, Resources.OrderTagGroup), OnAddOrderTagGroup);
            DeleteOrderTagGroupCommand = new CaptionCommand<string>(string.Format(Resources.Delete_f, Resources.OrderTagGroup), OnDeleteOrderTagGroup, CanDeleteOrderTagGroup);
            AddCalculationTemplateCommand = new CaptionCommand<string>(string.Format(Resources.Select_f, Resources.CalculationTemplate), OnAddCalculationTemplate);
            DeleteCalculationTemplateCommand = new CaptionCommand<string>(string.Format(Resources.Delete_f, Resources.CalculationTemplate), OnDeleteCalculationTempalte, CanDeleteCalculationTemplate);
            AddPaymentTemplateCommand = new CaptionCommand<string>(string.Format(Resources.Select_f, Resources.PaymentTemplate), OnAddPaymentTemplate);
            DeletePaymentTemplateCommand = new CaptionCommand<string>(string.Format(Resources.Delete_f, Resources.PaymentTemplate), OnDeletePaymentTemplate, CanDeletePaymentTemplate);
        }

        public ICaptionCommand AddTicketTagGroupCommand { get; set; }
        public ICaptionCommand DeleteTicketTagGroupCommand { get; set; }
        public ICaptionCommand AddOrderTagGroupCommand { get; set; }
        public ICaptionCommand DeleteOrderTagGroupCommand { get; set; }
        public ICaptionCommand AddCalculationTemplateCommand { get; set; }
        public ICaptionCommand DeleteCalculationTemplateCommand { get; set; }
        public ICaptionCommand AddPaymentTemplateCommand { get; set; }
        public ICaptionCommand DeletePaymentTemplateCommand { get; set; }

        private IEnumerable<Numerator> _numerators;
        public IEnumerable<Numerator> Numerators { get { return _numerators ?? (_numerators = Workspace.All<Numerator>()); } set { _numerators = value; } }

        public Numerator TicketNumerator { get { return Model.TicketNumerator; } set { Model.TicketNumerator = value; } }
        public Numerator OrderNumerator { get { return Model.OrderNumerator; } set { Model.OrderNumerator = value; } }

        public TicketTagGroup SelectedTicketTag { get; set; }
        public OrderTagGroup SelectedOrderTagGroup { get; set; }
        public CalculationTemplate SelectedCalculationTemplate { get; set; }
        public PaymentTemplate SelectedPaymentTemplate { get; set; }

        private IEnumerable<AccountTransactionTemplate> _accountTransactionTemplates;
        public IEnumerable<AccountTransactionTemplate> AccountTransactionTemplates { get { return _accountTransactionTemplates ?? (_accountTransactionTemplates = Workspace.All<AccountTransactionTemplate>()); } }

        public AccountTransactionTemplate SaleTransactionTemplate { get { return Model.SaleTransactionTemplate; } set { Model.SaleTransactionTemplate = value; } }

        private ObservableCollection<TicketTagGroup> _ticketTagGroups;
        public ObservableCollection<TicketTagGroup> TicketTagGroups
        {
            get { return _ticketTagGroups ?? (_ticketTagGroups = new ObservableCollection<TicketTagGroup>(GetTicketTags(Model))); }
        }

        private ObservableCollection<CalculationTemplate> _calculationTemplates;
        public ObservableCollection<CalculationTemplate> CalculationTemplates
        {
            get { return _calculationTemplates ?? (_calculationTemplates = new ObservableCollection<CalculationTemplate>(GetCalculationTemplates(Model))); }
        }

        private ObservableCollection<PaymentTemplate> _paymentTemplates;
        public ObservableCollection<PaymentTemplate> PaymentTemplates
        {
            get { return _paymentTemplates ?? (_paymentTemplates = new ObservableCollection<PaymentTemplate>(GetPaymentTemplates(Model))); }
        }

        private ObservableCollection<OrderTagGroup> _orderTagGroups;
        public ObservableCollection<OrderTagGroup> OrderTagGroups
        {
            get { return _orderTagGroups ?? (_orderTagGroups = new ObservableCollection<OrderTagGroup>(GetOrderTagGroups(Model))); }
        }

        private static IEnumerable<TicketTagGroup> GetTicketTags(TicketTemplate model)
        {
            return model.TicketTagGroups.OrderBy(x => x.Order);
        }

        private static IEnumerable<OrderTagGroup> GetOrderTagGroups(TicketTemplate model)
        {
            return model.OrderTagGroups.OrderBy(x => x.Order);
        }

        private static IEnumerable<CalculationTemplate> GetCalculationTemplates(TicketTemplate model)
        {
            return model.CalulationTemplates.OrderBy(x => x.Order);
        }

        private static IEnumerable<PaymentTemplate> GetPaymentTemplates(TicketTemplate model)
        {
            return model.PaymentTemplates.OrderBy(x => x.Order);
        }

        private bool CanDeleteOrderTagGroup(string arg)
        {
            return SelectedOrderTagGroup != null;
        }

        private void OnDeleteOrderTagGroup(string obj)
        {
            Model.OrderTagGroups.Remove(SelectedOrderTagGroup);
            OrderTagGroups.Remove(SelectedOrderTagGroup);
        }

        private void OnAddOrderTagGroup(string obj)
        {
            var selectedValues =
                  InteractionService.UserIntraction.ChooseValuesFrom(Workspace.All<OrderTagGroup>().ToList<IOrderable>(),
                  Model.OrderTagGroups.ToList<IOrderable>(), Resources.OrderTagGroups, string.Format(Resources.ChooseTagsForDepartmentHint, Model.Name),
                  Resources.OrderTagGroup, Resources.OrderTagGroups);

            foreach (OrderTagGroup selectedValue in selectedValues)
            {
                if (!Model.OrderTagGroups.Contains(selectedValue))
                    Model.OrderTagGroups.Add(selectedValue);
            }

            _orderTagGroups = new ObservableCollection<OrderTagGroup>(GetOrderTagGroups(Model));

            RaisePropertyChanged(() => OrderTagGroups);
        }

        private bool CanDeleteTicketTagGroup(string arg)
        {
            return SelectedTicketTag != null;
        }

        private void OnDeleteTicketTagGroup(string obj)
        {
            Model.TicketTagGroups.Remove(SelectedTicketTag);
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

            _ticketTagGroups = new ObservableCollection<TicketTagGroup>(GetTicketTags(Model));

            RaisePropertyChanged(() => TicketTagGroups);
        }

        private bool CanDeletePaymentTemplate(string arg)
        {
            return SelectedPaymentTemplate != null;
        }

        private void OnDeletePaymentTemplate(string obj)
        {
            Model.PaymentTemplates.Remove(SelectedPaymentTemplate);
            PaymentTemplates.Remove(SelectedPaymentTemplate);
        }

        private void OnAddPaymentTemplate(string obj)
        {
            var selectedValues =
              InteractionService.UserIntraction.ChooseValuesFrom(Workspace.All<PaymentTemplate>().ToList<IOrderable>(),
              Model.PaymentTemplates.ToList<IOrderable>(), Resources.PaymentTemplates, string.Format(Resources.ChoosePaymentsForTicketTemplate_f, Model.Name),
              Resources.PaymentTemplate, Resources.PaymentTemplates);

            foreach (PaymentTemplate selectedValue in selectedValues)
            {
                if (!Model.PaymentTemplates.Contains(selectedValue))
                    Model.PaymentTemplates.Add(selectedValue);
            }

            _paymentTemplates = new ObservableCollection<PaymentTemplate>(GetPaymentTemplates(Model));

            RaisePropertyChanged(() => PaymentTemplates);
        }


        private bool CanDeleteCalculationTemplate(string arg)
        {
            return SelectedCalculationTemplate != null;
        }

        private void OnDeleteCalculationTempalte(string obj)
        {
            Model.CalulationTemplates.Remove(SelectedCalculationTemplate);
            CalculationTemplates.Remove(SelectedCalculationTemplate);
        }

        private void OnAddCalculationTemplate(string obj)
        {
            var selectedValues =
              InteractionService.UserIntraction.ChooseValuesFrom(Workspace.All<CalculationTemplate>().ToList<IOrderable>(),
              Model.CalulationTemplates.ToList<IOrderable>(), Resources.CalculationTemplates, string.Format(Resources.ChooseCalculationsForTicketTemplate_f, Model.Name),
              Resources.CalculationTemplate, Resources.CalculationTemplates);

            foreach (CalculationTemplate selectedValue in selectedValues)
            {
                if (!Model.CalulationTemplates.Contains(selectedValue))
                    Model.CalulationTemplates.Add(selectedValue);
            }

            _calculationTemplates = new ObservableCollection<CalculationTemplate>(GetCalculationTemplates(Model));

            RaisePropertyChanged(() => CalculationTemplates);
        }

        public override Type GetViewType()
        {
            return typeof(TicketTemplateView);
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
            RuleFor(x => x.SaleTransactionTemplate).NotNull();
            RuleFor(x => x.SaleTransactionTemplate.DefaultSourceAccountId).GreaterThan(0).When(x => x.SaleTransactionTemplate != null);
            RuleFor(x => x.TicketNumerator).NotEqual(x => x.OrderNumerator).When(x => x.TicketNumerator != null);
        }
    }
}