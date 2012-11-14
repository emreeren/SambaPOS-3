using System;
using System.Windows;
using System.Windows.Controls;

namespace Samba.Presentation.Controls.FxButton
{
    public class ButtonContentTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DefaultTemplate { get; set; }
        public DataTemplate StringTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item,
          DependencyObject container)
        {
            if (item is String) return StringTemplate;
            return null;
        }
    }
}
