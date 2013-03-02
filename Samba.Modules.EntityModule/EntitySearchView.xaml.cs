using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Samba.Domain.Models.Entities;
using Samba.Presentation.Common;
using Samba.Presentation.Controls.ListViewLM;

namespace Samba.Modules.EntityModule
{
    /// <summary>
    /// Interaction logic for AccountSearchView.xaml
    /// </summary>

    [Export]
    public partial class EntitySearchView : UserControl
    {
        [ImportingConstructor]
        public EntitySearchView(EntitySearchViewModel viewModel)
        {
            DataContext = viewModel;
            viewModel.SelectedEntityTypeChanged += viewModel_SelectedAccountTypeChanged;
            InitializeComponent();
        }

        void viewModel_SelectedAccountTypeChanged(object sender, System.EventArgs e)
        {
            var gridView = MainListView.View as GridView;
            var selector = sender as EntitySearchViewModel;
            if (selector != null && gridView != null)
            {
                gridView.Columns.Where(x => x.Header.ToString() != Localization.Properties.Resources.Name).ToList().ForEach(x => gridView.Columns.Remove(x));
                if (selector.SelectedEntityType != null)
                    selector.SelectedEntityType.EntityCustomFields.Where(x => !x.Hidden).Select(CreateColumn).ToList().ForEach(x => gridView.Columns.Add(x));
                MainListView.RaiseEvent(new RoutedEventArgs(LoadedEvent, MainListView));
            }
        }

        private static GridViewColumn CreateColumn(EntityCustomField customField)
        {
            var template = new DataTemplate { DataType = typeof(string) };
            var fef = new FrameworkElementFactory(typeof(TextBlock));
            fef.SetBinding(TextBlock.TextProperty, new Binding("[" + customField.Name + "]") { StringFormat = customField.EditingFormat });
            template.VisualTree = fef;
            var c = new GridViewColumn { Header = customField.Name, CellTemplate = template };
            ProportionalColumn.ApplyWidth(c, 1);
            return c;
        }

        private void SearchStringPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                if (((EntitySearchViewModel)DataContext).SelectEntityCommand.CanExecute(""))
                    ((EntitySearchViewModel)DataContext).SelectEntityCommand.Execute("");
            }
        }

        private void FlexButtonClick(object sender, RoutedEventArgs e)
        {
            Reset();
        }

        private void Reset()
        {
            ((EntitySearchViewModel)DataContext).RefreshSelectedEntity(null);
            SearchString.BackgroundFocus();
        }

        private void SearchStringLoaded(object sender, RoutedEventArgs e)
        {
            SearchString.BackgroundFocus();
        }
    }
}
