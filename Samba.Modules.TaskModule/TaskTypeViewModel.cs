using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Tasks;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.TaskModule
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class TaskTypeViewModel : EntityViewModelBase<TaskType>
    {
        public ICaptionCommand AddCustomFieldCommand { get; set; }
        public ICaptionCommand DeleteCustomFieldCommand { get; set; }

        [ImportingConstructor]
        public TaskTypeViewModel()
        {
            AddCustomFieldCommand = new CaptionCommand<string>(Resources.Add, OnAddCustomField);
            DeleteCustomFieldCommand = new CaptionCommand<string>(Resources.Delete, OnDeleteCustomField, CanDeleteCustomField);
        }

        private ObservableCollection<TaskCustomFieldViewModel> _taskCustomFields;
        public ObservableCollection<TaskCustomFieldViewModel> TaskCustomFields
        {
            get { return _taskCustomFields ?? (_taskCustomFields = new ObservableCollection<TaskCustomFieldViewModel>(Model.TaskCustomFields.Select(x => new TaskCustomFieldViewModel(x)))); }
            set { _taskCustomFields = value; }
        }

        public TaskCustomFieldViewModel SelectedCustomField { get; set; }

        public override Type GetViewType()
        {
            return typeof(TaskTypeView);
        }

        public override string GetModelTypeString()
        {
            return Resources.TaskType;
        }

        private void OnDeleteCustomField(string obj)
        {
            if (SelectedCustomField.Model.Id > 0)
                Workspace.Delete(SelectedCustomField.Model);
            Model.TaskCustomFields.Remove(SelectedCustomField.Model);
            _taskCustomFields = null;
            RaisePropertyChanged(() => TaskCustomFields);
        }

        private bool CanDeleteCustomField(string arg)
        {
            return SelectedCustomField != null;
        }

        private void OnAddCustomField(string obj)
        {
            var cf = new TaskCustomField();
            Model.TaskCustomFields.Add(cf);
            _taskCustomFields = null;
            RaisePropertyChanged(() => TaskCustomFields);
        }
    }

    public class TaskCustomFieldViewModel : ObservableObject
    {
        private readonly TaskCustomField _model;

        public TaskCustomFieldViewModel(TaskCustomField taskCustomField)
        {
            _model = taskCustomField;
        }

        public List<string> FieldTypes { get { return new List<string>(new[] { "String", "Number" }); } }

        public string Name { get { return _model.Name; } set { _model.Name = value; } }
        public string FieldType { get { return FieldTypes[_model.FieldType]; } set { _model.FieldType = FieldTypes.IndexOf(value); } }
        public string EditingFormat { get { return _model.EditingFormat; } set { _model.EditingFormat = value; } }
        public string DisplayFormat { get { return _model.DisplayFormat; } set { _model.DisplayFormat = value; } }

        public TaskCustomField Model
        {
            get { return _model; }
        }
    }
}
