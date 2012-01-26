using System.ComponentModel.Composition;
using Samba.Presentation.Common;

namespace Samba.Modules.DeliveryModule
{
    [Export]
    public class DeliveryViewModel : ObservableObject
    {
        [ImportingConstructor]
        public DeliveryViewModel()
        {
        }

        public void SearchAccount(string phoneNumber)
        {
            //ClearSearchValues();
            //SearchString = phoneNumber;
            //UpdateFoundAccounts();
        }
    }
}
