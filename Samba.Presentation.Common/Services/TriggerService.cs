using System;
using System.Collections.Generic;
using Samba.Domain.Models.Settings;
using Samba.Infrastructure.Cron;
using Samba.Persistance.Data;
using Samba.Services;

namespace Samba.Presentation.Common.Services
{
    public static class TriggerService
    {
        private static readonly List<CronObject> CronObjects = new List<CronObject>();

        public static void UpdateCronObjects()
        {
            CloseTriggers();

            var triggers = Dao.Query<Trigger>();
            foreach (var trigger in triggers)
            {
                var dataContext = new CronObjectDataContext
                    {
                        Object = trigger,
                        LastTrigger = trigger.LastTrigger,
                        CronSchedules = new List<CronSchedule> { CronSchedule.Parse(trigger.Expression) }
                    };
                var cronObject = new CronObject(dataContext);
                cronObject.OnCronTrigger += OnCronTrigger;
                CronObjects.Add(cronObject);
            }
            CronObjects.ForEach(x => x.Start());
        }

        public static void CloseTriggers()
        {
            foreach (var cronObject in CronObjects)
            {
                cronObject.Stop();
                cronObject.OnCronTrigger -= OnCronTrigger;
            }
            CronObjects.Clear();
        }

        private static void OnCronTrigger(CronObject cronobject)
        {
            using (var workspace = WorkspaceFactory.Create())
            {
                var trigger = workspace.Single<Trigger>(x => x.Id == ((Trigger)cronobject.Object).Id);
                if (trigger != null)
                {
                    trigger.LastTrigger = DateTime.Now;
                    workspace.CommitChanges();
                    if (AppServices.ActiveAppScreen != AppScreens.Dashboard)
                        RuleExecutor.NotifyEvent(RuleEventNames.TriggerExecuted, new { TriggerName = trigger.Name });
                }
                else cronobject.Stop();
            }
        }
    }
}
