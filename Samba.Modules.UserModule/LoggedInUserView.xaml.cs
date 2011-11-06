using System.Windows.Controls;
using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Events;

namespace Samba.Modules.UserModule
{
    /// <summary>
    /// Interaction logic for LoggedInUserView.xaml
    /// </summary>
    
    [Export]
    public partial class LoggedInUserView : UserControl
    {
        private IEventAggregator _eventAggregator;
        private LoggedInUserViewModel _viewModel;

        [ImportingConstructor]
        public LoggedInUserView(LoggedInUserViewModel viewModel,IEventAggregator eventAggregator)
        {
            InitializeComponent();
            DataContext = viewModel;
            _eventAggregator = eventAggregator;
            _viewModel = viewModel;
        }
    }
}
