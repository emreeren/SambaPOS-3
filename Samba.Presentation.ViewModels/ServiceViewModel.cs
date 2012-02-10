using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Settings;
using Samba.Localization.Properties;

namespace Samba.Presentation.ViewModels
{
    public class ServiceViewModel
    {
        public Calculation Model { get; set; }

        public ServiceViewModel(Calculation model)
        {
            Model = model;
        }

        public string Name { get { return Model.Name; } }
        
        public string Amount
        {
            get
            {
                return (Model.CalculationAmount).ToString(LocalSettings.DefaultCurrencyFormat);
            }
        }
    }
}
