using System;
using System.Collections.Generic;
using Samba.Localization.Properties;

namespace Samba.Presentation.Common.ModelBase
{
    public abstract class AbstractEntityCollectionViewModelBase : VisibleViewModelBase
    {
        public ICaptionCommand AddItemCommand { get; set; }
        public ICaptionCommand EditItemCommand { get; set; }
        public ICaptionCommand DeleteItemCommand { get; set; }
        public ICaptionCommand DuplicateItemCommand { get; set; }

        public IList<ICaptionCommand> CustomCommands { get; set; }

        public string ModelTitle { get { return GetModelTitle(); } }


        private IEnumerable<ICaptionCommand> _allCommands;
        public IEnumerable<ICaptionCommand> AllCommands
        {
            get { return _allCommands ?? (_allCommands = GetCommands()); }
        }

        private IEnumerable<ICaptionCommand> GetCommands()
        {
            var result = new List<ICaptionCommand> { AddItemCommand, EditItemCommand, DeleteItemCommand };
            result.AddRange(CustomCommands);
            return result;
        }

        protected AbstractEntityCollectionViewModelBase()
        {
            AddItemCommand = new CaptionCommand<object>(string.Format(Resources.Add_f, ModelTitle), OnAddItem, CanAddItem);
            EditItemCommand = new CaptionCommand<object>(string.Format(Resources.Edit_f, ModelTitle), OnEditItem, CanEditItem);
            DeleteItemCommand = new CaptionCommand<object>(string.Format(Resources.Delete_f, ModelTitle), OnDeleteItem, CanDeleteItem);
            DuplicateItemCommand = new CaptionCommand<object>(string.Format(Resources.Clone_f, ModelTitle), OnDuplicateItem, CanDuplicateItem);
            CustomCommands = new List<ICaptionCommand>();
        }

        public abstract string GetModelTitle();

        protected abstract void OnDeleteItem(object obj);
        protected abstract void OnAddItem(object obj);
        protected abstract void OnEditItem(object obj);
        protected abstract bool CanEditItem(object obj);
        protected abstract bool CanDeleteItem(object obj);
        protected abstract bool CanDuplicateItem(object arg);
        protected abstract bool CanAddItem(object obj);
        protected abstract void OnDuplicateItem(object obj);

        protected override string GetHeaderInfo()
        {
            return string.Format(Resources.List_f, ModelTitle);
        }

        public override Type GetViewType()
        {
            return typeof(EntityCollectionBaseView);
        }
    }
}
