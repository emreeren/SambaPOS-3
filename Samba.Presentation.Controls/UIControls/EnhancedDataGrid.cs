// Got from http://bengribaudo.com/blog/2012/03/14/1942/saving-restoring-wpf-datagrid-columns-size-sorting-and-order

using System;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Samba.Infrastructure.Data.BinarySerializer;
using Samba.Infrastructure.Settings;

namespace Samba.Presentation.Controls.UIControls
{
    public class EnhancedDataGrid : DataGrid
    {
        private bool _inWidthChange;
        private bool _updatingColumnInfo;

        public static readonly DependencyProperty ColumnInfoProperty = DependencyProperty.Register("ColumnInfo",
                typeof(ObservableCollection<ColumnInfo>), typeof(EnhancedDataGrid),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, ColumnInfoChangedCallback)
            );

        private static void ColumnInfoChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var grid = (EnhancedDataGrid)dependencyObject;
            if (!grid._updatingColumnInfo) { grid.ColumnInfoChanged(); }
        }

        protected override void OnInitialized(EventArgs e)
        {
            EventHandler sortDirectionChangedHandler = (sender, x) => UpdateColumnInfo();
            EventHandler widthPropertyChangedHandler = (sender, x) => _inWidthChange = true;
            var sortDirectionPropertyDescriptor = DependencyPropertyDescriptor.FromProperty(DataGridColumn.SortDirectionProperty, typeof(DataGridColumn));
            var widthPropertyDescriptor = DependencyPropertyDescriptor.FromProperty(DataGridColumn.WidthProperty, typeof(DataGridColumn));

            Loaded += (sender, x) =>
            {
                foreach (var column in Columns)
                {
                    sortDirectionPropertyDescriptor.AddValueChanged(column, sortDirectionChangedHandler);
                    widthPropertyDescriptor.AddValueChanged(column, widthPropertyChangedHandler);
                }
                Load();
            };

            Unloaded += (sender, x) =>
            {
                Save();
                foreach (var column in Columns)
                {
                    sortDirectionPropertyDescriptor.RemoveValueChanged(column, sortDirectionChangedHandler);
                    widthPropertyDescriptor.RemoveValueChanged(column, widthPropertyChangedHandler);
                }
            };

            base.OnInitialized(e);
        }

        public ObservableCollection<ColumnInfo> ColumnInfo
        {
            get { return (ObservableCollection<ColumnInfo>)GetValue(ColumnInfoProperty); }
            set { SetValue(ColumnInfoProperty, value); }
        }

        private void UpdateColumnInfo()
        {
            _updatingColumnInfo = true;
            ColumnInfo = new ObservableCollection<ColumnInfo>(Columns.Select(x => new ColumnInfo(x)));
            _updatingColumnInfo = false;
        }

        protected override void OnColumnReordered(DataGridColumnEventArgs e)
        {
            UpdateColumnInfo();
            base.OnColumnReordered(e);
        }

        protected override void OnPreviewMouseLeftButtonUp(System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_inWidthChange)
            {
                _inWidthChange = false;
                UpdateColumnInfo();
            }
            base.OnPreviewMouseLeftButtonUp(e);
        }

        private void ColumnInfoChanged()
        {
            Items.SortDescriptions.Clear();
            foreach (var column in ColumnInfo)
            {
                var realColumn = Columns.FirstOrDefault(x => column.Header != null && column.Header.Equals(x.Header));
                if (realColumn == null) { continue; }
                column.Apply(realColumn, Columns.Count, Items.SortDescriptions);
            }
        }

        private void Save()
        {
            if (string.IsNullOrEmpty(Name)) return;
            var data = SilverlightSerializer.Serialize(ColumnInfo);
            File.WriteAllBytes(LocalSettings.DocumentPath + "\\" + Name + ".dat", data);
        }

        private void Load()
        {
            if (string.IsNullOrEmpty(Name)) return;
            var fn = LocalSettings.DocumentPath + "\\" + Name + ".dat";
            if (File.Exists(fn))
            {
                var data = File.ReadAllBytes(fn);
                var ci = SilverlightSerializer.Deserialize(data);
                ColumnInfo = (ObservableCollection<ColumnInfo>)ci;
            }
        }
    }

    public class ColumnInfo
    {
        public ColumnInfo()
        {
            //Default
        }

        public ColumnInfo(DataGridColumn column)
        {
            Header = column.Header;
            WidthValue = column.Width.DisplayValue;
            WidthType = column.Width.UnitType;
            DisplayIndex = column.DisplayIndex;
        }

        public void Apply(DataGridColumn column, int gridColumnCount, SortDescriptionCollection sortDescriptions)
        {
            column.Width = new DataGridLength(WidthValue, WidthType);

            if (column.DisplayIndex != DisplayIndex)
            {
                var maxIndex = (gridColumnCount == 0) ? 0 : gridColumnCount - 1;
                column.DisplayIndex = (DisplayIndex <= maxIndex) ? DisplayIndex : maxIndex;
            }
        }

        public object Header { get; set; }
        public int DisplayIndex { get; set; }
        public double WidthValue { get; set; }
        public DataGridLengthUnitType WidthType { get; set; }
    }
}

