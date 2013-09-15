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
    [Export]
    public partial class EntitySearchView : UserControl
    {
        public EntitySearchViewModel ViewModel { get; set; }

        [ImportingConstructor]
        public EntitySearchView(EntitySearchViewModel viewModel)
        {
            DataContext = viewModel;
            ViewModel = viewModel;
            viewModel.SelectedEntityTypeChanged += viewModel_SelectedAccountTypeChanged;
            InitializeComponent();
        }

        void viewModel_SelectedAccountTypeChanged(object sender, System.EventArgs e)
        {
            var gridView = MainListView.View as GridView;
            if (ViewModel != null && gridView != null)
            {
                gridView.Columns.Where(x => x != gridView.Columns.First()).ToList().ForEach(x => gridView.Columns.Remove(x));
                if (ViewModel.SelectedEntityType != null)
                    ViewModel.SelectedEntityType.EntityCustomFields.Where(x => !x.Hidden).Select(CreateColumn).ToList().ForEach(x => gridView.Columns.Add(x));
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
                ViewModel.SelectFullMatch();
            }
            else if (e.Key == Key.Down)
            {
                if (ViewModel.FoundEntities.Count > 0)
                {
                    e.Handled = true;
                    MainListView.Focus();
                }
            }
        }

        private void FlexButtonClick(object sender, RoutedEventArgs e)
        {
            Reset();
        }

        private void Reset()
        {
            ViewModel.ResetSearch();
            SearchString.BackgroundFocus();
        }

        private void SearchStringLoaded(object sender, RoutedEventArgs e)
        {
            SearchString.BackgroundFocus();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel.IsKeyboardVisible)
            {
                var keyboardHeight = Properties.Settings.Default.KeyboardHeight;
                if (keyboardHeight <= 10 && keyboardHeight > 1) keyboardHeight = 0;
                KeyboardRow.Height = new GridLength(keyboardHeight, GridUnitType.Star);
                var contentHeight = Properties.Settings.Default.ContentHeight;
                ContentRow.Height = new GridLength(contentHeight, GridUnitType.Star);
            }
            else
            {
                HideKeyboard();
            }
        }

        private void HideKeyboard()
        {
            KeyboardRow.Height = new GridLength(0, GridUnitType.Auto);
            KeyboardRow.MinHeight = 0;
            Keyboard.Visibility = Visibility.Collapsed;
            Splitter.Visibility = Visibility.Collapsed;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel.IsKeyboardVisible)
            {
                Properties.Settings.Default.KeyboardHeight = KeyboardRow.Height.Value;
                Properties.Settings.Default.ContentHeight = ContentRow.Height.Value;
                Properties.Settings.Default.Save();
            }
        }

        private void GridSplitter_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            KeyboardRow.Height = new GridLength(1, GridUnitType.Star);
            ContentRow.Height = new GridLength(1, GridUnitType.Star);
        }
    }
}
