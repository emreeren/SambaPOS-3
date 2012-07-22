using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace Samba.Presentation.Common.UIControls
{
    public class DiagramCanvas : InkCanvas
    {
        //Just a simple INotifyCollectionChanged collection
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

        //called when a new value is set (through binding for example)
        protected static void SourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            //gets the instance that changed the "local" value
            var instance = sender as DiagramCanvas;
            //the new collection that will be set
            var newCollection = args.NewValue as ObservableCollection<IDiagram>;
            //the previous collection that was set
            var oldCollection = args.OldValue as ObservableCollection<IDiagram>;

            if (oldCollection != null)
            {
                //removes the CollectionChangedEventHandler from the old collection
                oldCollection.CollectionChanged -= instance.collection_CollectionChanged;
            }

            //clears all the previous children in the collection
            instance.Children.Clear();

            if (newCollection != null)
            {
                //adds all the children of the new collection
                foreach (IDiagram item in newCollection)
                {
                    AddControl(item, instance);
                }

                //adds a new CollectionChangedEventHandler to the new collection
                newCollection.CollectionChanged += instance.collection_CollectionChanged;
            }
        }

        //append when an Item in the collection is changed
        protected void collection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems == null) return;
            //adds the new items in the children collection
            foreach (IDiagram item in e.NewItems)
            {
                AddControl(item);
            }
        }

        public static ContextMenu ButtonContextMenu { get; set; }

        protected static void AddControl(IDiagram buttonHolder, InkCanvas parentControl)
        {
            var result = WidgetCreatorRegistry.CreateWidget(buttonHolder, ButtonContextMenu);
            if (result != null) parentControl.Children.Add(result);
        }

        protected void AddControl(IDiagram buttonHolder)
        {
            AddControl(buttonHolder, this);
        }

        static DiagramCanvas()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DiagramCanvas), new FrameworkPropertyMetadata(typeof(DiagramCanvas)));
            ButtonContextMenu = new ContextMenu();
            var menuItem = new MenuItem() { Header = "Özellikler" };
            menuItem.Click += MenuItemClick;
            ButtonContextMenu.Items.Add(menuItem);
        }

        static void MenuItemClick(object sender, RoutedEventArgs e)
        {
            ((IDiagram)((Control)((ContextMenu)((MenuItem)sender).Parent).PlacementTarget).DataContext).
                EditProperties();
        }

    }
}
