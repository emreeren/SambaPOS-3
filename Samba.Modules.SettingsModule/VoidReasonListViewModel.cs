using System;
using System.Linq;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Common.Services;
using Samba.Services;

namespace Samba.Modules.SettingsModule
{
    public class VoidReasonListViewModel : EntityCollectionViewModelBase<VoidReasonViewModel, Reason>
    {
        public ICaptionCommand CreateBatchVoidReasons { get; set; }

        public VoidReasonListViewModel()
        {
            CreateBatchVoidReasons = new CaptionCommand<string>(Resources.BatchAddVoidReasons, OnCreateBatchVoidReasons);
            CustomCommands.Add(CreateBatchVoidReasons);
        }

        private void OnCreateBatchVoidReasons(string obj)
        {
            var values = InteractionService.UserIntraction.GetStringFromUser(
                Resources.BatchAddVoidReasons,
                Resources.BatchAddVoidReasonsDialogHint);

            var createdItems = new DataCreationService().BatchCreateReasons(values, 0, Workspace);
            Workspace.CommitChanges();

            foreach (var mv in createdItems.Select(CreateNewViewModel))
            {
                mv.Init(Workspace);
                Items.Add(mv);
            }
        }

        protected override VoidReasonViewModel CreateNewViewModel(Reason model)
        {
            return new VoidReasonViewModel(model);
        }

        protected override Reason CreateNewModel()
        {
            return new Reason { ReasonType = 0 };
        }

        protected override System.Collections.ObjectModel.ObservableCollection<VoidReasonViewModel> GetItemsList()
        {
            return BuildViewModelList(Workspace.All<Reason>(x => x.ReasonType == 0));
        }

        protected override string CanDeleteItem(Reason model)
        {
            var voids = Dao.Count<Order>(x => x.Voided && x.ReasonId == model.Id);
            return voids > 0 ? Resources.DeleteErrorVoidReasonUsed : "";
        }
    }
}
