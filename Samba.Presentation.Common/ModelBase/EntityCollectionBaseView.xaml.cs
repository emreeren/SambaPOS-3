using System;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Samba.Presentation.Common.ModelBase
{
    /// <summary>
    /// Interaction logic for EntityCollectionBaseView.xaml
    /// </summary>
    public partial class EntityCollectionBaseView : UserControl
    {
        private readonly Timer _updateTimer;
        private string _beforeText;

        public EntityCollectionBaseView()
        {
            InitializeComponent();
            _updateTimer = new Timer(500);
            _updateTimer.Elapsed += UpdateTimerElapsed;
        }

        private void UpdateTimerElapsed(object sender, ElapsedEventArgs e)
        {
            _updateTimer.Stop();

            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(RefreshItems));
        }

        private void RefreshItems()
        {
            if (FilterTextBox.Text != _beforeText)
                ((AbstractEntityCollectionViewModelBase)DataContext).RefreshItems();
        }

        private void MainGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var bm = (DataContext as AbstractEntityCollectionViewModelBase);
            if (bm != null && bm.EditItemCommand.CanExecute(null))
                bm.EditItemCommand.Execute(null);
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var baseModelView = DataContext as AbstractEntityCollectionViewModelBase;
            if (baseModelView != null && baseModelView.CustomCommands.Count > 0)
            {
                MainGrid.ContextMenu.Items.Add(new Separator());
                foreach (var item in ((AbstractEntityCollectionViewModelBase)DataContext).CustomCommands)
                {
                    MainGrid.ContextMenu.Items.Add(
                        new MenuItem { Command = item, Header = item.Caption });
                }
            }
        }

        private void MainGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (((AbstractEntityCollectionViewModelBase)DataContext).EditItemCommand.CanExecute(null))
                    ((AbstractEntityCollectionViewModelBase)DataContext).EditItemCommand.Execute(null);
            }
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            _updateTimer.Stop();
            if (e.Key == Key.Enter)
            {
                ((AbstractEntityCollectionViewModelBase)DataContext).RefreshItems();
                FilterTextBox.SelectAll();
            }
            else if (e.Key == Key.Down)
            {
                MainGrid.BackgroundFocus();
            }
            else
            {
                _beforeText = FilterTextBox.Text;
                _updateTimer.Start();
            }
        }
    }
}
