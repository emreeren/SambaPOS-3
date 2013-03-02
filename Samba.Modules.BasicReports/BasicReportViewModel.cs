using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;
using Samba.Domain.Models.Users;
using Samba.Presentation.Common;
using Samba.Presentation.Services.Common;

namespace Samba.Modules.BasicReports
{
    [Export]
    public class BasicReportViewModel : ObservableObject
    {
        public IEnumerable<ReportViewModelBase> Reports { get { return ReportContext.Reports; } }
        public DelegateCommand<ReportViewModelBase> ReportExecuteCommand { get; set; }

        private ReportViewModelBase _activeReport;
        public ReportViewModelBase ActiveReport
        {
            get { return _activeReport; }
            set
            {
                _activeReport = value;
                RaisePropertyChanged(()=>ActiveReport);
                RaisePropertyChanged(()=>IsReportVisible);
            }
        }

        public bool IsReportVisible { get { return ActiveReport != null; } }

        public BasicReportViewModel()
        {
            ReportExecuteCommand = new DelegateCommand<ReportViewModelBase>(OnExecuteReport);

            EventServiceFactory.EventService.GetEvent<GenericEvent<User>>().Subscribe(x =>
            {
                if (x.Topic == EventTopicNames.UserLoggedOut && ActiveReport != null)
                {
                    ActiveReport.Document = null;
                    ActiveReport = null;
                }
            });

            EventServiceFactory.EventService.GetEvent<GenericEvent<EventAggregator>>().Subscribe(
                x =>
                {
                    if (x.Topic == EventTopicNames.ActivateNavigation && ActiveReport != null)
                    {
                        ActiveReport.Document = null; ActiveReport = null;
                    }
                });
        }

        private void OnExecuteReport(ReportViewModelBase obj)
        {
            foreach (var report in Reports) report.Selected = false;
            obj.Selected = true;
            ActiveReport = obj;
            ActiveReport.RefreshReport();
        }
    }
}
