using System.Windows;
using System.Windows.Controls;
using Samba.Presentation.Common;

namespace Samba.Modules.EntityModule
{
    /// <summary>
    /// Interaction logic for AccountCustomDataEditor.xaml
    /// </summary>
    public partial class EntityCustomDataEditor : UserControl
    {
        public EntityCustomDataEditor()
        {
            InitializeComponent();
        }

        private void EntityCustomDataEditor_OnLoaded(object sender, RoutedEventArgs e)
        {
            var entityCustomDataViewModel = DataContext as EntityCustomDataViewModel;
            if (entityCustomDataViewModel != null && entityCustomDataViewModel.IsMaskedTextBoxVisible)
                EntityNameEdit.BackgroundFocus();
            else
                EntityNameEdit2.BackgroundFocus();
        }
    }
}
