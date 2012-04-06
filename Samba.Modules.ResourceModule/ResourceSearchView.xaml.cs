using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Samba.Domain.Models.Resources;
using Samba.Presentation.Common;

namespace Samba.Modules.ResourceModule
{
    /// <summary>
    /// Interaction logic for AccountSearchView.xaml
    /// </summary>

    [Export]
    public partial class ResourceSearchView : UserControl
    {
        [ImportingConstructor]
        public ResourceSearchView(ResourceSearchViewModel viewModel)
        {
            DataContext = viewModel;
            viewModel.SelectedResourceTemplateChanged += viewModel_SelectedAccountTemplateChanged;
            InitializeComponent();
        }

        void viewModel_SelectedAccountTemplateChanged(object sender, System.EventArgs e)
        {
            var gridView = MainListView.View as GridView;
            var selector = sender as ResourceSearchViewModel;
            if (selector != null && gridView != null)
            {
                gridView.Columns.Where(x => x.Header.ToString() != "Name").ToList().ForEach(x => gridView.Columns.Remove(x));
                if (selector.SelectedResource != null)
                    selector.SelectedResourceTemplate.ResoruceCustomFields.Where(x => !x.Hidden).Select(CreateColumn).ToList().ForEach(x => gridView.Columns.Add(x));
                MainListView.RaiseEvent(new RoutedEventArgs(LoadedEvent, MainListView));
            }
        }

        private static GridViewColumn CreateColumn(ResourceCustomField customField)
        {
            var template = new DataTemplate { DataType = typeof(string) };
            var fef = new FrameworkElementFactory(typeof(TextBlock));
            fef.SetBinding(TextBlock.TextProperty, new Binding("[" + customField.Name + "]") { StringFormat = customField.EditingFormat });
            template.VisualTree = fef;
            var c = new GridViewColumn { Header = customField.Name, CellTemplate = template };
            Presentation.Common.ListViewLM.ProportionalColumn.ApplyWidth(c, 1);
            return c;
        }

        private void SearchStringPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                if (((ResourceSearchViewModel)DataContext).SelectResourceCommand.CanExecute(""))
                    ((ResourceSearchViewModel)DataContext).SelectResourceCommand.Execute("");
            }
        }

        private void FlexButtonClick(object sender, RoutedEventArgs e)
        {
            Reset();
        }

        private void Reset()
        {
            ((ResourceSearchViewModel)DataContext).RefreshSelectedResource(null);
            SearchString.BackgroundFocus();
        }

        private void SearchStringLoaded(object sender, RoutedEventArgs e)
        {
            SearchString.BackgroundFocus();
        }
    }
}
