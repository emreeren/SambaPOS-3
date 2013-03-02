using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PropertyTools.Wpf;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Common.Services;

namespace Samba.Presentation.Controls.Interaction
{
    /// <summary>
    /// Interaction logic for PropertyEditorForm.xaml
    /// </summary>
    public partial class PropertyEditorForm : Window
    {
        public PropertyEditorForm()
        {
            InitializeComponent();
            Height = Properties.Settings.Default.PEHeight;
            Width = Properties.Settings.Default.PEWidth;
            PropertyEditorControl.PropertyControlFactory = new PropertyControlFactory();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Properties.Settings.Default.PEHeight = Height;
            Properties.Settings.Default.PEWidth = Width;
        }
    }
}
