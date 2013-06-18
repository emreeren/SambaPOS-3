using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using PropertyTools.Wpf;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Presentation.Controls.Interaction
{
    public class PropertyControlFactory : DefaultPropertyControlFactory
    {
        public override FrameworkElement CreateControl(PropertyItem pi, PropertyControlFactoryOptions options)
        {
            if (pi.Is(typeof(IValueWithSource)))
            {
                return CreateComboBox(pi, options);
            }
            return base.CreateControl(pi, options);
        }

        protected virtual FrameworkElement CreateComboBox(PropertyItem pi, PropertyControlFactoryOptions options)
        {
            var cbox = new ComboBox { IsEditable = true };
            cbox.SetBinding(ComboBox.TextProperty, new Binding(pi.Descriptor.Name + ".Text"));
            cbox.SetBinding(ItemsControl.ItemsSourceProperty, new Binding(pi.Descriptor.Name + ".Values"));
            return cbox;
        }
    }

    public class CustomItemsGridControlFactory : ItemsGridControlFactory
    {
        public override FrameworkElement CreateEditControl(PropertyDefinition d, int index)
        {
            if (d.PropertyName.Contains("Path"))
            {
                return CreateFilePicker(d, index);
            }
            return base.CreateEditControl(d, index);
        }

        private FrameworkElement CreateFilePicker(PropertyDefinition property, int index)
        {
            var c = new FilePicker
            {
                Filter = "Image Files (*.jpg, *.png)|*.jpg;*.png",
                UseOpenDialog = true,
            };
            c.SetBinding(FilePicker.FilePathProperty, property.CreateBinding(index));
            return c;
        }
    }
}
