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

        private IEnumerable<TicketType> _ticketTypes;
        public IEnumerable<TicketType> TicketTypes
        {
            get { return _ticketTypes ?? (_ticketTypes = Workspace.All<TicketType>()); }
        }
        public TicketType TicketType { get { return Model.TicketType; } set { Model.TicketType = value; } }

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
            RuleFor(x => x.TicketType).NotNull();
        }
    }
}
