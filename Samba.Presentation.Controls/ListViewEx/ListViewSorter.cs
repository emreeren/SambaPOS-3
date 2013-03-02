/*
 *   Demo program for ListView Sorting
 *   Created by Li Gao, 2007
 *   
 *   Modified for demo purposes only.
 *   This program is provided as is and no warranty or support.
 *   Use it as your own risk
 * */

using System;
using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.ComponentModel;

namespace Samba.Presentation.Controls.ListViewEx
{
    static public class ListViewSorter
    {

        public static DependencyProperty CustomSorterProperty = DependencyProperty.RegisterAttached(
         "CustomSorter",
         typeof(IComparer),
         typeof(ListViewSorter));

        public static IComparer GetCustomSorter(DependencyObject obj)
        {
            return (IComparer)obj.GetValue(CustomSorterProperty);
        }

        public static void SetCustomSorter(DependencyObject obj, IComparer value)
        {
            obj.SetValue(CustomSorterProperty, value);
        }

        public static DependencyProperty SortBindingMemberProperty = DependencyProperty.RegisterAttached(
            "SortBindingMember",
            typeof(BindingBase),
            typeof(ListViewSorter));

        public static BindingBase GetSortBindingMember(DependencyObject obj)
        {
            return (BindingBase)obj.GetValue(SortBindingMemberProperty);
        }

        public static void SetSortBindingMember(DependencyObject obj, BindingBase value)
        {
            obj.SetValue(SortBindingMemberProperty, value);
        }
        
        public readonly static DependencyProperty IsListviewSortableProperty = DependencyProperty.RegisterAttached(
            "IsListviewSortable",
            typeof(Boolean),
            typeof(ListViewSorter),
            new FrameworkPropertyMetadata(false, OnRegisterSortableGrid));

        public static Boolean GetIsListviewSortable(DependencyObject obj)
        {
            //return true;
            return (Boolean)obj.GetValue(IsListviewSortableProperty);
        }

        public static void SetIsListviewSortable(DependencyObject obj, Boolean value)
        {            
            obj.SetValue(IsListviewSortableProperty, value);
        }

        private static GridViewColumnHeader _lastHeaderClicked;
        private static ListSortDirection _lastDirection = ListSortDirection.Ascending;
        private static ListView _lv;

        private static void OnRegisterSortableGrid(DependencyObject obj,
          DependencyPropertyChangedEventArgs args)
        {
            var grid = obj as ListView;
            if (grid != null)
            {
                _lv = grid;
                RegisterSortableGridView(grid, args);
            }
        }

        private static void RegisterSortableGridView(ListView grid,
          DependencyPropertyChangedEventArgs args)
        {
        
            if (args.NewValue is Boolean && (Boolean)args.NewValue)
            {
                grid.AddHandler(ButtonBase.ClickEvent,
                    new RoutedEventHandler(GridViewColumnHeaderClickedHandler));
            }
            else
            {
                grid.AddHandler(ButtonBase.ClickEvent,
                 new RoutedEventHandler(GridViewColumnHeaderClickedHandler));
            }
        }

        private static void GridViewColumnHeaderClickedHandler(object sender, RoutedEventArgs e)
        {

            var headerClicked = e.OriginalSource as GridViewColumnHeader;

            if (headerClicked != null)
            {
                ListSortDirection direction;
                if (!Equals(headerClicked, _lastHeaderClicked))
                {

                    direction = ListSortDirection.Ascending;

                }

                else
                {

                    direction = _lastDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;

                }

                string header = String.Empty;

                try
                {                  
                    header = ((Binding)GetSortBindingMember(headerClicked.Column)).Path.Path;
                }
                catch (Exception)
                {
                }

                if (header == String.Empty)
                    return;             

                Sort(header, direction);

                string resourceTypeName = String.Empty;

                //if (_lastHeaderClicked != null)
                //{
                //    ResourceTypeName = "HeaderTemplateSortNon";
                //    tmpTemplate = lv.TryFindResource(ResourceTypeName) as DataTemplate;
                //    _lastHeaderClicked.Column.HeaderTemplate = tmpTemplate;

                //}             

                switch (direction)
                {
                    case ListSortDirection.Ascending: resourceTypeName = "HeaderTemplateSortAsc"; break;
                    case ListSortDirection.Descending: resourceTypeName = "HeaderTemplateSortDesc"; break;
                }
                
                var tmpTemplate = _lv.TryFindResource(resourceTypeName) as DataTemplate;
                if (tmpTemplate != null)
                {
                    headerClicked.Column.HeaderTemplate = tmpTemplate;
                }

                _lastHeaderClicked = headerClicked;
                _lastDirection = direction;

            }            
          
        }
       
        private static void Sort(string sortBy, ListSortDirection direction)
        {     

            var  view = (ListCollectionView ) CollectionViewSource.GetDefaultView(_lv.ItemsSource);

            if (view != null)
            {
                try
                {
                    var sorter = (ListViewCustomComparer)GetCustomSorter(_lv);
                   if (sorter != null)
                   {
                       // measuring timing of custom sort
                       int tick1 = Environment.TickCount;

                       sorter.AddSort(sortBy, direction);
                  
                       view.CustomSort = sorter;
                       _lv.Items.Refresh();

                       int tick2 = Environment.TickCount;

                       double elapsed1 = (tick2 - tick1)/1000.0;

                       MessageBox.Show(elapsed1.ToString(CultureInfo.InvariantCulture) + " seconds.");
                    
                   }
                   else
                   {
                       //measuring timem of SortDescriptions sort
                       int tick3 = Environment.TickCount;

                       _lv.Items.SortDescriptions.Clear();

                       var sd = new SortDescription(sortBy, direction);
                                          
                       _lv.Items.SortDescriptions.Add(sd);
                       _lv.Items.Refresh();

                       int tick4 = Environment.TickCount;

                       double elapsed2 = (tick4 - tick3) / 1000.0;

                       MessageBox.Show(elapsed2.ToString(CultureInfo.InvariantCulture) + " seconds.");

                   }
                }
                catch (Exception)
                {
                }

            }

        }
    }


}
