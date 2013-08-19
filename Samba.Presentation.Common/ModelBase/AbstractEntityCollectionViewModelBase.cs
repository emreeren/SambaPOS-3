using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Samba.Localization.Properties;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Services.Common;

namespace Samba.Presentation.Common.ModelBase
{
    public abstract class AbstractEntityCollectionViewModelBase : VisibleViewModelBase
    {
        public ICaptionCommand AddItemCommand { get; set; }
        public ICaptionCommand EditItemCommand { get; set; }
        public ICaptionCommand DeleteItemCommand { get; set; }
        public ICaptionCommand DuplicateItemCommand { get; set; }
        public ICaptionCommand DeleteSelectedItemsCommand { get; set; }

        public IList<ICaptionCommand> CustomCommands { get; set; }

        private string _modelTitle;
        public string ModelTitle { get { return _modelTitle ?? (_modelTitle = GetModelTitle()); } }

        private string _pluralModelTitle;
        public string PluralModelTitle { get { return _pluralModelTitle ?? (_pluralModelTitle = ModelTitle.ToPlural()); } }

        private IList<ICaptionCommand> _allCommands;
        public IList<ICaptionCommand> AllCommands
        {
            get { return _allCommands ?? (_allCommands = GetCommands().ToList()); }
        }

        private IEnumerable<ICaptionCommand> GetCommands()
        {
            var result = new List<ICaptionCommand> { AddItemCommand, EditItemCommand, DeleteItemCommand };
            result.AddRange(CustomCommands);
            return result;
        }

        public void RefreshCommands()
        {
            _allCommands = null;
            RaisePropertyChanged(() => AllCommands);
        }

        public void RemoveCommand(ICaptionCommand command)
        {
            if (AllCommands.Contains(command))
            {
                AllCommands.Remove(command);
                RaisePropertyChanged(() => AllCommands);
            }
        }

        public void InsertCommand(ICaptionCommand command, int index = -1)
        {
            if (index > -1)
            {
                AllCommands.Insert(index, command);
            }
            else AllCommands.Add(command);
        }

        protected AbstractEntityCollectionViewModelBase()
        {
            AddItemCommand = new CaptionCommand<object>(string.Format(Resources.Add_f, ModelTitle), OnAddItem, CanAddItem);
            EditItemCommand = new CaptionCommand<object>(string.Format(Resources.Edit_f, ModelTitle), OnEditItem, CanEditItem);
            DeleteItemCommand = new CaptionCommand<object>(string.Format(Resources.Delete_f, ModelTitle), OnDeleteItem, CanDeleteItem);
            DuplicateItemCommand = new CaptionCommand<object>(string.Format(Resources.Clone_f, ModelTitle), OnDuplicateItem, CanDuplicateItem);
            DeleteSelectedItemsCommand = new CaptionCommand<IEnumerable>(Resources.DeleteSelectedItems, OnDeleteSelectedItems, CanDeleteSelectedItems);

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
        protected abstract void OnDeleteSelectedItems(IEnumerable obj);
        protected abstract bool CanDeleteSelectedItems(IEnumerable arg);


        protected override string GetHeaderInfo()
        {
            return string.Format(Resources.List_f, ModelTitle);
        }

        public override Type GetViewType()
        {
            return typeof(EntityCollectionBaseView);
        }

        public abstract void RefreshItems();
    }
}
