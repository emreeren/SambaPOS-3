using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Services;

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

        public IEnumerable<string> PriceTags { get { return _priceListService.GetTags(); } }
        public string PriceTag { get { return Model.PriceTag; } set { Model.PriceTag = value; } }

        private IEnumerable<TicketType> _ticketTypes;
        public IEnumerable<TicketType> TicketTypes
        {
            get { return _ticketTypes ?? (_ticketTypes = Workspace.All<TicketType>()); }
        }
        public int TicketTypeId { get { return Model.TicketTypeId; } set { Model.TicketTypeId = value; } }

        public override Type GetViewType()
        {
            return typeof(DepartmentView);
        }

        public override string GetModelTypeString()
        {
            return Resources.Department;
        }
    }


}
