using System;
using System.Windows;

namespace Samba.Presentation.Controls.Interaction
{
    /// <summary>
    /// Interaction logic for StringGetterForm.xaml
    /// </summary>
    public partial class StringGetterForm : Window
    {
        public StringGetterForm()
        {
            InitializeComponent();
            Width = Properties.Settings.Default.SGWidth;
            Height = Properties.Settings.Default.SGHeight;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            TextBox.Text = "";
            Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            TextBox.Focus();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.SGHeight = Height;
            Properties.Settings.Default.SGWidth = Width;
        }
    }
}
