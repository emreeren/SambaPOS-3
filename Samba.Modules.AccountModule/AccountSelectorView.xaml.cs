using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Samba.Domain.Models.Accounts;
using Samba.Presentation.Common;

namespace Samba.Modules.AccountModule
{
    /// <summary>
    /// Interaction logic for AccountSearchView.xaml
    /// </summary>

    [Export]
    public partial class AccountSelectorView : UserControl
    {
        [ImportingConstructor]
        public AccountSelectorView(AccountSelectorViewModel viewModel)
        {
            DataContext = viewModel;
            viewModel.SelectedAccountTemplateChanged += viewModel_SelectedAccountTemplateChanged;
            InitializeComponent();
        }

        //Header = priceTag,
        //Binding = new Binding("[" + i + "]") { StringFormat = "#,#0.00;-#,#0.00;-" },
        //MinWidth = 60,
        //CellStyle = (Style)FindResource("RightAlignedCellStyle")

        void viewModel_SelectedAccountTemplateChanged(object sender, System.EventArgs e)
        {
            var gridView = MainListView.View as GridView;
            var selector = sender as AccountSelectorViewModel;
            if (selector != null && gridView != null)
            {
                gridView.Columns.Where(x => x.Header.ToString() != "Account Name").ToList().ForEach(x => gridView.Columns.Remove(x));
                selector.SelectedAccountTemplate.AccountCustomFields.Select(CreateColumn).ToList().ForEach(x => gridView.Columns.Add(x));
                MainListView.RaiseEvent(new RoutedEventArgs(LoadedEvent, MainListView));
            }
        }

        private static GridViewColumn CreateColumn(AccountCustomField accountCustomField)
        {
            var template = new DataTemplate { DataType = typeof(string) };
            var fef = new FrameworkElementFactory(typeof(TextBlock));
            fef.SetBinding(TextBlock.TextProperty, new Binding("[" + accountCustomField.Name + "]") { StringFormat = accountCustomField.EditingFormat });
            template.VisualTree = fef;
            var c = new GridViewColumn { Header = accountCustomField.Name, CellTemplate = template };
            Presentation.Common.ListViewLM.ProportionalColumn.ApplyWidth(c, 1);
            return c;
        }

        private void SearchStringPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                if (((AccountSelectorViewModel)DataContext).SelectAccountCommand.CanExecute(""))
                    ((AccountSelectorViewModel)DataContext).SelectAccountCommand.Execute("");
            }
        }

        private void TicketNoPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                if (((AccountSelectorViewModel)DataContext).FindTicketCommand.CanExecute(""))
                    ((AccountSelectorViewModel)DataContext).FindTicketCommand.Execute("");
            }
        }

        private void FlexButtonClick(object sender, RoutedEventArgs e)
        {
            Reset();
        }

        private void Reset()
        {
            ((AccountSelectorViewModel)DataContext).RefreshSelectedAccount();
            SearchString.BackgroundFocus();
        }

        private void SearchStringLoaded(object sender, RoutedEventArgs e)
        {
            SearchString.BackgroundFocus();
        }
    }
}
