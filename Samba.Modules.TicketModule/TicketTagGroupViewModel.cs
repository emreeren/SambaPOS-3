using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.ViewModels;
using Samba.Services;

namespace Samba.Modules.TicketModule
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class TicketTagGroupViewModel : EntityViewModelBase<TicketTagGroup>
    {
        private readonly IUserService _userService;
        private readonly IDepartmentService _departmentService;
        private readonly ISettingService _settingService;
        private readonly IList<string> _tagTypes = new[] { Resources.Alphanumeric, Resources.Numeric, Resources.Price };
        public IList<string> DataTypes { get { return _tagTypes; } }

        private ObservableCollection<TicketTagViewModel> _ticketTags;
        public ObservableCollection<TicketTagViewModel> TicketTags { get { return _ticketTags ?? (_ticketTags = GetTicketTags(Model)); } }

        private ObservableCollection<TicketTagMapViewModel> _ticketTagMaps;
        public ObservableCollection<TicketTagMapViewModel> TicketTagMaps
        {
            get { return _ticketTagMaps ?? (_ticketTagMaps = new ObservableCollection<TicketTagMapViewModel>(Model.TicketTagMaps.Select(x => new TicketTagMapViewModel(x, _userService, _departmentService, _settingService)))); }
        }

        public TicketTagViewModel SelectedTicketTag { get; set; }
        public ICaptionCommand AddTicketTagCommand { get; set; }
        public ICaptionCommand DeleteTicketTagCommand { get; set; }

        public TicketTagMapViewModel SelectedTicketTagMap { get; set; }
        public CaptionCommand<string> DeleteTicketTagMapCommand { get; set; }
        public CaptionCommand<string> AddTicketTagMapCommand { get; set; }

        public string DataType { get { return DataTypes[Model.DataType]; } set { Model.DataType = DataTypes.IndexOf(value); } }

        public bool FreeTagging { get { return Model.FreeTagging; } set { Model.FreeTagging = value; } }
        public bool SaveFreeTags { get { return Model.SaveFreeTags; } set { Model.SaveFreeTags = value; } }
        public bool ForceValue { get { return Model.ForceValue; } set { Model.ForceValue = value; } }
        public string ButtonColorWhenTagSelected { get { return Model.ButtonColorWhenTagSelected; } set { Model.ButtonColorWhenTagSelected = value; } }
        public string ButtonColorWhenNoTagSelected { get { return Model.ButtonColorWhenNoTagSelected; } set { Model.ButtonColorWhenNoTagSelected = value; } }

        [ImportingConstructor]
        public TicketTagGroupViewModel(IUserService userService, IDepartmentService departmentService, ISettingService settingService)
        {
            _userService = userService;
            _departmentService = departmentService;
            _settingService = settingService;

            AddTicketTagCommand = new CaptionCommand<string>(string.Format(Resources.Add_f, Resources.Tag), OnAddTicketTagExecuted);
            DeleteTicketTagCommand = new CaptionCommand<string>(string.Format(Resources.Delete_f, Resources.Tag), OnDeleteTicketTagExecuted, CanDeleteTicketTag);
            AddTicketTagMapCommand = new CaptionCommand<string>(Resources.Add, OnAddTicketTagMap);
            DeleteTicketTagMapCommand = new CaptionCommand<string>(Resources.Delete, OnDeleteTicketTagMap, CanDeleteTicketTagMap);
        }

        private void OnDeleteTicketTagMap(string obj)
        {
            if (SelectedTicketTagMap.Id > 0)
                Workspace.Delete(SelectedTicketTagMap.Model);
            Model.TicketTagMaps.Remove(SelectedTicketTagMap.Model);
            TicketTagMaps.Remove(SelectedTicketTagMap);
        }

        private bool CanDeleteTicketTagMap(string arg)
        {
            return SelectedTicketTagMap != null;
        }

        private void OnAddTicketTagMap(string obj)
        {
            TicketTagMaps.Add(new TicketTagMapViewModel(Model.AddTicketTagMap(), _userService, _departmentService, _settingService));
        }

        private static ObservableCollection<TicketTagViewModel> GetTicketTags(TicketTagGroup ticketTagGroup)
        {
            return new ObservableCollection<TicketTagViewModel>(ticketTagGroup.TicketTags.Select(item => new TicketTagViewModel(item)));
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

        protected override void OnSave(string value)
        {
            if (TicketTagMaps.Count == 0)
                TicketTagMaps.Add(new TicketTagMapViewModel(Model.AddTicketTagMap(), _userService, _departmentService, _settingService));
            base.OnSave(value);
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
