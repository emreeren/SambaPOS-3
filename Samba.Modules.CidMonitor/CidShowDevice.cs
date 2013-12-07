using System;
using System.ComponentModel.Composition;
using Axcidv5callerid;
using Samba.Localization.Properties;
using Samba.Presentation.Common.Services;
using Samba.Presentation.Services;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.CidMonitor
{
    [Export(typeof(IDevice))]
    class CidShowDevice : AbstractCidDevice
    {
        private FrmMain _frmMain;
        private AbstractCidSettings _settings;
        public AbstractCidSettings Settings { get { return _settings ?? (_settings = LoadSettings<AbstractCidSettings>()); } }

        [ImportingConstructor]
        public CidShowDevice(IApplicationState applicationState, IEntityService entityService, ICacheService cacheService)
            : base(cacheService, applicationState, entityService)
        {

        }

        protected override DeviceType GetDeviceType()
        {
            return DeviceType.CallerId;
        }

        protected override string GetName()
        {
            return "CID Show";
        }

        protected override bool DoInitialize()
        {
            try
            {
                _frmMain = new FrmMain();
                _frmMain.axCIDv51.OnCallerID += axCIDv51_OnCallerID;
                _frmMain.axCIDv51.Start();
                return true;
            }
            catch (Exception)
            {
                InteractionService.UserIntraction.DisplayPopup("", Resources.Information, Resources.CallerIdDriverError);
                return false;
            }
        }

        protected override void DoFinalize()
        {
            _frmMain.Dispose();
            _frmMain = null;
        }

        protected override AbstractCidSettings GetSettings()
        {
            return Settings;
        }

        void axCIDv51_OnCallerID(object sender, ICIDv5Events_OnCallerIDEvent e)
        {
            var pn = e.phoneNumber;
            ProcessPhoneNumber(pn);
        }
    }
}
