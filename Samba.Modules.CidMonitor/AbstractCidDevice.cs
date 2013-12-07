using System;
using System.IO;
using System.Linq;
using System.Threading;
using Samba.Domain.Models.Entities;
using Samba.Infrastructure.Data.Serializer;
using Samba.Infrastructure.Settings;
using Samba.Localization.Properties;
using Samba.Presentation.Common.Services;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.CidMonitor
{
    abstract class AbstractCidDevice : IDevice
    {
        private readonly ICacheService _cacheService;
        private readonly IApplicationState _applicationState;
        private readonly IEntityService _entityService;

        protected EntityType CustomerType { get; set; }
        private bool IsInitialized { get; set; }
        public string Name { get { return GetName(); } }
        public DeviceType DeviceType { get { return GetDeviceType(); } }

        protected AbstractCidDevice(ICacheService cacheService, IApplicationState applicationState, IEntityService entityService)
        {
            _cacheService = cacheService;
            _applicationState = applicationState;
            _entityService = entityService;
        }

        public void InitializeDevice()
        {
            if (IsInitialized) return;
            CustomerType = _cacheService.GetEntityTypes().SingleOrDefault(x => x.Name == Resources.Customers);
            IsInitialized = DoInitialize();
        }

        public void FinalizeDevice()
        {
            if (!IsInitialized) return;
            DoFinalize();
            IsInitialized = false;
        }

        protected abstract DeviceType GetDeviceType();
        protected abstract string GetName();
        protected abstract bool DoInitialize();
        protected abstract void DoFinalize();
        protected abstract AbstractCidSettings GetSettings();

        protected void ProcessPhoneNumber(string phoneNumber)
        {
            phoneNumber = phoneNumber.Trim();
            if (!string.IsNullOrEmpty(GetSettings().TrimChars ?? ""))
                GetSettings().TrimChars.ToList().ForEach(x => phoneNumber = phoneNumber.TrimStart(x));
            if (string.IsNullOrEmpty(phoneNumber)) return;
            var thread = new Thread(() => _applicationState.MainDispatcher.Invoke(new Action(() => Process(phoneNumber))));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        protected string GetSettingFileName()
        {
            return string.Format("{0}\\{1}Settings.txt", LocalSettings.DataPath, Name);
        }

        protected T LoadSettings<T>() where T : class, new()
        {
            if (File.Exists(GetSettingFileName()))
            {
                var settingsData = File.ReadAllText(GetSettingFileName());
                return ObjectCloner.Deserialize<T>(settingsData);
            }
            return new T();
        }

        public object GetSettingsObject()
        {
            return GetSettings();
        }

        public void SaveSettings()
        {
            var data = ObjectCloner.Serialize(GetSettings());
            File.WriteAllText(GetSettingFileName(), data);
        }

        private void Process(string phoneNumber)
        {
            if (CustomerType != null)
            {
                var popupName = GetSettings().PopupName;
                if (string.IsNullOrWhiteSpace(popupName)) popupName = Name;
                var sr = _entityService.SearchEntities(CustomerType, phoneNumber, "");
                if (sr.Count == 1)
                {
                    var entity = sr.First();
                    InteractionService.UserIntraction.DisplayPopup(popupName, CustomerType.GetFormattedDisplayName(entity.Name, entity),
                        entity.Name + " " + Resources.Calling + ".\r" +
                        entity.SearchString + "\r", "DarkRed", OnClick, phoneNumber);
                }
                else
                    InteractionService.UserIntraction.DisplayPopup(popupName, phoneNumber,
                        phoneNumber + " " + Resources.Calling + "...", "DarkRed",
                        OnClick, phoneNumber);
            }

            _applicationState.NotifyEvent(RuleEventNames.DeviceEventGenerated,
                new { DeviceName = Name, EventName = "CID_Event", EventData = phoneNumber });
        }

        private void OnClick(object phoneNumber)
        {
            var phone = phoneNumber.ToString();
            if (string.IsNullOrEmpty(CustomerType.PrimaryFieldName) &&
                CustomerType.EntityCustomFields.Any(x => x.Name == Resources.Phone))
                phone = Resources.Phone + ":" + phone;
            OperationRequest<Entity>.Publish(Entity.GetNullEntity(CustomerType.Id), EventTopicNames.SelectEntity, EventTopicNames.EntitySelected, phone);
        }
    }
}