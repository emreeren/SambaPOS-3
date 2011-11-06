using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using PropertyTools.Wpf;
using Samba.Presentation.Common.Services;
using ColumnDefinition = PropertyTools.Wpf.ColumnDefinition;

namespace Samba.Presentation.Common.Interaction
{
    /// <summary>
    /// Interaction logic for GridEditorForm.xaml
    /// </summary>
    public partial class GridEditorForm : Window
    {
        public GridEditorForm()
        {
            InitializeComponent();
        }

        public void SetList(IList items)
        {
            if (items.Count > 0)
            {
                MainGrid.ColumnHeaders = new StringCollection();
                var itemType = items[0].GetType();
                foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(itemType))
                {
                    if (!descriptor.IsBrowsable)
                    {
                        continue;
                    }

                    var cd = new ColumnDefinition() { DataField = descriptor.Name };

                    if (descriptor.PropertyType == typeof(SolidColorBrush))
                    {
                        var colorDisplayTemplate = new DataTemplate { DataType = typeof(SolidColorBrush) };
                        var fef = new FrameworkElementFactory(typeof(Rectangle));
                        fef.SetBinding(Shape.FillProperty, new Binding(descriptor.Name));
                        fef.SetValue(WidthProperty, 12.0);
                        fef.SetValue(HeightProperty, 12.0);
                        fef.SetValue(MarginProperty, new Thickness(4, 0, 4, 0));
                        fef.SetValue(Shape.StrokeThicknessProperty, 1.0);
                        fef.SetValue(Shape.StrokeProperty, Brushes.Gainsboro);
                        colorDisplayTemplate.VisualTree = fef;
                        cd.DisplayTemplate = colorDisplayTemplate;

                        var colorEditTemplate = new DataTemplate { DataType = typeof(SolidColorBrush) };
                        var fefe = new FrameworkElementFactory(typeof(ColorPicker));
                        fefe.SetBinding(ColorPicker.SelectedColorProperty, new Binding(descriptor.Name) { Converter = new BrushToColorConverter() });
                        colorEditTemplate.VisualTree = fefe;
                        cd.EditTemplate = colorEditTemplate;
                    }

                    if (descriptor.PropertyType == typeof(string) && descriptor.Name.Contains("Image"))
                    {
                        var colorEditTemplate = new DataTemplate { DataType = typeof(string) };
                        var fefe = new FrameworkElementFactory(typeof(FilePicker));
                        fefe.SetBinding(FilePicker.FilePathProperty, new Binding(descriptor.Name));
                        colorEditTemplate.VisualTree = fefe;
                        cd.EditTemplate = colorEditTemplate;
                    }

                    var displayName = descriptor.DisplayName;
                    if (!String.IsNullOrEmpty(displayName))
                        cd.Header = descriptor.DisplayName;


                    MainGrid.ColumnDefinitions.Add(cd);
                }
                MainGrid.Content = items;
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
