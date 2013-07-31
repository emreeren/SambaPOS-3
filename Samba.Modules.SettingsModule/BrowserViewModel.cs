using System;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.SettingsModule
{
    class BrowserViewModel : VisibleViewModelBase
    {
        protected string Header = Resources.InternetBrowser;
        protected string Url = "about:Blank";
        
        private Uri _activeUrl;

        public BrowserViewModel()
        {
            ActiveUrl = new Uri("about:Blank");
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
        {
            return typeof(BrowserView);
        }

        public override void OnShown()
        {
            if (ActiveUrl == new Uri("about:Blank"))
            { ActiveUrl = new Uri(Url); }
        }

        protected override string GetHeaderInfo()
        {
            return Header;
        }
    }
}
