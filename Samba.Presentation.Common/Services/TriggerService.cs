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
                var dataContext = new CronObjectDataContext(new List<CronSchedule> { CronSchedule.Parse(trigger.Expression) })
                    {
                        Object = trigger,
                        LastTrigger = trigger.LastTrigger
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

        private static void OnCronTrigger(object sender, CronEventArgs e)
        {
            using (var workspace = WorkspaceFactory.Create())
            {
                var trigger = workspace.Single<Trigger>(x => x.Id == ((Trigger)e.CronObject.Object).Id);
                if (trigger != null)
                {
                    trigger.LastTrigger = DateTime.Now;
                    workspace.CommitChanges();
                    if (AppServices.ActiveAppScreen != AppScreens.Dashboard)
                        RuleExecutor.NotifyEvent(RuleEventNames.TriggerExecuted, new { TriggerName = trigger.Name });
                }
                else e.CronObject.Stop();
            }
        }
    }
}
