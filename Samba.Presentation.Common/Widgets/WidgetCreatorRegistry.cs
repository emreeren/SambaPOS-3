using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Samba.Domain.Models.Entities;
using Samba.Presentation.Services;

namespace Samba.Presentation.Common.Widgets
{
    public static class WidgetCreatorRegistry
    {
        private static readonly Dictionary<string, IWidgetCreator> Creators;
        private static readonly IWidgetCreator DefaultWidgetCreator = new DefaultWidgetCreator();

        static WidgetCreatorRegistry()
        {
            Creators = new Dictionary<string, IWidgetCreator>();
        }

        public static void RegisterWidgetCreator(IWidgetCreator creator)
        {
            Creators.Add(creator.GetCreatorName(), creator);
        }

        public static FrameworkElement CreateWidgetControl(IDiagram widget, ContextMenu contextMenu)
        {
            return GetCreator(widget.CreatorName).CreateWidgetControl(widget, contextMenu);
        }

        public static IDiagram CreateWidgetViewModel(Widget widget, IApplicationState applicationState)
        {
            return GetCreator(widget.CreatorName).CreateWidgetViewModel(widget, applicationState);
        }

        private static IWidgetCreator GetCreator(string creatorName)
        {
            if (!string.IsNullOrEmpty(creatorName) && Creators.ContainsKey(creatorName))
            {
                return Creators[creatorName];
            }
            return DefaultWidgetCreator;
        }

        public static Widget CreateWidgetFor(string creatorName)
        {
            return GetCreator(creatorName).CreateNewWidget();
        }

        public static IEnumerable<IWidgetCreator> GetCreators()
        {
            return Creators.Values;
        }
    }
}
