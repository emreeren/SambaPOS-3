using System.Windows.Forms;
using UserControl = System.Windows.Controls.UserControl;

namespace Samba.Presentation.Controls.VirtualKeyboard
{
    /// <summary>
    /// Interaction logic for KeyboardView.xaml
    /// </summary>
    public partial class KeyboardView : UserControl
    {
        public KeyboardView()
        {
            InitializeComponent();
            DataContext = new KeyboardViewModel();
        }

        private void UserControl_IsVisibleChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (DataContext == null || ((KeyboardViewModel)DataContext).Model == null) return;

            ((KeyboardViewModel)DataContext).Model.ReleaseKey(Keys.ShiftKey);
            ((KeyboardViewModel)DataContext).Model.ReleaseKey(Keys.LShiftKey);
            ((KeyboardViewModel)DataContext).Model.ReleaseKey(Keys.RShiftKey);
        }

        public void SendKey(Keys key)
        {
            ((KeyboardViewModel)DataContext).Model.SendKey(key);
        }
    }
}
