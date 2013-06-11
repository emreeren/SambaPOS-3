using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Widgets;

namespace Samba.Presentation.Controls.UIControls
{
    public class DiagramCanvas : InkCanvas
    {
        public ObservableCollection<IDiagram> Source
        {
            get { return (ObservableCollection<IDiagram>)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source",
            typeof(ObservableCollection<IDiagram>),
            typeof(DiagramCanvas),
            new FrameworkPropertyMetadata(new ObservableCollection<IDiagram>(),
            new PropertyChangedCallback(SourceChanged)));

        protected static void SourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var instance = sender as DiagramCanvas;
            Debug.Assert(instance != null);

            var newCollection = args.NewValue as ObservableCollection<IDiagram>;
            var oldCollection = args.OldValue as ObservableCollection<IDiagram>;

            if (oldCollection != null)
            {
                oldCollection.CollectionChanged -= instance.collection_CollectionChanged;
            }

            instance.Children.Clear();

            if (newCollection != null)
            {
                foreach (var item in newCollection)
                {
                    AddControl(item, instance);
                }
                newCollection.CollectionChanged += instance.collection_CollectionChanged;
            }
        }

        protected void collection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems == null) return;
            foreach (IDiagram item in e.NewItems)
            {
                AddControl(item);
            }
        }

        public static ContextMenu ButtonContextMenu { get; set; }
        public event EventHandler WidgetRemoved;

        public void OnWidgetRemoved(EventArgs e, IDiagram widgetViewModel)
        {
            var handler = WidgetRemoved;
            if (handler != null) handler(widgetViewModel, e);
        }

        protected static void AddControl(IDiagram buttonHolder, InkCanvas parentControl)
        {
            var result = WidgetCreatorRegistry.CreateWidgetControl(buttonHolder, ButtonContextMenu);
            if (result != null) parentControl.Children.Add(result);
            buttonHolder.Refresh();
        }

        protected void AddControl(IDiagram buttonHolder)
        {
            AddControl(buttonHolder, this);
        }

        static DiagramCanvas()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DiagramCanvas), new FrameworkPropertyMetadata(typeof(DiagramCanvas)));
            ButtonContextMenu = new ContextMenu();
        }

        public DiagramCanvas()
        {
            ButtonContextMenu.Items.Clear();
            var menuItem = new MenuItem { Header = Localization.Properties.Resources.Properties };
            menuItem.Click += MenuItemClick;
            ButtonContextMenu.Items.Add(menuItem);
            var mi2 = new MenuItem { Header = Localization.Properties.Resources.Settings };
            mi2.Click += Mi2Click;
            ButtonContextMenu.Items.Add(mi2);
            var mi3 = new MenuItem { Header = Localization.Properties.Resources.Delete };
            mi3.Click += Mi3Click;
            ButtonContextMenu.Items.Add(mi3);
        }

        void Mi3Click(object sender, RoutedEventArgs e)
        {
            if (EditingMode != InkCanvasEditingMode.None)
            {
                var diagram = ((Control)((ContextMenu)((MenuItem)sender).Parent).PlacementTarget).DataContext as IDiagram ??
                              ((Control)((ContextMenu)((MenuItem)sender).Parent).PlacementTarget).Tag as IDiagram;
                OnWidgetRemoved(EventArgs.Empty, diagram);
            }
        }

        void Mi2Click(object sender, RoutedEventArgs e)
        {
            if (EditingMode != InkCanvasEditingMode.None)
            {
                var diagram = ((Control)((ContextMenu)((MenuItem)sender).Parent).PlacementTarget).DataContext as IDiagram ??
                              ((Control)((ContextMenu)((MenuItem)sender).Parent).PlacementTarget).Tag as IDiagram;
                if (diagram != null)
                    diagram.EditSettings();
            }
        }

        void MenuItemClick(object sender, RoutedEventArgs e)
        {
            if (EditingMode != InkCanvasEditingMode.None)
            {
                var diagram = ((Control)((ContextMenu)((MenuItem)sender).Parent).PlacementTarget).DataContext as IDiagram ??
                              ((Control)((ContextMenu)((MenuItem)sender).Parent).PlacementTarget).Tag as IDiagram;
                if (diagram != null)
                    diagram.EditProperties();
            }
        }
    }
}
