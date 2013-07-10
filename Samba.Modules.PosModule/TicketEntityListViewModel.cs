using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.Practices.Prism.Commands;
using Samba.Domain.Models.Entities;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Services.Common;
using Samba.Services;

namespace Samba.Modules.PosModule
{
    [Export]
    public class TicketEntityListViewModel : ObservableObject
    {
        private readonly ICacheService _cacheService;
        public DelegateCommand<Entity> SelectionCommand { get; set; }
        public CaptionCommand<string> CloseCommand { get; set; }

        [ImportingConstructor]
        public TicketEntityListViewModel(ICacheService cacheService)
        {
            _cacheService = cacheService;
            SelectionCommand = new DelegateCommand<Entity>(OnSelectEntity);
            CloseCommand = new CaptionCommand<string>(Resources.Close, OnClose);
            EntityList = new ObservableCollection<Entity>();
        }

        private void OnClose(string obj)
        {
            EventServiceFactory.EventService.PublishEvent(EventTopicNames.CloseTicketRequested);
        }

        private void OnSelectEntity(Entity obj)
        {
            new OperationRequest<Entity>(obj, null).PublishEvent(EventTopicNames.EntitySelected, true);
        }

        public ObservableCollection<Entity> EntityList { get; set; }

        public void Update(Ticket selectedTicket)
        {
            EntityList.Clear();
            var rt = _cacheService.GetTicketTypeById(selectedTicket.TicketTypeId).EntityTypeAssignments.First(
                    x => x.AskBeforeCreatingTicket && selectedTicket.TicketEntities.All(y => y.EntityTypeId != x.EntityTypeId));
            EntityList.AddRange(_cacheService.GetEntities(rt.EntityTypeId, rt.State));
            RaisePropertyChanged(() => RowCount);
            RaisePropertyChanged(() => ColumnCount);
        }

        public int ColumnCount { get { return EntityList.Count % 7 == 0 ? EntityList.Count / 7 : (EntityList.Count / 7) + 1; } }
        public int RowCount { get { return 7; } }
    }
}
