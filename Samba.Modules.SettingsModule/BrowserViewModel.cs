using System;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Services.Common;

namespace Samba.Modules.SettingsModule
{
    class BrowserViewModel : VisibleViewModelBase
    {
        protected string header = Resources.InternetBrowser;
        protected string url = "about:Blank";
        private Uri _activeUrl;

        public BrowserViewModel()
        {
            ActiveUrl = new Uri("about:Blank");
            EventServiceFactory.EventService.GetEvent<GenericEvent<Uri>>().Subscribe(OnBrowseUri);
        }

        public Uri ActiveUrl
        {
            get { return _activeUrl; }
            set
            {
                _activeUrl = value;
                RaisePropertyChanged(() => ActiveUrl);
            }
        }
        
        public override Type GetViewType()
        { return typeof(BrowserView); }

        public override void OnShown()
        {
            if (ActiveUrl == new Uri("about:Blank"))
            { new Uri(url).PublishEvent(EventTopicNames.BrowseUrl); }
        }

        protected override string GetHeaderInfo()
        { return header; }

        private void OnBrowseUri(EventParameters<Uri> obj)
        {
            if (obj.Topic == EventTopicNames.BrowseUrl)
            { ActiveUrl = obj.Value; }
        }
        }
}
