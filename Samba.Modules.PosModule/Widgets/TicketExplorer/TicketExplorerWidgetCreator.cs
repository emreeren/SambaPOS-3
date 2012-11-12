using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Samba.Domain.Models.Resources;
using Samba.Presentation.Common;
using Samba.Presentation.Services;
using Samba.Services;

namespace Samba.Modules.PosModule.Widgets.TicketExplorer
{
    [Export(typeof(IWidgetCreator))]
    class TicketExplorerWidgetCreator : IWidgetCreator
    {
        private readonly IUserService _userService;
        private readonly ITicketService _ticketService;
        private readonly ICacheService _cacheService;

        [ImportingConstructor]
        public TicketExplorerWidgetCreator(IUserService userService, ITicketService ticketService,ICacheService cacheService)
        {
            _userService = userService;
            _ticketService = ticketService;
            _cacheService = cacheService;
        }

        public string GetCreatorName()
        {
            return "TicketExplorerCreator";
        }

        public string GetCreatorDescription()
        {
            return "Ticket Explorer";
        }

        public Widget CreateNewWidget()
        {
            var result = new Widget { CreatorName = GetCreatorName() };
            return result;
        }

        public IDiagram CreateWidgetViewModel(Widget widget,IApplicationState applicationState)
        {
            return new TicketExplorerViewModel(widget,applicationState, _ticketService, _userService,_cacheService);
        }

        public FrameworkElement CreateWidgetControl(IDiagram widgetViewModel, ContextMenu contextMenu)
        {
            var buttonHolder = widgetViewModel as TicketExplorerViewModel;

            var ret = new TicketExplorerView { DataContext = buttonHolder, ContextMenu = contextMenu };

            var heightBinding = new Binding("Height") { Source = buttonHolder, Mode = BindingMode.TwoWay };
            var widthBinding = new Binding("Width") { Source = buttonHolder, Mode = BindingMode.TwoWay };
            var xBinding = new Binding("X") { Source = buttonHolder, Mode = BindingMode.TwoWay };
            var yBinding = new Binding("Y") { Source = buttonHolder, Mode = BindingMode.TwoWay };

            ret.SetBinding(InkCanvas.LeftProperty, xBinding);
            ret.SetBinding(InkCanvas.TopProperty, yBinding);
            ret.SetBinding(FrameworkElement.HeightProperty, heightBinding);
            ret.SetBinding(FrameworkElement.WidthProperty, widthBinding);

            return ret;
        }
    }
}
