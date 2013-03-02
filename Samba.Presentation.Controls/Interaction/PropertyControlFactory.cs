using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using PropertyTools.Wpf;
using Samba.Presentation.Common.ModelBase;
using ColumnDefinition = System.Windows.Controls.ColumnDefinition;

namespace Samba.Presentation.Controls.Interaction
{
    public class PropertyControlFactory : DefaultPropertyControlFactory
    {
        public override FrameworkElement CreateControl(PropertyItem pi, PropertyControlFactoryOptions options)
        {
            // Check if the property is of type Range
            if (pi.Is(typeof(IValueWithSource)))
            {
                // Create a control to edit the Range
                return this.CreateComboBox(pi, options);
            }

            return base.CreateControl(pi, options);
        }

        protected virtual FrameworkElement CreateComboBox(PropertyItem pi, PropertyControlFactoryOptions options)
        {
            var cbox = new ComboBox() { IsEditable = true };
            cbox.SetBinding(Selector.SelectedItemProperty, new Binding(pi.Descriptor.Name + ".Text"));
            cbox.SetBinding(ItemsControl.ItemsSourceProperty, new Binding(pi.Descriptor.Name + ".Values"));
            return cbox;
        }
    }
}
