using System.Linq;
using System.Windows;
using FluentValidation;
using Samba.Infrastructure.Data;
using Samba.Localization.Properties;

namespace Samba.Presentation.Common.ModelBase
{
    public abstract class EntityViewModelBase<TModel> : VisibleViewModelBase where TModel : class, IEntity
    {
        private bool _modelSaved;

        public TModel Model { get; private set; }
        public ICaptionCommand SaveCommand { get; private set; }
        private IValidator<TModel> _validator;

        protected IWorkspace Workspace { get; private set; }

        protected EntityViewModelBase(TModel model)
        {
            Model = model;
            SaveCommand = new CaptionCommand<string>(Resources.Save, OnSave, CanSave);
        }

        public string Name
        {
            get { return Model.Name; }
            set
            {
                Model.Name = value.Trim();
                RaisePropertyChanged(() => Name);
            }
        }

        private string _error;
        public string Error
        {
            get { return _error; }
            set
            {
                _error = value;
                RaisePropertyChanged(() => Error);
            }
        }

        public abstract string GetModelTypeString();

        public void Init(IWorkspace workspace)
        {
            _modelSaved = false;
            Workspace = workspace;
            Initialize();
        }

        protected virtual void Initialize()
        {
            // override to initialize
        }

        public override void OnShown()
        {
            _modelSaved = false;
        }

        public override void OnClosed()
        {
            if (!_modelSaved)
                RollbackModel();
        }

        protected override string GetHeaderInfo()
        {
            if (Model.Id > 0)
                return string.Format(Resources.EditModel_f, GetModelTypeString(), Name);
            return string.Format(Resources.AddModel_f, GetModelTypeString());
        }

        protected virtual void OnSave(string value)
        {
            var errorMessage = GetSaveErrorMessage();
            if (CanSave())
            {
                _modelSaved = true;
                if (Model.Id == 0)
                {
                    this.PublishEvent(EventTopicNames.AddedModelSaved);
                }
                this.PublishEvent(EventTopicNames.ModelAddedOrDeleted);
                ((VisibleViewModelBase)this).PublishEvent(EventTopicNames.ViewClosed);
            }
            else
            {
                if (string.IsNullOrEmpty(Name))
                    errorMessage = string.Format(Resources.EmptyNameError, GetModelTypeString());
                MessageBox.Show(errorMessage, Resources.CantSave);
            }
        }

        public bool CanSave()
        {
            return !string.IsNullOrEmpty(Name) && string.IsNullOrEmpty(GetSaveErrorMessage()) && CanSave("");
        }

        protected virtual string GetSaveErrorMessage()
        {
            return "";
        }

        protected virtual bool CanSave(string arg)
        {
            return Validate();
        }

        private bool Validate()
        {
            var validator = _validator ?? (_validator = GetValidator());
            var vs = validator.Validate(Model);
            if (!vs.IsValid)
            {
                Error = string.Join("\r", vs.Errors.Select(x => x.ErrorMessage));
                return false;
            }
            Error = "";
            return true;
        }

        protected virtual AbstractValidator<TModel> GetValidator()
        {
            return new EntityValidator<TModel>();
        }

        public void RollbackModel()
        {
            if (Model.Id > 0)
            {
                Workspace.Refresh(Model);
                RaisePropertyChanged(() => Name);
            }
        }
    }

    public class EntityValidator<TModel> : AbstractValidator<TModel> where TModel : IEntity
    {
        public EntityValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
        }
    }
}
