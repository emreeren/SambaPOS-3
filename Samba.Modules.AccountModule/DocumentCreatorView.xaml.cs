using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
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
using Samba.Presentation.Common;

namespace Samba.Modules.AccountModule
{
    /// <summary>
    /// Interaction logic for DocumentCreatorView.xaml
    /// </summary>
    
    [Export]
    public partial class DocumentCreatorView : UserControl
    {

        [ImportingConstructor]
        public DocumentCreatorView(DocumentCreatorViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            DescriptionEdit.BackgroundFocus();
        }
    }
}
