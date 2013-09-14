using System;
using System.Collections.Generic;
using System.Linq;
using Samba.Domain.Models.Settings;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.SettingsModule
{
    class StateViewModel : EntityViewModelBase<State>
    {
        private readonly string[] _stateTypes = new[]
                                                    {
                                                        string.Format(Resources.State_f, Resources.Entity),
                                                        string.Format(Resources.State_f, Resources.Ticket),
                                                        string.Format(Resources.State_f, Resources.Order)
                                                    };
        public string[] StateTypes
        {
            get { return _stateTypes; }
        }

        public IEnumerable<string> GroupNames { get { return Dao.Distinct<State>(x => x.GroupName); } }

        public string Color { get { return Model.Color; } set { Model.Color = value; } }
        public string StateType { get { return StateTypes[Model.StateType]; } set { Model.StateType = StateTypes.ToList().IndexOf(value); } }
        public string GroupName { get { return Model.GroupName; } set { Model.GroupName = value; } }
        public bool ShowOnEndOfDayReport { get { return Model.ShowOnEndOfDayReport; } set { Model.ShowOnEndOfDayReport = value; } }
        public bool ShowOnProductReport { get { return Model.ShowOnProductReport; } set { Model.ShowOnProductReport = value; } }
        public bool ShowOnTicket { get { return Model.ShowOnTicket; } set { Model.ShowOnTicket = value; } }

        public override Type GetViewType()
        {
            return typeof(StateView);
        }

        public override string GetModelTypeString()
        {
            return Resources.State;
        }
    }
}
