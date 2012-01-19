using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;

namespace Samba.Presentation.Common.ModelBase
{
    /// <summary>
    /// Interaction logic for EntityCollectionBaseView.xaml
    /// </summary>
    public partial class EntityCollectionBaseView : UserControl
    {
        public EntityCollectionBaseView()
        {
            InitializeComponent();
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

        private void MainGrid_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (((ICountable)MainGrid.DataContext).GetCount() < 10)
                MainGrid.ColumnHeaderHeight = 0;
        }

        private void MainGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (((AbstractEntityCollectionViewModelBase)DataContext).EditItemCommand.CanExecute(null))
                    ((AbstractEntityCollectionViewModelBase)DataContext).EditItemCommand.Execute(null);
            }
        }

    }
}
