using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.ViewModels;

namespace Samba.Modules.MenuModule
{
    public class TicketTagGroupViewModel : EntityViewModelBase<TicketTagGroup>
    {
        private readonly IList<string> _actions = new[] { Resources.Refresh, Resources.CloseTicket, Resources.GetPayment };
        public IList<string> Actions { get { return _actions; } }

        private readonly IList<string> _tagTypes = new[] { Resources.Alphanumeric, Resources.Numeric, Resources.Price };
        public IList<string> DataTypes { get { return _tagTypes; } }

        private IEnumerable<Numerator> _numerators;
        public IEnumerable<Numerator> Numerators
        {
            get { return _numerators ?? (_numerators = Workspace.All<Numerator>()); }
        }

        private readonly ObservableCollection<TicketTagViewModel> _ticketTags;
        public ObservableCollection<TicketTagViewModel> TicketTags { get { return _ticketTags; } }

        public TicketTagViewModel SelectedTicketTag { get; set; }
        public ICaptionCommand AddTicketTagCommand { get; set; }
        public ICaptionCommand DeleteTicketTagCommand { get; set; }

        public string Action { get { return Actions[Model.Action]; } set { Model.Action = Actions.IndexOf(value); } }
        public string DataType { get { return DataTypes[Model.DataType]; } set { Model.DataType = DataTypes.IndexOf(value); } }

        public Numerator Numerator { get { return Model.Numerator; } set { Model.Numerator = value; } }
        public bool FreeTagging { get { return Model.FreeTagging; } set { Model.FreeTagging = value; } }
        public bool SaveFreeTags { get { return Model.SaveFreeTags; } set { Model.SaveFreeTags = value; } }
        public bool ForceValue { get { return Model.ForceValue; } set { Model.ForceValue = value; } }
        public string ButtonColorWhenTagSelected { get { return Model.ButtonColorWhenTagSelected; } set { Model.ButtonColorWhenTagSelected = value; } }
        public string ButtonColorWhenNoTagSelected { get { return Model.ButtonColorWhenNoTagSelected; } set { Model.ButtonColorWhenNoTagSelected = value; } }
        public bool ActiveOnPosClient { get { return Model.ActiveOnPosClient; } set { Model.ActiveOnPosClient = value; } }
        public bool ActiveOnTerminalClient { get { return Model.ActiveOnTerminalClient; } set { Model.ActiveOnTerminalClient = value; } }

        public TicketTagGroupViewModel(TicketTagGroup model)
            : base(model)
        {
            _ticketTags = new ObservableCollection<TicketTagViewModel>(GetTicketTags(model));
            AddTicketTagCommand = new CaptionCommand<string>(string.Format(Resources.Add_f, Resources.Tag), OnAddTicketTagExecuted);
            DeleteTicketTagCommand = new CaptionCommand<string>(string.Format(Resources.Delete_f, Resources.Tag), OnDeleteTicketTagExecuted, CanDeleteTicketTag);
        }

        private static IEnumerable<TicketTagViewModel> GetTicketTags(TicketTagGroup ticketTagGroup)
        {
            return ticketTagGroup.TicketTags.Select(item => new TicketTagViewModel(item));
        }

        public override string GetModelTypeString()
        {
            return Resources.TicketTag;
        }

        public override Type GetViewType()
        {
            return typeof(TicketTagGroupView);
        }

        private bool CanDeleteTicketTag(string arg)
        {
            return SelectedTicketTag != null;
        }

        private void OnDeleteTicketTagExecuted(string obj)
        {
            if (SelectedTicketTag == null) return;
            if (SelectedTicketTag.Model.Id > 0)
                Workspace.Delete(SelectedTicketTag.Model);
            Model.TicketTags.Remove(SelectedTicketTag.Model);
            TicketTags.Remove(SelectedTicketTag);
        }

        private void OnAddTicketTagExecuted(string obj)
        {
            var ti = new TicketTag { Name = Resources.NewTag };
            Workspace.Add(ti);
            Model.TicketTags.Add(ti);
            TicketTags.Add(new TicketTagViewModel(ti));
        }

        protected override string GetSaveErrorMessage()
        {
            foreach (var ticketTag in TicketTags)
            {
                try
                {
                    var str = ticketTag.Model.Name;

                    if (Model.IsDecimal)
                        str = Convert.ToDecimal(ticketTag.Model.Name).ToString();

                    if (Model.IsInteger)
                        str = Convert.ToInt32(ticketTag.Model.Name).ToString();

                    if (str != ticketTag.Model.Name)
                        return Resources.NumericTagsShouldBeNumbersErrorMessage;
                }
                catch (FormatException)
                {
                    return Resources.NumericTagsShouldBeNumbersErrorMessage;
                }
                catch (OverflowException)
                {
                    return Resources.NumericTagsShouldBeNumbersErrorMessage;
                }
            }
            return base.GetSaveErrorMessage();
        }
    }
}
