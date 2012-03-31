using System.Windows;
using System.Windows.Controls;

namespace Samba.Presentation.ViewModels
{
    public class ResourceCustomFieldTemplateSelector : DataTemplateSelector
    {
        public DataTemplate TextTemplate { get; set; }
        public DataTemplate WideTextTemplate { get; set; }
        public DataTemplate MaskedTemplate { get; set; }
        public DataTemplate NumberTemplate { get; set; }
        public DataTemplate ComboBoxTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var pv = item as CustomDataValue;
            if (pv != null)
            {
                if (!string.IsNullOrEmpty(pv.CustomField.EditingFormat)) return MaskedTemplate;
                if (pv.CustomField.IsWideString) return WideTextTemplate;
                if (pv.CustomField.IsNumber) return NumberTemplate;
                if (!string.IsNullOrEmpty(pv.CustomField.ValueSource)) return ComboBoxTemplate;
            }
            return TextTemplate;
        }
    }
}
