using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Samba.Presentation.Services.Common;
using Samba.Services;

namespace Samba.Modules.AutomationModule
{
    public class ParameterValueTemplateSelector : DataTemplateSelector
    {
        public DataTemplate TextTemplate { get; set; }
        public DataTemplate ValueTemplate { get; set; }
        public DataTemplate PasswordTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var pv = item as IParameterValue;
            if (pv != null)
            {
                if (pv.Name.Contains("Password")) return PasswordTemplate;
                if (pv.Values.Count() > 0) return ValueTemplate;
            }
            return TextTemplate;
        }
    }
}