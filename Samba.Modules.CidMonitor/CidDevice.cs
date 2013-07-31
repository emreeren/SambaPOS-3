using System;
using System.ComponentModel.Composition;
using System.Linq;
using Axcidv5callerid;
using Samba.Domain.Models.Entities;
using Samba.Localization.Properties;
using Samba.Presentation.Common.Services;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.CidMonitor
{
    [Export(typeof(IDevice))]
    public class CidDevice : IDevice
    {
        private readonly IApplicationState _applicationState;
        private readonly IEntityService _entityService;
        private readonly ICacheService _cacheService;

        [ImportingConstructor]
        public CidDevice(IApplicationState applicationState, IEntityService entityService, ICacheService cacheService)
        {
            _applicationState = applicationState;
            _entityService = entityService;
            _cacheService = cacheService;
        }

        protected EntityType CustomerType { get; set; }

        public bool IsInitialized { get; set; }
        public string Name { get { return "CID Easy"; } }
        public void Initialize()
        {
            if (IsInitialized) return;
            CustomerType = _cacheService.GetEntityTypes().SingleOrDefault(x => x.Name == Resources.Customers);
            try
            {
                var frmMain = new FrmMain();
                frmMain.axCIDv51.OnCallerID += axCIDv51_OnCallerID;
                frmMain.axCIDv51.Start();
                IsInitialized = true;
            }
            catch (Exception)
            {
#if DEBUG
                var i = 0;
#else
                InteractionService.UserIntraction.DisplayPopup(Resources.Information, Resources.CallerIdDriverError);
#endif
            }
        }

        void axCIDv51_OnCallerID(object sender, ICIDv5Events_OnCallerIDEvent e)
        {
            var pn = e.phoneNumber;
            pn = pn.TrimStart('+');
            pn = pn.TrimStart('0');
            pn = pn.TrimStart('9');
            pn = pn.TrimStart('0');

            PublishPhoneNumber(pn);
        }

        private void PublishPhoneNumber(string phoneNumber)
        {
            if (CustomerType != null)
            {
                var sr = _entityService.SearchEntities(CustomerType, phoneNumber, "");
                if (sr.Count == 1)
                {
                    var entity = sr.First();
                    InteractionService.UserIntraction.DisplayPopup(entity.Name,
                                                                   entity.Name + " " + Resources.Calling + ".\r" +
                                                                   entity.SearchString + "\r", "DarkRed", OnClick, phoneNumber);
                }
                else
                    InteractionService.UserIntraction.DisplayPopup(phoneNumber,
                                                                   phoneNumber + " " + Resources.Calling + "...", "DarkRed",
                                                                   OnClick, phoneNumber);
            }

            _applicationState.NotifyEvent(RuleEventNames.DeviceEventGenerated,
                                          new { DeviceName = Name, EventName = "CID_Event", EventData = phoneNumber });
        }

        private void OnClick(object phoneNumber)
        {
            OperationRequest<Entity>.Publish(Entity.GetNullEntity(CustomerType.Id), EventTopicNames.SelectEntity, EventTopicNames.EntitySelected, phoneNumber.ToString());
        }
    }
}
