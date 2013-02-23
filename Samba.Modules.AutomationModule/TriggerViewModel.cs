using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Windows;
using Samba.Infrastructure.Cron;
using Samba.Localization.Properties;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Services;
using Trigger = Samba.Domain.Models.Settings.Trigger;

namespace Samba.Modules.AutomationModule
{
    internal class CommonCronSetting
    {
        public CommonCronSetting(string name, string minute, string hour, string day, string month, string weekday)
        {
            SettingName = name;
            Minute = minute;
            Hour = hour;
            Day = day;
            Month = month;
            Weekday = weekday;
        }

        private string _settingName;
        public string SettingName
        {
            get { return string.Format("{0} ({1})", _settingName, Expression.Trim()); }
            set { _settingName = value; }
        }

        public string Minute { get; set; }
        public string Hour { get; set; }
        public string Day { get; set; }
        public string Month { get; set; }
        public string Weekday { get; set; }

        public string Expression { get { return string.Format("{0} {1} {2} {3} {4}", Minute, Hour, Day, Month, Weekday); } }
    }

    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    class TriggerViewModel : EntityViewModelBase<Trigger>
    {
        private readonly ITriggerService _triggerService;
        private readonly IMethodQueue _methodQueue;

        [ImportingConstructor]
        public TriggerViewModel(ITriggerService triggerService,IMethodQueue methodQueue)
        {
            _triggerService = triggerService;
            _methodQueue = methodQueue;
            TestExpressionCommand = new CaptionCommand<string>("Test", OnTestExpression);
        }

        private DateTime? GetNextDateTime()
        {
            try
            {
                DateTime nextTime;
                var cs = CronSchedule.Parse(Expression);
                cs.GetNext(DateTime.Now, out nextTime);
                return nextTime;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private string GetTestMessage()
        {
            var nextTime = GetNextDateTime();
            if (nextTime != null)
            {
                var ts = nextTime.GetValueOrDefault() - DateTime.Now;
                return Resources.ExpressionValid + "\r\n" + string.Format(Resources.TriggerTestResultMessage_f,
                     nextTime, ts.Days, ts.Hours, ts.Minutes);
            }
            return Resources.ErrorInExpression + "!";
        }

        private void OnTestExpression(string obj)
        {
            MessageBox.Show(GetTestMessage());
        }

        private void GenerateCommonSettings()
        {
            CommonCronSettings = new List<CommonCronSetting>
                                     {
                                         new CommonCronSetting(Resources.EveryMinute, "*", "*", "*", "*", "*"),
                                         new CommonCronSetting(Resources.Every5Minutes, "*/5", "*", "*", "*", "*"),
                                         new CommonCronSetting(Resources.TwiceAnHour, "0,30", "*", "*", "*", "*"),
                                         new CommonCronSetting(Resources.OnceAnHour, "0", "*", "*", "*", "*"),
                                         new CommonCronSetting(Resources.TwiceADay, "0", "0,12", "*", "*", "*"),
                                         new CommonCronSetting(Resources.OnceADay, "0", "0", "*", "*", "*"),
                                         new CommonCronSetting(Resources.OnceAWeek, "0", "0", "*", "*", "0"),
                                         new CommonCronSetting(Resources.FirstAndFifteenth, "0", "0", "1,15", "*", "*"),
                                         new CommonCronSetting(Resources.OnceAMonth, "0", "0", "1", "*", "*"),
                                         new CommonCronSetting(Resources.OnceAYear, "0", "0", "1", "1", "*")
                                     };

            CommonMinuteCronSettings = new List<CommonCronSetting>
                                           {
                                               new CommonCronSetting(Resources.EveryMinute, "*", "", "", "", ""),
                                               new CommonCronSetting(Resources.EveryOtherMinute, "*/2", "", "", "", ""),
                                               new CommonCronSetting(Resources.Every5Minutes, "*/5", "", "", "", ""),
                                               new CommonCronSetting(Resources.Every10Minutes, "*/10", "", "", "", ""),
                                               new CommonCronSetting(Resources.Every15Minutes, "*/15", "", "", "", ""),
                                               new CommonCronSetting(Resources.Every30Minutes, "0,30", "", "", "", "")
                                           };
            for (var i = 0; i < 59; i++)
            {
                if (i == 0) CommonMinuteCronSettings.Add(new CommonCronSetting(Resources.TopOfTheHour, "0", "", "", "", ""));
                else if (i == 15) CommonMinuteCronSettings.Add(new CommonCronSetting(Resources.QuarterPast, "15", "", "", "", ""));
                else if (i == 30) CommonMinuteCronSettings.Add(new CommonCronSetting(Resources.HalfPast, "30", "", "", "", ""));
                else if (i == 15) CommonMinuteCronSettings.Add(new CommonCronSetting(Resources.QuarterTil, "45", "", "", "", ""));
                else CommonMinuteCronSettings.Add(new CommonCronSetting(i.ToString("00"), i.ToString(), "", "", "", ""));
            }


            CommonHourCronSettings = new List<CommonCronSetting>
                                         {
                                             new CommonCronSetting(Resources.EveryHour, "", "*", "", "", ""),
                                             new CommonCronSetting(Resources.EveryOtherHour, "", "*/2", "", "", ""),
                                             new CommonCronSetting(Resources.Every3Hours, "", "*/3", "", "", ""),
                                             new CommonCronSetting(Resources.Every4Hours, "", "*/4", "", "", ""),
                                             new CommonCronSetting(Resources.Every6Hours, "", "*/6", "", "", ""),
                                             new CommonCronSetting(Resources.Every12Hours, "", "0,12", "", "", ""),
                                             new CommonCronSetting(Resources.TwelveAmMidnight, "", "0", "", "", "")
                                         };
            for (int i = 1; i < 24; i++)
            {
                if (i == 12) CommonHourCronSettings.Add(new CommonCronSetting(string.Format("{0} {1} {2}", i, Resources.PM, Resources.Noon), "", i.ToString(), "", "", ""));
                else if (i > 12) CommonHourCronSettings.Add(new CommonCronSetting(string.Format("{0} {1}", i, Resources.PM), "", i.ToString(), "", "", ""));
                else CommonHourCronSettings.Add(new CommonCronSetting(string.Format("{0} {1}", i, Resources.AM), "", i.ToString(), "", "", ""));
            }

            CommonDayCronSettings = new List<CommonCronSetting>
                                        {
                                            new CommonCronSetting(Resources.EveryDay, "", "", "*", "", ""),
                                            new CommonCronSetting(Resources.EveryOtherDay, "", "", "*/2", "", ""),
                                            new CommonCronSetting(Resources.FirstAndFifteenth, "", "", "1,15", "", "")
                                        };
            for (int i = 1; i < 32; i++)
            {
                if (i == 1 || i == 21 || i == 31) CommonDayCronSettings.Add(new CommonCronSetting(i + "st", "", "", i.ToString(), "", ""));
                else if (i == 2 || i == 22) CommonDayCronSettings.Add(new CommonCronSetting(i + "nd", "", "", i.ToString(), "", ""));
                else if (i == 3 || i == 23) CommonDayCronSettings.Add(new CommonCronSetting(i + "rd", "", "", i.ToString(), "", ""));
                else CommonDayCronSettings.Add(new CommonCronSetting(i + "th", "", "", i.ToString(), "", ""));
            }

            CommonMonthCronSettings = new List<CommonCronSetting>
                                          {
                                              new CommonCronSetting(Resources.EveryMonth, "", "", "", "*", ""),
                                              new CommonCronSetting(Resources.EveryOtherMonth, "", "", "", "*/2", ""),
                                              new CommonCronSetting(Resources.Every3Months, "", "", "", "*/4", ""),
                                              new CommonCronSetting(Resources.Every6Months, "", "", "", "1,7", "")
                                          };

            for (int i = 0; i < 12; i++)
            {
                CommonMonthCronSettings.Add(new CommonCronSetting(CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[i], "", "", "", (i + 1).ToString(), ""));
            }

            CommonWeekdayCronSettings = new List<CommonCronSetting>
                                            {
                                                new CommonCronSetting(Resources.EveryWeekday, "", "", "", "", "*"),
                                                new CommonCronSetting(Resources.MonThruFri, "", "", "", "", "1-5"),
                                                new CommonCronSetting(Resources.SatAndSun, "", "", "", "", "6,0"),
                                                new CommonCronSetting(Resources.MonWedFri, "", "", "", "", "1,3,5"),
                                                new CommonCronSetting(Resources.TuesAndThurs, "", "", "", "", "2,4")
                                            };

            for (var i = 0; i < 7; i++)
            {
                CommonWeekdayCronSettings.Add(new CommonCronSetting(CultureInfo.CurrentCulture.DateTimeFormat.DayNames[i], "", "", "", "", i.ToString()));
            }
        }

        public string Expression
        {
            get { return Model.Expression; }
            set
            {
                Model.Expression = value;
                RaisePropertyChanged(() => Expression);
            }
        }

        public ICaptionCommand TestExpressionCommand { get; set; }

        public List<CommonCronSetting> CommonCronSettings { get; set; }
        public List<CommonCronSetting> CommonMinuteCronSettings { get; set; }
        public List<CommonCronSetting> CommonHourCronSettings { get; set; }
        public List<CommonCronSetting> CommonDayCronSettings { get; set; }
        public List<CommonCronSetting> CommonMonthCronSettings { get; set; }
        public List<CommonCronSetting> CommonWeekdayCronSettings { get; set; }

        private CommonCronSetting _selectedCronSetting;
        public CommonCronSetting SelectedCronSetting
        {
            get { return _selectedCronSetting; }
            set { _selectedCronSetting = value; UpdateCronSetting(value); }
        }

        private CommonCronSetting _selectedMinuteCronSetting;
        public CommonCronSetting SelectedMinuteCronSetting
        {
            get { return _selectedMinuteCronSetting; }
            set { _selectedMinuteCronSetting = value; UpdateCronSetting(value); RaisePropertyChanged(() => SelectedMinuteCronSetting); }
        }

        private CommonCronSetting _selectedHourCronSetting;
        public CommonCronSetting SelectedHourCronSetting
        {
            get { return _selectedHourCronSetting; }
            set { _selectedHourCronSetting = value; UpdateCronSetting(value); RaisePropertyChanged(() => SelectedHourCronSetting); }
        }

        private CommonCronSetting _selectedDayCronSetting;
        public CommonCronSetting SelectedDayCronSetting
        {
            get { return _selectedDayCronSetting; }
            set { _selectedDayCronSetting = value; UpdateCronSetting(value); RaisePropertyChanged(() => SelectedDayCronSetting); }
        }

        private CommonCronSetting _selectedMonthCronSetting;
        public CommonCronSetting SelectedMonthCronSetting
        {
            get { return _selectedMonthCronSetting; }
            set { _selectedMonthCronSetting = value; UpdateCronSetting(value); RaisePropertyChanged(() => SelectedMonthCronSetting); }
        }

        private CommonCronSetting _selectedWeekdayCronSetting;
        public CommonCronSetting SelectedWeekdayCronSetting
        {
            get { return _selectedWeekdayCronSetting; }
            set { _selectedWeekdayCronSetting = value; UpdateCronSetting(value); RaisePropertyChanged(() => SelectedWeekdayCronSetting); }
        }

        private string _minute;
        public string Minute
        {
            get { return _minute; }
            set { _minute = value; RaisePropertyChanged(() => Minute); UpdateExpression(); }
        }

        private string _hour;
        public string Hour
        {
            get { return _hour; }
            set { _hour = value; RaisePropertyChanged(() => Hour); UpdateExpression(); }
        }

        private string _day;
        public string Day
        {
            get { return _day; }
            set { _day = value; RaisePropertyChanged(() => Day); UpdateExpression(); }
        }

        private string _month;
        public string Month
        {
            get { return _month; }
            set { _month = value; RaisePropertyChanged(() => Month); UpdateExpression(); }
        }

        private string _weekday;
        public string Weekday
        {
            get { return _weekday; }
            set { _weekday = value; RaisePropertyChanged(() => Weekday); UpdateExpression(); }
        }

        public DateTime LastTrigger { get { return Model.LastTrigger; } set { Model.LastTrigger = value; } }

        public void UpdateCronSetting(CommonCronSetting cronSetting)
        {
            if (!string.IsNullOrEmpty(cronSetting.Minute)) Minute = cronSetting.Minute;
            if (!string.IsNullOrEmpty(cronSetting.Hour)) Hour = cronSetting.Hour;
            if (!string.IsNullOrEmpty(cronSetting.Day)) Day = cronSetting.Day;
            if (!string.IsNullOrEmpty(cronSetting.Month)) Month = cronSetting.Month;
            if (!string.IsNullOrEmpty(cronSetting.Weekday)) Weekday = cronSetting.Weekday;
        }

        public void UpdateExpression()
        {
            _selectedMinuteCronSetting = CommonMinuteCronSettings.FirstOrDefault(x => x.Minute == Minute);
            _selectedHourCronSetting = CommonHourCronSettings.FirstOrDefault(x => x.Hour == Hour);
            _selectedDayCronSetting = CommonDayCronSettings.FirstOrDefault(x => x.Day == Day);
            _selectedMonthCronSetting = CommonMonthCronSettings.FirstOrDefault(x => x.Month == Month);
            _selectedWeekdayCronSetting = CommonWeekdayCronSettings.FirstOrDefault(x => x.Weekday == Weekday);
            RaisePropertyChanged(() => SelectedMinuteCronSetting);
            RaisePropertyChanged(() => SelectedHourCronSetting);
            RaisePropertyChanged(() => SelectedDayCronSetting);
            RaisePropertyChanged(() => SelectedMonthCronSetting);
            RaisePropertyChanged(() => SelectedWeekdayCronSetting);
            Expression = string.Format("{0} {1} {2} {3} {4}", Minute, Hour, Day, Month, Weekday);
            _selectedCronSetting = CommonCronSettings.FirstOrDefault(x => x.Expression == Expression);
            RaisePropertyChanged(() => SelectedCronSetting);
        }

        public override Type GetViewType()
        {
            return typeof(TriggerView);
        }

        public override string GetModelTypeString()
        {
            return Resources.Trigger;
        }

        protected override void OnSave(string value)
        {
            LastTrigger = DateTime.Now;
            base.OnSave(value);
            _methodQueue.Queue("UpdateCronObjects", _triggerService.UpdateCronObjects);
        }

        protected override string GetSaveErrorMessage()
        {
            var nextTime = GetNextDateTime();
            if (nextTime == null)
                return Resources.ErrorInExpression + "!";
            return base.GetSaveErrorMessage();
        }

        protected override void Initialize()
        {
            GenerateCommonSettings();

            if (!string.IsNullOrEmpty(Model.Expression))
            {
                var parts = Model.Expression.Split(' ');
                if (parts.Length == 5)
                {
                    Minute = parts[0];
                    Hour = parts[1];
                    Day = parts[2];
                    Month = parts[3];
                    Weekday = parts[4];
                }
            }

            base.Initialize();
        }
    }
}
