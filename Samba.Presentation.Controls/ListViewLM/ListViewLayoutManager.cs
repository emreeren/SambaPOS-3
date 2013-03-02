// -- FILE ------------------------------------------------------------------
// name       : ListViewLayoutManager.cs
// created    : Jani Giannoudis - 2008.03.27
// language   : c#
// environment: .NET 3.0
// --------------------------------------------------------------------------

using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.ComponentModel;

namespace Samba.Presentation.Controls.ListViewLM
{

    // ------------------------------------------------------------------------
    public class ListViewLayoutManager
    {

        // ----------------------------------------------------------------------
        public static readonly DependencyProperty EnabledProperty = DependencyProperty.RegisterAttached(
            "Enabled",
            typeof(bool),
            typeof(ListViewLayoutManager),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnLayoutManagerEnabledChanged)));

        // ----------------------------------------------------------------------
        public ListViewLayoutManager(ListView listView)
        {
            if (listView == null)
            {
                throw new ArgumentNullException("listView");
            }

            this.listView = listView;
            this.listView.Loaded += new RoutedEventHandler(ListViewLoaded);
            this.listView.Unloaded += new RoutedEventHandler(ListViewUnloaded);
        }

        // ListViewLayoutManager

        // ----------------------------------------------------------------------
        public ListView ListView
        {
            get { return this.listView; }
        } // ListView

        // ----------------------------------------------------------------------
        public ScrollBarVisibility VerticalScrollBarVisibility
        {
            get { return this.verticalScrollBarVisibility; }
            set { this.verticalScrollBarVisibility = value; }
        } // VerticalScrollBarVisibility

        // ----------------------------------------------------------------------
        public static void SetEnabled(DependencyObject dependencyObject, bool enabled)
        {
            dependencyObject.SetValue(EnabledProperty, enabled);
        } // SetEnabled

        // ----------------------------------------------------------------------
        private void RegisterEvents(DependencyObject start)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(start); i++)
            {
                Visual childVisual = VisualTreeHelper.GetChild(start, i) as Visual;
                if (childVisual is Thumb)
                {
                    GridViewColumn gridViewColumn = FindParentColumn(childVisual);
                    if (gridViewColumn == null)
                    {
                        continue;
                    }

                    Thumb thumb = childVisual as Thumb;
                    thumb.PreviewMouseMove += new MouseEventHandler(ThumbPreviewMouseMove);
                    thumb.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(ThumbPreviewMouseLeftButtonDown);
                    DependencyPropertyDescriptor.FromProperty(
                        GridViewColumn.WidthProperty,
                        typeof(GridViewColumn)).AddValueChanged(gridViewColumn, GridColumnWidthChanged);
                }
                else if (childVisual is GridViewColumnHeader)
                {
                    GridViewColumnHeader columnHeader = childVisual as GridViewColumnHeader;
                    columnHeader.SizeChanged += new SizeChangedEventHandler(GridColumnHeaderSizeChanged);
                }
                else if (this.scrollViewer == null && childVisual is ScrollViewer)
                {
                    this.scrollViewer = childVisual as ScrollViewer;
                    this.scrollViewer.ScrollChanged += new ScrollChangedEventHandler(ScrollViewerScrollChanged);
                    // assume we do the regulation of the horizontal scrollbar
                    this.scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
                    this.scrollViewer.VerticalScrollBarVisibility = this.verticalScrollBarVisibility;
                }

                RegisterEvents(childVisual);  // recursive
            }
        } // RegisterEvents

        // ----------------------------------------------------------------------
        private void UnregisterEvents(DependencyObject start)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(start); i++)
            {
                Visual childVisual = VisualTreeHelper.GetChild(start, i) as Visual;
                if (childVisual is Thumb)
                {
                    GridViewColumn gridViewColumn = FindParentColumn(childVisual);
                    if (gridViewColumn == null)
                    {
                        continue;
                    }

                    Thumb thumb = childVisual as Thumb;
                    thumb.PreviewMouseMove -= new MouseEventHandler(ThumbPreviewMouseMove);
                    thumb.PreviewMouseLeftButtonDown -= new MouseButtonEventHandler(ThumbPreviewMouseLeftButtonDown);
                    DependencyPropertyDescriptor.FromProperty(
                        GridViewColumn.WidthProperty,
                        typeof(GridViewColumn)).RemoveValueChanged(gridViewColumn, GridColumnWidthChanged);
                }
                else if (childVisual is GridViewColumnHeader)
                {
                    GridViewColumnHeader columnHeader = childVisual as GridViewColumnHeader;
                    columnHeader.SizeChanged -= new SizeChangedEventHandler(GridColumnHeaderSizeChanged);
                }
                else if (this.scrollViewer == null && childVisual is ScrollViewer)
                {
                    this.scrollViewer = childVisual as ScrollViewer;
                    this.scrollViewer.ScrollChanged -= new ScrollChangedEventHandler(ScrollViewerScrollChanged);
                }

                UnregisterEvents(childVisual);  // recursive
            }
        } // UnregisterEvents

        // ----------------------------------------------------------------------
        private GridViewColumn FindParentColumn(DependencyObject element)
        {
            if (element == null)
            {
                return null;
            }

            while (element != null)
            {
                if (element is GridViewColumnHeader)
                {
                    return ((GridViewColumnHeader)element).Column;
                }
                element = VisualTreeHelper.GetParent(element);
            }

            return null;
        } // FindParentColumn

        // ----------------------------------------------------------------------
        private GridViewColumnHeader FindColumnHeader(DependencyObject start, GridViewColumn gridViewColumn)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(start); i++)
            {
                Visual childVisual = VisualTreeHelper.GetChild(start, i) as Visual;
                if (childVisual is GridViewColumnHeader)
                {
                    GridViewColumnHeader gridViewHeader = childVisual as GridViewColumnHeader;
                    if (gridViewHeader != null && gridViewHeader.Column == gridViewColumn)
                    {
                        return gridViewHeader;
                    }
                }
                GridViewColumnHeader childGridViewHeader = FindColumnHeader(childVisual, gridViewColumn);  // recursive
                if (childGridViewHeader != null)
                {
                    return childGridViewHeader;
                }
            }
            return null;
        } // FindColumnHeader

        // ----------------------------------------------------------------------
        private void InitColumns()
        {
            GridView view = this.listView.View as GridView;
            if (view == null)
            {
                return;
            }

            foreach (GridViewColumn gridViewColumn in view.Columns)
            {
                if (!RangeColumn.IsRangeColumn(gridViewColumn))
                {
                    continue;
                }

                double? minWidth = RangeColumn.GetRangeMinWidth(gridViewColumn);
                double? maxWidth = RangeColumn.GetRangeMaxWidth(gridViewColumn);
                if (!minWidth.HasValue && !maxWidth.HasValue)
                {
                    continue;
                }

                GridViewColumnHeader columnHeader = FindColumnHeader(this.listView, gridViewColumn);
                if (columnHeader == null)
                {
                    continue;
                }

                double actualWidth = columnHeader.ActualWidth;
                if (minWidth.HasValue)
                {
                    columnHeader.MinWidth = minWidth.Value;
                    if (!double.IsInfinity(actualWidth) && actualWidth < columnHeader.MinWidth)
                    {
                        gridViewColumn.Width = columnHeader.MinWidth;
                    }
                }
                if (maxWidth.HasValue)
                {
                    columnHeader.MaxWidth = maxWidth.Value;
                    if (!double.IsInfinity(actualWidth) && actualWidth > columnHeader.MaxWidth)
                    {
                        gridViewColumn.Width = columnHeader.MaxWidth;
                    }
                }
            }
        } // InitColumns

        // ----------------------------------------------------------------------
        protected virtual void ResizeColumns()
        {
            GridView view = this.listView.View as GridView;
            if (view == null || view.Columns.Count == 0)
            {
                return;
            }

            // listview width
            double actualWidth = double.PositiveInfinity;
            if (this.scrollViewer != null)
            {
                actualWidth = this.scrollViewer.ViewportWidth;
            }
            if (double.IsInfinity(actualWidth))
            {
                actualWidth = this.listView.ActualWidth;
            }
            if (double.IsInfinity(actualWidth) || actualWidth <= 0)
            {
                return;
            }

            double resizeableRegionCount = 0;
            double otherColumnsWidth = 0;
            // determine column sizes
            foreach (GridViewColumn gridViewColumn in view.Columns)
            {
                if (ProportionalColumn.IsProportionalColumn(gridViewColumn))
                {
                    resizeableRegionCount += ProportionalColumn.GetProportionalWidth(gridViewColumn).Value;
                }
                else
                {
                    otherColumnsWidth += gridViewColumn.ActualWidth;
                }
            }

            if (resizeableRegionCount <= 0)
            {
                // no proportional columns present: commit the regulation to the scroll viewer
                this.scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;

                // search the first fill column
                GridViewColumn fillColumn = null;
                for (int i = 0; i < view.Columns.Count; i++)
                {
                    GridViewColumn gridViewColumn = view.Columns[i];
                    if (IsFillColumn(gridViewColumn))
                    {
                        fillColumn = gridViewColumn;
                        break;
                    }
                }

                if (fillColumn != null)
                {
                    double otherColumnsWithoutFillWidth = otherColumnsWidth - fillColumn.ActualWidth;
                    double fillWidth = actualWidth - otherColumnsWithoutFillWidth;
                    if (fillWidth > 0)
                    {
                        double? minWidth = RangeColumn.GetRangeMinWidth(fillColumn);
                        double? maxWidth = RangeColumn.GetRangeMaxWidth(fillColumn);

                        bool setWidth = true;
                        if (minWidth.HasValue && fillWidth < minWidth.Value)
                        {
                            setWidth = false;
                        }
                        if (maxWidth.HasValue && fillWidth > maxWidth.Value)
                        {
                            setWidth = false;
                        }
                        if (setWidth)
                        {
                            this.scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
                            fillColumn.Width = fillWidth;
                        }
                    }
                }
                return;
            }

            double resizeableColumnsWidth = actualWidth - otherColumnsWidth;
            if (resizeableColumnsWidth <= 0)
            {
                return; // missing space
            }

            // resize columns
            double resizeableRegionWidth = resizeableColumnsWidth / resizeableRegionCount;
            foreach (GridViewColumn gridViewColumn in view.Columns)
            {
                if (ProportionalColumn.IsProportionalColumn(gridViewColumn))
                {
                    gridViewColumn.Width = ProportionalColumn.GetProportionalWidth(gridViewColumn).Value * resizeableRegionWidth;
                }
            }
        } // ResizeColumns

        // ----------------------------------------------------------------------
        // returns the delta
        private double SetRangeColumnToBounds(GridViewColumn gridViewColumn)
        {
            double startWidth = gridViewColumn.Width;

            double? minWidth = RangeColumn.GetRangeMinWidth(gridViewColumn);
            double? maxWidth = RangeColumn.GetRangeMaxWidth(gridViewColumn);

            if ((minWidth.HasValue && maxWidth.HasValue) && (minWidth > maxWidth))
            {
                return 0; // invalid case
            }

            if (minWidth.HasValue && gridViewColumn.Width < minWidth.Value)
            {
                gridViewColumn.Width = minWidth.Value;
            }
            else if (maxWidth.HasValue && gridViewColumn.Width > maxWidth.Value)
            {
                gridViewColumn.Width = maxWidth.Value;
            }

            return gridViewColumn.Width - startWidth;
        } // SetRangeColumnToBounds

        // ----------------------------------------------------------------------
        private bool IsFillColumn(GridViewColumn gridViewColumn)
        {
            if (gridViewColumn == null)
            {
                return false;
            }

            GridView view = this.listView.View as GridView;
            if (view == null || view.Columns.Count == 0)
            {
                return false;
            }

            bool? isFillCoumn = RangeColumn.GetRangeIsFillColumn(gridViewColumn);
            return isFillCoumn.HasValue && isFillCoumn.Value == true;
        } // IsFillColumn

        // ----------------------------------------------------------------------
        private void DoResizeColumns()
        {
            if (this.resizing)
            {
                return;
            }

            this.resizing = true;
            try
            {
                ResizeColumns();
            }
            catch
            {
                throw;
            }
            finally
            {
                this.resizing = false;
            }
        } // DoResizeColumns

        // ----------------------------------------------------------------------
        private void ListViewLoaded(object sender, RoutedEventArgs e)
        {
            RegisterEvents(this.listView);
            InitColumns();
            DoResizeColumns();
            this.loaded = true;
        } // ListViewLoaded

        // ----------------------------------------------------------------------
        private void ListViewUnloaded(object sender, RoutedEventArgs e)
        {
            if (!this.loaded)
            {
                return;
            }
            UnregisterEvents(this.listView);
            this.loaded = false;
        } // ListViewUnloaded

        // ----------------------------------------------------------------------
        private void ThumbPreviewMouseMove(object sender, MouseEventArgs e)
        {
            Thumb thumb = sender as Thumb;
            GridViewColumn gridViewColumn = FindParentColumn(thumb);
            if (gridViewColumn == null)
            {
                return;
            }

            // suppress column resizing for proportional, fixed and range fill columns
            if (ProportionalColumn.IsProportionalColumn(gridViewColumn) ||
                FixedColumn.IsFixedColumn(gridViewColumn) ||
                IsFillColumn(gridViewColumn))
            {
                thumb.Cursor = null;
                return;
            }

            // check range column bounds
            if (thumb.IsMouseCaptured && RangeColumn.IsRangeColumn(gridViewColumn))
            {
                double? minWidth = RangeColumn.GetRangeMinWidth(gridViewColumn);
                double? maxWidth = RangeColumn.GetRangeMaxWidth(gridViewColumn);

                if ((minWidth.HasValue && maxWidth.HasValue) && (minWidth > maxWidth))
                {
                    return; // invalid case
                }

                if (this.resizeCursor == null)
                {
                    this.resizeCursor = thumb.Cursor; // save the resize cursor
                }

                if (minWidth.HasValue && gridViewColumn.Width <= minWidth.Value)
                {
                    thumb.Cursor = Cursors.No;
                }
                else if (maxWidth.HasValue && gridViewColumn.Width >= maxWidth.Value)
                {
                    thumb.Cursor = Cursors.No;
                }
                else
                {
                    thumb.Cursor = this.resizeCursor; // between valid min/max
                }
            }
        } // ThumbPreviewMouseMove

        // ----------------------------------------------------------------------
        private void ThumbPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Thumb thumb = sender as Thumb;
            GridViewColumn gridViewColumn = FindParentColumn(thumb);

            // suppress column resizing for proportional, fixed and range fill columns
            if (ProportionalColumn.IsProportionalColumn(gridViewColumn) ||
                FixedColumn.IsFixedColumn(gridViewColumn) ||
                IsFillColumn(gridViewColumn))
            {
                e.Handled = true;
                return;
            }
        } // ThumbPreviewMouseLeftButtonDown

        // ----------------------------------------------------------------------
        private void GridColumnWidthChanged(object sender, EventArgs e)
        {
            if (!this.loaded)
            {
                return;
            }

            GridViewColumn gridViewColumn = sender as GridViewColumn;

            // suppress column resizing for proportional and fixed columns
            if (ProportionalColumn.IsProportionalColumn(gridViewColumn) || FixedColumn.IsFixedColumn(gridViewColumn))
            {
                return;
            }

            // ensure range column within the bounds
            if (RangeColumn.IsRangeColumn(gridViewColumn))
            {
                // special case: auto column width - maybe conflicts with min/max range
                if (gridViewColumn.Width.Equals(double.NaN))
                {
                    this.autoSizedColumn = gridViewColumn;
                    return; // handled by the change header size event
                }

                // ensure column bounds
                if (SetRangeColumnToBounds(gridViewColumn) != 0)
                {
                    return;
                }
            }

            DoResizeColumns();
        } // GridColumnWidthChanged

        // ----------------------------------------------------------------------
        // handle autosized column
        private void GridColumnHeaderSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.autoSizedColumn == null)
            {
                return;
            }

            GridViewColumnHeader gridViewColumnHeader = sender as GridViewColumnHeader;
            if (gridViewColumnHeader.Column == this.autoSizedColumn)
            {
                if (gridViewColumnHeader.Width.Equals(double.NaN))
                {
                    // sync column with 
                    gridViewColumnHeader.Column.Width = gridViewColumnHeader.ActualWidth;
                    DoResizeColumns();
                }

                this.autoSizedColumn = null;
            }
        } // GridColumnHeaderSizeChanged

        // ----------------------------------------------------------------------
        private void ScrollViewerScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (this.loaded && e.ViewportWidthChange != 0)
            {
                DoResizeColumns();
            }
        } // ScrollViewerScrollChanged

        // ----------------------------------------------------------------------
        private static void OnLayoutManagerEnabledChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            ListView listView = dependencyObject as ListView;
            if (listView != null)
            {
                bool enabled = (bool)e.NewValue;
                if (enabled)
                {
                    new ListViewLayoutManager(listView);
                }
            }
        } // OnLayoutManagerEnabledChanged

        // ----------------------------------------------------------------------
        // members
        private readonly ListView listView;
        private ScrollViewer scrollViewer;
        private bool loaded = false;
        private bool resizing = false;
        private Cursor resizeCursor;
        private ScrollBarVisibility verticalScrollBarVisibility = ScrollBarVisibility.Auto;
        private GridViewColumn autoSizedColumn;

    } // class ListViewLayoutManager

} // namespace Itenso.Windows.Controls.ListViewLayout
// -- EOF -------------------------------------------------------------------
