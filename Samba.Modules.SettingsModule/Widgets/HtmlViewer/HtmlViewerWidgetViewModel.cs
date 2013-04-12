using System.ComponentModel;
using System.Linq;
using Samba.Domain.Models.Entities;
using Samba.Infrastructure.Helpers;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Widgets;
using Samba.Presentation.Services;

namespace Samba.Modules.SettingsModule.Widgets.HtmlViewer
{
    public class HtmlViewerWidgetViewModel : WidgetViewModel
    {
        [Browsable(false)]
        public HtmlViewerWidgetSettings Settings { get { return SettingsObject as HtmlViewerWidgetSettings; } }

        private string _url;
        [Browsable(false)]
        public string Url
        {
            get { return _url; }
            set
            {
                _url = value;
                RaisePropertyChanged(() => Url);
            }
        }

        public HtmlViewerWidgetViewModel(Widget model, IApplicationState applicationState)
            : base(model, applicationState)
        {

        }

        protected override object CreateSettingsObject()
        {
            return JsonHelper.Deserialize<HtmlViewerWidgetSettings>(_model.Properties);
        }

        public override void Refresh()
        {
            Url = "about:blank";
            Url = Settings.Url;
        }
    }
}