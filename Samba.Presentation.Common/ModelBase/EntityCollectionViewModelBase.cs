using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Linq;
using Microsoft.Practices.Prism.Events;
using Samba.Infrastructure.Data;
using Samba.Infrastructure.Data.Serializer;
using Samba.Localization.Properties;
using Samba.Persistance.Data;

namespace Samba.Presentation.Common.ModelBase
{
    public abstract class EntityCollectionViewModelBase<TViewModel, TModel> : AbstractEntityCollectionViewModelBase, ICountable
        where TViewModel : EntityViewModelBase<TModel>
        where TModel : class, IEntity, new()
    {
        private readonly IWorkspace _workspace = WorkspaceFactory.Create();

        public IWorkspace Workspace { get { return _workspace; } }

        private ObservableCollection<TViewModel> _items;
        public ObservableCollection<TViewModel> Items { get { return _items ?? (_items = GetItemsList()); } }

        protected virtual ObservableCollection<TViewModel> GetItemsList()
        {
            return BuildViewModelList(SelectItems());
        }

        protected virtual IEnumerable<TModel> SelectItems()
        {
            return _workspace.All<TModel>();
        }

        protected virtual void BeforeDeleteItem(TModel item)
        {
            // override if needed.
        }

        private void DoDeleteItem(TModel item)
        {
            BeforeDeleteItem(item);
            _workspace.Delete(item);
            _workspace.CommitChanges();
        }

        protected abstract TViewModel CreateNewViewModel(TModel model);
        protected abstract TModel CreateNewModel();
        private readonly SubscriptionToken _token;
        private readonly SubscriptionToken _token2;
        public IList<EntityViewModelBase<TModel>> OpenViewModels { get; set; }

        protected EntityCollectionViewModelBase()
        {
            OpenViewModels = new List<EntityViewModelBase<TModel>>();

            _token = EventServiceFactory.EventService.GetEvent<GenericEvent<EntityViewModelBase<TModel>>>().Subscribe(x =>
                 {
                     if (x.Topic == EventTopicNames.AddedModelSaved)
                         if (x.Value is TViewModel)
                             Items.Add(x.Value as TViewModel);

                     if (x.Topic == EventTopicNames.ModelAddedOrDeleted)
                     {
                         foreach (var openViewModel in OpenViewModels)
                         {
                             if (!openViewModel.CanSave())
                                 openViewModel.RollbackModel();
                         }

                         if (x.Value is TViewModel)
                         {
                             _workspace.Update(x.Value.Model);
                             _workspace.CommitChanges();
                             _workspace.Refresh(x.Value.Model);
                         }
                     }
                 });

            _token2 = EventServiceFactory.EventService.GetEvent<GenericEvent<VisibleViewModelBase>>().Subscribe(
                  s =>
                  {
                      if (s.Topic == EventTopicNames.ViewClosed)
                      {
                          if (s.Value is EntityViewModelBase<TModel> && OpenViewModels.Contains(s.Value))
                              OpenViewModels.Remove(s.Value as EntityViewModelBase<TModel>);
                      }
                  });
        }

        private TViewModel _selectedItem;
        public TViewModel SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                RaisePropertyChanged(() => SelectedItem);
            }
        }

        public override string GetModelTitle()
        {
            return CreateNewViewModel(new TModel()).GetModelTypeString();
        }

        protected virtual string CanDeleteItem(TModel model)
        {
            return "";
        }

        protected override bool CanAddItem(object obj)
        {
            return true;
        }

        protected override bool CanClose(object arg)
        {
            return OpenViewModels.Count == 0;
        }

        protected override void OnDeleteItem(object obj)
        {
            if (MessageBox.Show(string.Format(Resources.DeleteItemConfirmation_f, ModelTitle, SelectedItem.Model.Name), Resources.Confirmation, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                var errorMessage = CanDeleteItem(SelectedItem.Model);
                if (string.IsNullOrEmpty(errorMessage))
                {
                    if (SelectedItem.Model.Id > 0)
                    {
                        DoDeleteItem(SelectedItem.Model);
                        SelectedItem.Model.PublishEvent(EventTopicNames.ModelAddedOrDeleted);
                    }
                    Items.Remove(SelectedItem);
                }
                else
                {
                    MessageBox.Show(errorMessage, Resources.Warning);
                }
            }
        }

        protected override void OnAddItem(object obj)
        {
            VisibleViewModelBase wm = InternalCreateNewViewModel(CreateNewModel());
            if (wm is EntityViewModelBase<TModel>)
                OpenViewModels.Add(wm as EntityViewModelBase<TModel>);
            wm.PublishEvent(EventTopicNames.ViewAdded);
        }

        protected override void OnDuplicateItem(object obj)
        {
            var duplicate = ObjectCloner.Clone(SelectedItem.Model);
            duplicate.Id = 0;
            duplicate.Name = "_" + duplicate.Name;
            VisibleViewModelBase wm = InternalCreateNewViewModel(duplicate);
            if (wm is EntityViewModelBase<TModel>)
                OpenViewModels.Add(wm as EntityViewModelBase<TModel>);
            wm.PublishEvent(EventTopicNames.ViewAdded);
        }

        protected override bool CanDuplicateItem(object arg)
        {
            return SelectedItem != null;
        }

        protected override bool CanEditItem(object obj)
        {
            return SelectedItem != null;
        }

        protected override bool CanDeleteItem(object obj)
        {
            return SelectedItem != null && !OpenViewModels.Contains(SelectedItem);
        }

        protected override void OnEditItem(object obj)
        {
            if (!OpenViewModels.Contains(SelectedItem)) OpenViewModels.Add(SelectedItem);
            (SelectedItem as VisibleViewModelBase).PublishEvent(EventTopicNames.ViewAdded);
        }

        protected ObservableCollection<TViewModel> BuildViewModelList(IEnumerable<TModel> itemsList)
        {
            return new ObservableCollection<TViewModel>(itemsList.Select(InternalCreateNewViewModel));
        }

        protected TViewModel InternalCreateNewViewModel(TModel model)
        {
            var result = CreateNewViewModel(model);
            result.Init(_workspace);
            return result;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_workspace != null)
                    _workspace.Dispose();
            }
            EventServiceFactory.EventService.GetEvent<GenericEvent<EntityViewModelBase<TModel>>>().Unsubscribe(_token);
            EventServiceFactory.EventService.GetEvent<GenericEvent<VisibleViewModelBase>>().Unsubscribe(_token2);
        }

        public int GetCount()
        {
            return Items.Count;
        }
    }
}
