using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Samba.Domain.Models.Resources;
using Samba.Infrastructure;
using Samba.Infrastructure.Helpers;
using Samba.Presentation.Common;
using Samba.Presentation.Services;
using Samba.Services;

namespace Samba.Modules.TaskModule.Widgets.TaskEditor
{
    [Export(typeof(IWidgetCreator))]
    class TaskEditorWidgetCreator : IWidgetCreator
    {
        private readonly ITaskService _taskService;
        private readonly IPresentationCacheService _cacheService;

        [ImportingConstructor]
        public TaskEditorWidgetCreator(ITaskService taskService, IPresentationCacheService cacheService)
        {
            _taskService = taskService;
            _cacheService = cacheService;
        }

        public string GetCreatorName()
        {
            return "TaskEditorCreator";
        }

        public string GetCreatorDescription()
        {
            return "Task Editor";
        }

        public Widget CreateNewWidget()
        {
            var parameters = JsonHelper.Serialize(new TaskEditorWidgetSettings());
            var result = new Widget { Properties = parameters, CreatorName = GetCreatorName() };
            return result;
        }

        public IDiagram CreateWidgetViewModel(Widget widget, IApplicationState applicationState)
        {
            return new TaskEditorViewModel(widget, applicationState, _taskService, _cacheService);
        }

        public FrameworkElement CreateWidgetControl(IDiagram widgetViewModel, ContextMenu contextMenu)
        {
            var buttonHolder = widgetViewModel as TaskEditorViewModel;

            var ret = new TaskEditorView { DataContext = buttonHolder, ContextMenu = contextMenu };
            var heightBinding = new Binding("Height") { Source = buttonHolder, Mode = BindingMode.TwoWay };
            var widthBinding = new Binding("Width") { Source = buttonHolder, Mode = BindingMode.TwoWay };
            var xBinding = new Binding("X") { Source = buttonHolder, Mode = BindingMode.TwoWay };
            var yBinding = new Binding("Y") { Source = buttonHolder, Mode = BindingMode.TwoWay };
            var transformBinding = new Binding("ScaleTransform") { Source = buttonHolder, Mode = BindingMode.OneWay };

            ret.SetBinding(InkCanvas.LeftProperty, xBinding);
            ret.SetBinding(InkCanvas.TopProperty, yBinding);
            ret.SetBinding(FrameworkElement.HeightProperty, heightBinding);
            ret.SetBinding(FrameworkElement.WidthProperty, widthBinding);
            ret.Border.SetBinding(FrameworkElement.LayoutTransformProperty, transformBinding);
            return ret;
        }
    }
}
