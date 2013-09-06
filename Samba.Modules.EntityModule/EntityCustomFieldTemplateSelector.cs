using System.Windows;
using System.Windows.Controls;
using Samba.Domain.Models.Entities;

namespace Samba.Modules.EntityModule
{
    public class EntityCustomFieldTemplateSelector : DataTemplateSelector
    {
        public DataTemplate TextTemplate { get; set; }
        public DataTemplate WideTextTemplate { get; set; }
        public DataTemplate MaskedTemplate { get; set; }
        public DataTemplate NumberTemplate { get; set; }
        public DataTemplate ComboBoxTemplate { get; set; }
        public DataTemplate DateTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var pv = item as CustomDataValueViewModel;
            if (pv != null)
            {
                if (!pv.CustomField.IsQuery && !string.IsNullOrEmpty(pv.CustomField.EditingFormat)) return MaskedTemplate;
                if (pv.CustomField.IsWideString) return WideTextTemplate;
                if (pv.CustomField.IsNumber) return NumberTemplate;
                if (pv.CustomField.IsDate) return DateTemplate;
                if (string.IsNullOrEmpty(pv.CustomField.EditingFormat) && !string.IsNullOrEmpty(pv.CustomField.ValueSource)) return ComboBoxTemplate;
            }
            return TextTemplate;
        }
    }
}
