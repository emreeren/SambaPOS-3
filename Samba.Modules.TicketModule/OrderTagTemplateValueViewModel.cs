using System.Collections.Generic;
using System.Linq;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Data;
using Samba.Localization.Properties;
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

        public string OrderTagGroupName
        {
            get
            {
                return Model.OrderTagGroup != null ? Model.OrderTagGroup.Name : Resources.Select;
            }
            set
            {
                Model.OrderTagGroup = value != null ? OrderTagGroups.FirstOrDefault(x => x.Name == value) : null;
                RaisePropertyChanged(() => OrderTags);
                RaisePropertyChanged(() => OrderTagGroupName);
            }
        }

        public string OrderTagName
        {
            get
            {
                return Model.OrderTag != null ? Model.OrderTag.Name : Resources.Select;
            }
            set
            {
                Model.OrderTag = value != null && OrderTags != null ? OrderTags.FirstOrDefault(x => x.Name == value) : null;
                RaisePropertyChanged(() => OrderTagName);
            }
        }

        public OrderTagGroup OrderTagGroup
        {
            get { return Model.OrderTagGroup; }
            set { Model.OrderTagGroup = value; RaisePropertyChanged(() => OrderTags); }
        }

        public OrderTag OrderTag
        {
            get { return Model.OrderTag; }
            set { Model.OrderTag = value; }
        }

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
