using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Samba.Modules.PosModule
{
    /// <summary>
    /// Interaction logic for MenuItemSelectorView.xaml
    /// </summary>
    /// 

    [Export]
    public partial class MenuItemSelectorView : UserControl
    {
        private readonly GridLength _thin = GridLength.Auto;
        private readonly GridLength _auto15 = new GridLength(15, GridUnitType.Star);
        private readonly GridLength _auto25 = new GridLength(25, GridUnitType.Star);
        private readonly GridLength _auto45 = new GridLength(45, GridUnitType.Star);

        [ImportingConstructor]
        public MenuItemSelectorView(MenuItemSelectorViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }

        private void ItemsControl_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            MainGrid.ColumnDefinitions[0].Width = ((ItemsControl)sender).Items.Count == 0 ? _thin : _auto25;
        }

        private void ItemsControl_TargetUpdated_1(object sender, DataTransferEventArgs e)
        {
            var sw = DataContext as MenuItemSelectorViewModel;
            if (sw == null) return;
            NumeratorRow.Height = sw.IsNumeratorVisible ? _auto45 : _thin;
            //AlphaButtonsColumn.Width = sw.AlphaButtonValues != null && sw.AlphaButtonValues.Length > 0 ? _auto15 : _thin;
        }
    }
}
