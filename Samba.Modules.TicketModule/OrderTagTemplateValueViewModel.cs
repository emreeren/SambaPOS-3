using System.Collections.Generic;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Data;
using Samba.Presentation.Common;

namespace Samba.Modules.TicketModule
{
    class OrderTagTemplateValueViewModel : ObservableObject
    {
        private readonly IWorkspace _workspace;

        public OrderTagTemplateValueViewModel(IWorkspace workspace, OrderTagTemplateValue model)
        {
            Model = model;
            _workspace = workspace;
        }

        public OrderTagTemplateValue Model { get; set; }

        public OrderTagGroup OrderTagGroup
        {
            get { return Model.OrderTagGroup; }
            set { Model.OrderTagGroup = value; RaisePropertyChanged(() => OrderTags); }
        }
        public OrderTag OrderTag { get { return Model.OrderTag; } set { Model.OrderTag = value; } }

        private IEnumerable<OrderTagGroup> _orderTagGroups;

        public IEnumerable<OrderTagGroup> OrderTagGroups
        {
            get { return _orderTagGroups ?? (_orderTagGroups = _workspace.All<OrderTagGroup>(x => x.OrderTags)); }
        }

        public IEnumerable<OrderTag> OrderTags
        {
            get
            {
                return OrderTagGroup != null ? OrderTagGroup.OrderTags : null;
            }
        }
    }
}
