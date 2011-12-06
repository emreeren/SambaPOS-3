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
            //adds the new items in the children collection
            foreach (IDiagram item in e.NewItems)
            {
                AddControl(item);
            }
        }

        public static ContextMenu ButtonContextMenu { get; set; }

        protected static void AddControl(IDiagram buttonHolder, InkCanvas parentControl)
        {
            var ret = new FlexButton.FlexButton { DataContext = buttonHolder, ContextMenu = ButtonContextMenu };
            ret.CommandParameter = buttonHolder;
            parentControl.Children.Add(ret);

            var heightBinding = new Binding("Height") { Source = buttonHolder, Mode = BindingMode.TwoWay };
            var widthBinding = new Binding("Width") { Source = buttonHolder, Mode = BindingMode.TwoWay };
            var xBinding = new Binding("X") { Source = buttonHolder, Mode = BindingMode.TwoWay };
            var yBinding = new Binding("Y") { Source = buttonHolder, Mode = BindingMode.TwoWay };
            var captionBinding = new Binding("Caption") { Source = buttonHolder, Mode = BindingMode.TwoWay };
            var radiusBinding = new Binding("CornerRadius") { Source = buttonHolder, Mode = BindingMode.TwoWay };
            var buttonColorBinding = new Binding("ButtonColor") { Source = buttonHolder, Mode = BindingMode.TwoWay };
            var commandBinding = new Binding("Command") { Source = buttonHolder, Mode = BindingMode.OneWay };
            var enabledBinding = new Binding("IsEnabled") { Source = buttonHolder, Mode = BindingMode.OneWay };
            var transformBinding = new Binding("RenderTransform") { Source = buttonHolder, Mode = BindingMode.OneWay };

            ret.SetBinding(LeftProperty, xBinding);
            ret.SetBinding(TopProperty, yBinding);
            ret.SetBinding(HeightProperty, heightBinding);
            ret.SetBinding(WidthProperty, widthBinding);
            ret.SetBinding(ContentControl.ContentProperty, captionBinding);
            ret.SetBinding(FlexButton.FlexButton.CornerRadiusProperty, radiusBinding);
            ret.SetBinding(FlexButton.FlexButton.ButtonColorProperty, buttonColorBinding);
            ret.SetBinding(ButtonBase.CommandProperty, commandBinding);
            ret.SetBinding(RenderTransformProperty, transformBinding);
            ret.SetBinding(IsEnabledProperty, enabledBinding);
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
