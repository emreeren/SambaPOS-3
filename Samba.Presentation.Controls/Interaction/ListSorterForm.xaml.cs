using System.Windows;

namespace Samba.Presentation.Controls.Interaction
{
    /// <summary>
    /// Interaction logic for ListSorterForm.xaml
    /// </summary>
    public partial class ListSorterForm : Window
    {
        public ListSorterForm()
        {
            InitializeComponent();
            Height = Properties.Settings.Default.LSHeight;
            Width = Properties.Settings.Default.LSWidth;
        }

        private void ButtonClick1(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.LSHeight = Height;
            Properties.Settings.Default.LSWidth = Width;
            Properties.Settings.Default.Save();
        }
    }
}
