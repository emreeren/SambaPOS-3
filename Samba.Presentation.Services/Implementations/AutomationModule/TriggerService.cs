using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Samba.Domain.Models.Settings;
using Samba.Infrastructure.Cron;
using Samba.Persistance.Data;
using Samba.Presentation.Services.Common;
using Samba.Services.Common;

namespace Samba.Presentation.Services.Implementations.AutomationModule
{
    [Export(typeof(ITriggerService))]
    public class TriggerService : ITriggerService
    {
        private readonly List<CronObject> _cronObjects;
        private readonly IApplicationState _applicationState;

        [ImportingConstructor]
        public TriggerService(IApplicationState applicationState)
        {
            _applicationState = applicationState;
            _cronObjects = new List<CronObject>();
        }

        public void UpdateCronObjects()
        {
            CloseTriggers();

            var triggers = Dao.Query<Trigger>();
            foreach (var trigger in triggers)
            {
                var dataContext = new CronObjectDataContext(new List<CronSchedule> { CronSchedule.Parse(trigger.Expression) })
                    {
                        Object = trigger,
                        LastTrigger = trigger.LastTrigger
                    };

                var cronObject = new CronObject(dataContext);
                cronObject.OnCronTrigger += OnCronTrigger;
                _cronObjects.Add(cronObject);
            }
            _cronObjects.ForEach(x => x.Start());
        }

        public void CloseTriggers()
        {
            foreach (var cronObject in _cronObjects)
            {
                cronObject.Stop();
                cronObject.OnCronTrigger -= OnCronTrigger;
            }
            _cronObjects.Clear();
        }

        private void OnCronTrigger(object sender, CronEventArgs e)
        {
            using (var workspace = WorkspaceFactory.Create())
            {
                var trigger = workspace.Single<Trigger>(x => x.Id == ((Trigger)e.CronObject.Object).Id);
                if (trigger != null)
                {
                    trigger.LastTrigger = DateTime.Now;
                    workspace.CommitChanges();
                    if (_applicationState.ActiveAppScreen != AppScreens.Management)
                        _applicationState.NotifyEvent(RuleEventNames.TriggerExecuted, new { TriggerName = trigger.Name });
                }
                else e.CronObject.Stop();
            }
        }
    }
}
