using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using FluentValidation;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;
using Samba.Services;

namespace Samba.Modules.DepartmentModule
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class DepartmentViewModel : EntityViewModelBase<Department>
    {
        private readonly IPriceListService _priceListService;

        [ImportingConstructor]
        public DepartmentViewModel(IMenuService menuService, IPriceListService priceListService)
        {
            _priceListService = priceListService;
        }

        private readonly IList<string> _ticketCreationMethods = new[] { string.Format(Resources.Select_f, Resources.Resource), string.Format(Resources.Create_f, Resources.Ticket) };
        public IList<string> TicketCreationMethods { get { return _ticketCreationMethods; } }
        public string TicketCreationMethod { get { return _ticketCreationMethods[Model.TicketCreationMethod]; } set { Model.TicketCreationMethod = _ticketCreationMethods.IndexOf(value); } }

        private IEnumerable<TicketTemplate> _ticketTemplates;
        public IEnumerable<TicketTemplate> TicketTemplates
        {
            get { return _ticketTemplates ?? (_ticketTemplates = Workspace.All<TicketTemplate>()); }
        }
        public TicketTemplate TicketTemplate { get { return Model.TicketTemplate; } set { Model.TicketTemplate = value; } }

        public IEnumerable<string> PriceTags { get { return _priceListService.GetTags(); } }
        public string PriceTag { get { return Model.PriceTag; } set { Model.PriceTag = value; } }

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
