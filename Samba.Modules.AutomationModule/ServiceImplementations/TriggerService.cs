using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Samba.Domain.Models.Settings;
using Samba.Infrastructure.Cron;
using Samba.Persistance.Data;
using Samba.Presentation.Common;
using Samba.Services;

namespace Samba.Modules.AutomationModule.ServiceImplementations
{
    [Export(typeof(ITriggerService))]
    public class TriggerService : ITriggerService
    {
        private readonly List<CronObject> _cronObjects;
        private readonly IApplicationState _applicationState;
        private readonly IRuleService _ruleService;

        [ImportingConstructor]
        public TriggerService(IApplicationState applicationState, IRuleService ruleService)
        {
            _applicationState = applicationState;
            _ruleService = ruleService;
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
                    if (_applicationState.ActiveAppScreen != AppScreens.Dashboard)
                        _ruleService.NotifyEvent(RuleEventNames.TriggerExecuted, new { TriggerName = trigger.Name });
                }
                else e.CronObject.Stop();
            }
        }
    }
}
