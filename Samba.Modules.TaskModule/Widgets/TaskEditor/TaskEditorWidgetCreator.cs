using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Samba.Domain.Models.Resources;
using Samba.Presentation.Common;
using Samba.Services;

namespace Samba.Modules.TaskModule.Widgets.TaskEditor
{
    [Export(typeof(IWidgetCreator))]
    class TaskEditorWidgetCreator : IWidgetCreator
    {
        private readonly ITaskService _taskService;

        [ImportingConstructor]
        public TaskEditorWidgetCreator(ITaskService taskService)
        {
            _taskService = taskService;
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
            var result = new Widget { CreatorName = GetCreatorName() };
            return result;
        }

        public IDiagram CreateWidgetViewModel(Widget widget)
        {
            return new TaskEditorViewModel(widget, _taskService);
        }

        public FrameworkElement CreateWidgetControl(IDiagram widgetViewModel, ContextMenu contextMenu)
        {
            var buttonHolder = widgetViewModel as TaskEditorViewModel;

            var ret = new TaskEditorView { DataContext = buttonHolder, ContextMenu = contextMenu };

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
