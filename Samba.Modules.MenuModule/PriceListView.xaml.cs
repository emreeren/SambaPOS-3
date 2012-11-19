using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DataGridFilterLibrary;

namespace Samba.Modules.MenuModule
{
    /// <summary>
    /// Interaction logic for PriceListView.xaml
    /// </summary>
    public partial class PriceListView : UserControl
    {
        public PriceListView()
        {
            InitializeComponent();
            DataContextChanged += PriceListView_DataContextChanged;
        }

        void PriceListView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var d = DataContext as PriceListViewModel;

            if (d != null)
            {
                var i = 0;
                foreach (var priceTag in d.PriceTags)
                {
                    DataGridColumn dgtc = new DataGridTextColumn()
                                   {
                                       Header = priceTag,
                                       Binding = new Binding("[" + i + "]") { StringFormat = "#,#0.00;-#,#0.00;-" },
                                       MinWidth = 60,
                                       CellStyle = (Style)FindResource("RightAlignedCellStyle")
                                   };
                    DataGridColumnExtensions.SetDoNotGenerateFilterControl(dgtc, true);
                    DataGrid.Columns.Add(dgtc);
                    i++;
                }
            }
        }

        private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditingElement is TextBox)
            {
                ((TextBox)e.EditingElement).Text = ((TextBox)e.EditingElement).Text.Replace("\b", "");
            }
        }

    }
}
