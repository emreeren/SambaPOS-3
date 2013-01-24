using System.ComponentModel;
using Samba.Domain.Models.Resources;
using Samba.Presentation.Common;
using Samba.Presentation.Services;
using Samba.Services;

namespace Samba.Modules.TicketModule.Widgets.TicketExplorer
{
    class TicketExplorerWidgetViewModel : WidgetViewModel
    {
        public TicketExplorerWidgetViewModel(Widget model, IApplicationState applicationState,
            ITicketService ticketService, IUserService userService, ICacheService cacheService)
            : base(model, applicationState)
        {
            TicketExplorerViewModel = new TicketExplorerViewModel(ticketService, userService, cacheService);
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
