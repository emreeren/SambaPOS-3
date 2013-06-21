using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Samba.Domain.Models.Entities;
using Samba.Persistance;
using Samba.Presentation.Common.Widgets;
using Samba.Presentation.Services;
using Samba.Services;

namespace Samba.Modules.TicketModule.Widgets.TicketLister
{
    [Export(typeof(IWidgetCreator))]
    class TicketListerWidgetCreator : IWidgetCreator
    {
        private readonly ITicketServiceBase _ticketServiceBase;
        private readonly IPrinterService _printerService;
        private readonly ICacheService _cacheService;
        private readonly IAutomationDao _automationDao;

        [ImportingConstructor]
        public TicketListerWidgetCreator(ITicketServiceBase ticketServiceBase, IPrinterService printerService,
            ICacheService cacheService, IAutomationDao automationDao)
        {
            _ticketServiceBase = ticketServiceBase;
            _printerService = printerService;
            _cacheService = cacheService;
            _automationDao = automationDao;
        }

        public string GetCreatorName()
        {
            return "TicketListerCreator";
        }

        public string GetCreatorDescription()
        {
            return "Ticket Lister";
        }

        public FrameworkElement CreateWidgetControl(IDiagram widget, ContextMenu contextMenu)
        {
            var viewModel = widget as TicketListerWidgetViewModel;

            var ret = new TicketListerControl { DataContext = viewModel, ContextMenu = contextMenu };

            var heightBinding = new Binding("Height") { Source = viewModel, Mode = BindingMode.TwoWay };
            var widthBinding = new Binding("Width") { Source = viewModel, Mode = BindingMode.TwoWay };
            var xBinding = new Binding("X") { Source = viewModel, Mode = BindingMode.TwoWay };
            var yBinding = new Binding("Y") { Source = viewModel, Mode = BindingMode.TwoWay };
            var fontNameBinding = new Binding("FontName") { Source = viewModel };
            var transformBinding = new Binding("RenderTransform") { Source = viewModel, Mode = BindingMode.OneWay };
            var scaleTransformBinding = new Binding("ScaleTransform") { Source = viewModel, Mode = BindingMode.OneWay };

            ret.SetBinding(InkCanvas.LeftProperty, xBinding);
            ret.SetBinding(InkCanvas.TopProperty, yBinding);
            ret.SetBinding(FrameworkElement.HeightProperty, heightBinding);
            ret.SetBinding(FrameworkElement.WidthProperty, widthBinding);
            ret.SetBinding(UIElement.RenderTransformProperty, transformBinding);
            ret.SetBinding(Control.FontFamilyProperty, fontNameBinding);
            ret.ListBox.SetBinding(FrameworkElement.LayoutTransformProperty, scaleTransformBinding);
            
            return ret;
        }

        public Widget CreateNewWidget()
        {
            var result = new Widget { CreatorName = GetCreatorName() };
            return result;
        }

        public IDiagram CreateWidgetViewModel(Widget widget, IApplicationState applicationState)
        {
            return new TicketListerWidgetViewModel(widget, applicationState, _ticketServiceBase, _printerService, _cacheService, _automationDao);
        }
    }
}
