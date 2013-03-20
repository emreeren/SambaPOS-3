using System;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using PropertyTools.Wpf;
using Samba.Presentation.Common.Services;

namespace Samba.Presentation.Controls.Interaction
{
    /// <summary>
    /// Interaction logic for GridEditorForm.xaml
    /// </summary>
    public partial class GridEditorForm : Window
    {
        public GridEditorForm()
        {
            InitializeComponent();
            MainGrid.ControlFactory = new CustomItemsGridControlFactory();
        }

        public void SetList(IList items)
        {
            if (items.Count > 0)
            {
                var itemType = items[0].GetType();
                foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(itemType))
                {
                    if (!descriptor.IsBrowsable)
                    {
                        continue;
                    }

                    var cd = new ColumnDefinition { PropertyName = descriptor.Name };

                    if (descriptor.PropertyType == typeof(Color) || descriptor.PropertyType == typeof(bool))
                    {
                        cd.HorizontalAlignment = HorizontalAlignment.Center;
                    }

                    var displayName = descriptor.DisplayName;

                    if (!String.IsNullOrEmpty(displayName))
                        cd.Header = descriptor.DisplayName;

                    MainGrid.ColumnDefinitions.Add(cd);
                }
                MainGrid.ItemsSource = items;
            }
        }

        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            foreach (var selectedItem in MainGrid.SelectedItems)
            {
                InteractionService.UserIntraction.EditProperties(selectedItem);
                e.Handled = true;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
