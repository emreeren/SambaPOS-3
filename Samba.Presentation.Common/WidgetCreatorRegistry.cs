using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace Samba.Presentation.Common
{
    public static class WidgetCreatorRegistry
    {
        private static readonly Dictionary<Type, IWidgetCreator> Creators;

        static WidgetCreatorRegistry()
        {
            Creators = new Dictionary<Type, IWidgetCreator>();
        }

        public static void RegisterWidgetCreator(IWidgetCreator creator)
        {
            Creators.Add(creator.GetWidgetType(), creator);
        }

        public static ContentControl CreateWidget(IDiagram widgetContainer, ContextMenu contextMenu)
        {
            if (Creators.ContainsKey(widgetContainer.GetType()))
            {
                return Creators[widgetContainer.GetType()].CreateWidget(widgetContainer,contextMenu);
            }
            return null;
        }
    }


    public interface IWidgetCreator
    {
        Type GetWidgetType();
        ContentControl CreateWidget(IDiagram widgetContainer, ContextMenu contextMenu);
    }
}
