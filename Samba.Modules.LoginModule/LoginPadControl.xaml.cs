using System.Windows;
using System.Windows.Controls;
using Samba.Presentation.Common;
using Samba.Presentation.Services.Common;

namespace Samba.Modules.LoginModule
{
    public delegate void PinSubmittedEventHandler(object sender, string pinValue);

    /// <summary>
    /// Interaction logic for LoginPadControl.xaml
    /// </summary>
    public partial class LoginPadControl : UserControl
    {
        public event PinSubmittedEventHandler PinSubmitted;
        private string _pinValue = string.Empty;

        public LoginPadControl()
        {
            InitializeComponent();
            PinValue = EmptyString;
        }

        private string PinValue { get { return _pinValue; } set { _pinValue = value; UpdatePinTextBox(_pinValue); } }
        private static string EmptyString { get { return " " + Localization.Properties.Resources.EnterPin; } }
        private void UpdatePinTextBox(string _pinValue)
        {
            if (_pinValue == EmptyString)
                PinTextBox.Text = _pinValue;
            else
                PinTextBox.Text = "".PadLeft(_pinValue.Length, '*');
        }

        private bool CheckPinValue()
        {
            if (_pinValue == EmptyString)
                PinValue = "";
            return _pinValue.Length < 19;
        }

        public void UpdatePinValue(string value)
        {
            if (CheckPinValue())
            {
                PinValue += value;
            }
        }

        public void SubmitPin()
        {
            if (PinSubmitted != null && AppServices.CanStartApplication())
                PinSubmitted(this, _pinValue);
            else
            {
                if (!AppServices.CanStartApplication())
                    MessageBox.Show(Localization.Properties.Resources.CheckDBVersion);
            }
            PinValue = EmptyString;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SubmitPin();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            UpdatePinValue("1");
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            UpdatePinValue("2");
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            UpdatePinValue("3");
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            UpdatePinValue("4");
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            UpdatePinValue("5");
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            UpdatePinValue("6");
        }

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            UpdatePinValue("7");
        }

        private void Button_Click_8(object sender, RoutedEventArgs e)
        {
            UpdatePinValue("8");
        }

        private void Button_Click_9(object sender, RoutedEventArgs e)
        {
            UpdatePinValue("9");
        }

        private void Button_Click_10(object sender, RoutedEventArgs e)
        {
            UpdatePinValue("0");
        }

        private void Button_Click_11(object sender, RoutedEventArgs e)
        {
            PinValue = "";
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            PinTextBox.BackgroundFocus();
        }
    }
}
