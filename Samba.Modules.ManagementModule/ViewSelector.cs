using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.ManagementModule
{
    internal class ViewSelector : DataTemplateSelector
    {
        private readonly Dictionary<Type, DataTemplate> _templateCache;

        public ViewSelector()
        {
            _templateCache = new Dictionary<Type, DataTemplate>();
        }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is VisibleViewModelBase)
            {
                if (!_templateCache.ContainsKey(item.GetType()))
                {
                    var result = new DataTemplate
                                     {
                                         VisualTree =
                                             new FrameworkElementFactory((item as VisibleViewModelBase).GetViewType())
                                     };

                    _templateCache.Add(item.GetType(), result);
                }
                return _templateCache[item.GetType()];
            }

            return base.SelectTemplate(item, container);
        }
    }
}
