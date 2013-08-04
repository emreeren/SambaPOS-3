using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Samba.Infrastructure.Settings;

namespace Samba.Presentation.Common
{
    public static class ExtensionServices
    {
        public static void BackgroundFocus(this UIElement el)
        {
            Action a = () => Keyboard.Focus(el);
            el.Dispatcher.BeginInvoke(DispatcherPriority.Input, a);
        }

        public static void BackgroundSelectAll(this TextBox textBox)
        {
            Action a = textBox.SelectAll;
            textBox.Dispatcher.BeginInvoke(DispatcherPriority.Input, a);
        }

        public static void AddRange<T>(this ObservableCollection<T> oc, IEnumerable<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            foreach (var item in collection)
            {
                oc.Add(item);
            }
        }

        private static readonly Action EmptyDelegate = delegate { };

        public static void Refresh(this UIElement uiElement)
        {
            uiElement.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
        }

        public static TContainer GetContainerFromIndex<TContainer>(this ItemsControl itemsControl, int index) where TContainer : DependencyObject
        {
            return (TContainer)
              itemsControl.ItemContainerGenerator.ContainerFromIndex(index);
        }

        public static bool IsEditing(this DataGrid dataGrid)
        {
            return dataGrid.GetEditingRow() != null;
        }

        public static DataGridRow GetEditingRow(this DataGrid dataGrid)
        {
            var sIndex = dataGrid.SelectedIndex;
            if (sIndex >= 0)
            {
                var selected = dataGrid.GetContainerFromIndex<DataGridRow>(sIndex);
                if (selected != null && selected.IsEditing) return selected;
            }

            for (int i = 0; i < dataGrid.Items.Count; i++)
            {
                if (i == sIndex) continue;
                var item = dataGrid.GetContainerFromIndex<DataGridRow>(i);
                if (item != null && item.IsEditing) return item;
            }

            return null;
        }

        public static DataGridCell GetCell(this DataGrid dataGrid, int row, int column)
        {
            DataGridRow rowContainer = GetRow(dataGrid, row);

            if (rowContainer != null)
            {
                var presenter = GetVisualChild<DataGridCellsPresenter>(rowContainer);
                if (presenter == null)
                {
                    dataGrid.ScrollIntoView(rowContainer, dataGrid.Columns[column]);
                    presenter = GetVisualChild<DataGridCellsPresenter>(rowContainer);
                }
                var cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);
                if (cell == null)
                {
                    dataGrid.ScrollIntoView(rowContainer, dataGrid.Columns[column]);
                    cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);
                }
                return cell;
            }
            return null;
        }

        public static DataGridRow GetRow(this DataGrid dataGrid, int index)
        {
            var row = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromIndex(index);
            if (row == null)
            {
                dataGrid.ScrollIntoView(dataGrid.Items[index]);
                row = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromIndex(index);
            }
            return row;
        }

        public static T GetVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            var child = default(T);
            if (parent == null) return null;
            var numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (var i = 0; i < numVisuals; i++)
            {
                var v = VisualTreeHelper.GetChild(parent, i);
                child = v as T ?? GetVisualChild<T>(v);
                if (child != null)
                {
                    break;
                }
            }
            return child;
        }

        public static string AsCsv<T>(this IEnumerable<T> items) where T : class
        {
            var csvBuilder = new StringBuilder();
            var properties = typeof(T).GetProperties();
            csvBuilder.AppendLine(string.Join(",", (from a in properties select a.Name).ToArray()));
            foreach (T item in items)
            {
                var line = string.Join(",",
                    properties.Select(p => p.GetValue(item, null).ToCsvValue()).ToArray());
                csvBuilder.AppendLine(line);
            }
            return csvBuilder.ToString();
        }

        private static string ToCsvValue<T>(this T item) where T : class
        {
            if (item is string)
            {
                return string.Format("\"{0}\"", item.ToString().Replace("\"", "\\\"")); ;
            }
            return string.Format("\"{0}\"", item);
        }

        public static ParallelQuery<TSource> SetCulture<TSource>(this ParallelQuery<TSource> source)
        {
            SetCulture();
            return source
                .Select(
                    item =>
                    {
                        SetCulture();
                        return item;
                    });
        }

        private static void SetCulture()
        {
            LocalSettings.UpdateThreadLanguage();
        }
    }
}
