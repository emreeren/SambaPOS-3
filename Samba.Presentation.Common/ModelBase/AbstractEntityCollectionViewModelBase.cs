using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Samba.Localization.Properties;

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
        public string PluralModelTitle { get { return _pluralModelTitle ?? (_pluralModelTitle = Pluralize(ModelTitle)); } }

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
            DeleteSelectedItemsCommand = new CaptionCommand<IEnumerable>("Delete Selected Items", OnDeleteSelectedItems, CanDeleteSelectedItems);

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

        private static readonly IList<string> Unpluralizables = new List<string> { "equipment", "information", "rice", "money", "species", "series", "fish", "sheep", "deer" };
        private static readonly IDictionary<string, string> Pluralizations = new Dictionary<string, string>
        {
            // Start with the rarest cases, and move to the most common
            { "person$", "people" },
            { "ox$", "oxen" },
            { "child$", "children" },
            { "foot$", "feet" },
            { "tooth$", "teeth" },
            { "goose$", "geese" },
            // And now the more standard rules.
            { "(.*)fe?", "$1ves" },         // ie, wolf, wife
            { "(.*)man$", "$1men" },
            { "(.+[aeiou]y)$", "$1s" },
            { "(.+[^aeiou])y$", "$1ies" },
            { "(.+z)$", "$1zes" },
            { "([m|l])ouse$", "$1ice" },
            { "(.+)(e|i)x$", @"$1ices"},    // ie, Matrix, Index
            { "(octop|vir)us$", "$1i"},
            { "(.+(s|x|sh|ch))$", @"$1es"},
            { "(.+)", @"$1s" }
        };

        public static string Pluralize(string singular)
        {
            if (Unpluralizables.Contains(singular))
                return singular;

            var plural = "";

            foreach (var pluralization in Pluralizations)
            {
                if (Regex.IsMatch(singular, pluralization.Key))
                {
                    plural = Regex.Replace(singular, pluralization.Key, pluralization.Value);
                    break;
                }
            }

            return plural;
        }
    }
}
