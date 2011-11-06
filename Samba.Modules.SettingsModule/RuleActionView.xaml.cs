using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Samba.Modules.SettingsModule
{
    /// <summary>
    /// Interaction logic for RuleActionView.xaml
    /// </summary>
    public partial class RuleActionView : UserControl
    {
        public RuleActionView()
        {
            InitializeComponent();
        }
    }

    public class PropertyEditorTemplateSelector : DataTemplateSelector
    {
        public DataTemplate TextTemplate { get; set; }
        public DataTemplate ValueTemplate { get; set; }
        public DataTemplate PasswordTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item,
          DependencyObject container)
        {
            var pv = item as ParameterValue;
            if (pv != null)
            {
                if (pv.Name.Contains("Password")) return PasswordTemplate;
                if (pv.Values.Count() > 0) return ValueTemplate;
            }
            return TextTemplate;
        }
    }
}
