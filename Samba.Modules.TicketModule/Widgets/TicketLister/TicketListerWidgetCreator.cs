using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Samba.Domain.Models.Entities;
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
        private readonly ISettingService _settingService;

        [ImportingConstructor]
        public TicketListerWidgetCreator(ITicketServiceBase ticketServiceBase, IPrinterService printerService, ISettingService settingService)
        {
            _ticketServiceBase = ticketServiceBase;
            _printerService = printerService;
            _settingService = settingService;
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

            ret.SetBinding(InkCanvas.LeftProperty, xBinding);
            ret.SetBinding(InkCanvas.TopProperty, yBinding);
            ret.SetBinding(FrameworkElement.HeightProperty, heightBinding);
            ret.SetBinding(FrameworkElement.WidthProperty, widthBinding);
            ret.SetBinding(UIElement.RenderTransformProperty, transformBinding);
            ret.SetBinding(Control.FontFamilyProperty, fontNameBinding);

            return ret;
        }

        public Widget CreateNewWidget()
        {
            var result = new Widget { CreatorName = GetCreatorName() };
            return result;
        }

        public IDiagram CreateWidgetViewModel(Widget widget, IApplicationState applicationState)
        {
            return new TicketListerWidgetViewModel(widget, applicationState, _ticketServiceBase, _printerService, _settingService);
        }
    }
}
