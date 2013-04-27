using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Collections;
using System.Collections.ObjectModel;
using Samba.Infrastructure.Data;
using Samba.Infrastructure.Data.Serializer;

namespace Samba.Presentation.Controls.Interaction
{
    /// <summary>
    /// Interaction logic for ValueChooserForm.xaml
    /// </summary>
    public partial class ValueChooserForm : Window
    {
        private readonly IList<IOrderable> _selectedValues;
        private readonly IList<IOrderable> _values;

        public ValueChooserForm(IList<IOrderable> values, IList<IOrderable> selectedValues)
        {
            InitializeComponent();

            Width = Properties.Settings.Default.VCWidth;
            Height = Properties.Settings.Default.VCHeight;

            _selectedValues = selectedValues;
            SelectedValues = new ObservableCollection<IOrderable>(_selectedValues);

            _values = values;
            InitValues();

            ValuesListBox.ItemsSource = Values;
            SelectedValuesListBox.ItemsSource = SelectedValues;

            SearchTextBox.Focus();
            SearchTextBox.MinHeight = 20;
        }

        private void InitValues()
        {
            Values = new ObservableCollection<IOrderable>(_values);

            foreach (var item in SelectedValues)
            {
                if (Values.Contains(item))
                    Values.Remove(item);
            }
        }

        public ObservableCollection<IOrderable> Values { get; set; }
        public ObservableCollection<IOrderable> SelectedValues { get; set; }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            SelectedValues = new ObservableCollection<IOrderable>(_selectedValues);
            Close();
        }

        private void MoveValuesToSelectedValues()
        {
            IList selectedItems = new ArrayList(ValuesListBox.SelectedItems);
            foreach (IOrderable item in selectedItems)
            {
                Values.Remove(item);
                SelectedValues.Add(item);
            }
        }

        private void MoveSelectedValuesToValues()
        {
            IList selectedItems = new ArrayList(SelectedValuesListBox.SelectedItems);
            foreach (IOrderable item in selectedItems)
            {
                Values.Add(item);
                SelectedValues.Remove(item);
            }
        }

        private void CopySelectedValuesToValues()
        {
            IList selectedItems = new ArrayList(SelectedValuesListBox.Items);
            foreach (IOrderable item in selectedItems)
            {
                if (!Values.Contains(item))
                    Values.Add(ObjectCloner.Clone(item));
            }
        }

        private void ValuesListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            MoveValuesToSelectedValues();
        }

        private void SelectedValuesListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            MoveSelectedValuesToValues();
        }

        private void SearchTextBox_Search(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(SearchTextBox.Text))
            {
                Values = new ObservableCollection<IOrderable>(_values.Where(x => x.UserString.ToLower().Contains(SearchTextBox.Text.ToLower()) && !SelectedValues.Contains(x)));
            }
            else
            {
                InitValues();
            }
            ValuesListBox.ItemsSource = Values;
            ValuesListBox.SelectedItem = null;
        }

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.IsKeyDown(Key.RightCtrl))
            {
                ValuesListBox.SelectAll();
                MoveValuesToSelectedValues();
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && string.IsNullOrEmpty(SearchTextBox.Text))
            {
                Close();
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            MoveValuesToSelectedValues();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            MoveSelectedValuesToValues();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            CopySelectedValuesToValues();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.VCHeight = Height;
            Properties.Settings.Default.VCWidth = Width;
            Properties.Settings.Default.Save();
        }
    }
}
