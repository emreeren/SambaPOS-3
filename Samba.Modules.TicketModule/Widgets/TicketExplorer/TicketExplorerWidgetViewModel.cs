using System.ComponentModel;
using Samba.Domain.Models.Entities;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Widgets;
using Samba.Presentation.Services;
using Samba.Services;

namespace Samba.Modules.TicketModule.Widgets.TicketExplorer
{
    class TicketExplorerWidgetViewModel : WidgetViewModel
    {
        public TicketExplorerWidgetViewModel(Widget model, IApplicationState applicationState,
             ITicketServiceBase ticketServiceBase, IUserService userService, ICacheService cacheService)
            : base(model, applicationState)
        {
            TicketExplorerViewModel = new TicketExplorerViewModel(ticketServiceBase, userService, cacheService, applicationState);
        }

        [Browsable(false)]
        public TicketExplorerViewModel TicketExplorerViewModel { get; private set; }

        protected override object CreateSettingsObject()
        {
            return null;
        }

        public override void Refresh()
        {
            TicketExplorerViewModel.Refresh();
        }
    }
}
