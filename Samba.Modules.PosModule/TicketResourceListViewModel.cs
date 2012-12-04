using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.Practices.Prism.Commands;
using Samba.Domain.Models.Resources;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Services.Common;
using Samba.Services;

namespace Samba.Modules.PosModule
{
    [Export]
    public class TicketResourceListViewModel : ObservableObject
    {
        private readonly ICacheService _cacheService;
        public DelegateCommand<Resource> SelectionCommand { get; set; }
        public CaptionCommand<string> CloseCommand { get; set; }

        [ImportingConstructor]
        public TicketResourceListViewModel(ICacheService cacheService)
        {
            _cacheService = cacheService;
            SelectionCommand = new DelegateCommand<Resource>(OnSelectResource);
            CloseCommand = new CaptionCommand<string>(Resources.Close, OnClose);
            ResourceList = new ObservableCollection<Resource>();
        }

        private void OnClose(string obj)
        {
            EventServiceFactory.EventService.PublishEvent(EventTopicNames.CloseTicketRequested);
        }

        private void OnSelectResource(Resource obj)
        {
            new EntityOperationRequest<Resource>(obj, null).PublishEvent(EventTopicNames.ResourceSelected, true);
        }

        public ObservableCollection<Resource> ResourceList { get; set; }

        public void Update(Ticket selectedTicket)
        {
            ResourceList.Clear();
            var rt = _cacheService.GetTicketTypeById(selectedTicket.TicketTypeId).ResourceTypeAssignments.First(
                    x => x.AskBeforeCreatingTicket && !selectedTicket.TicketResources.Any(y => y.ResourceTypeId == x.ResourceTypeId));
            ResourceList.AddRange(_cacheService.GetResources(rt.ResourceTypeId, rt.State));
            RaisePropertyChanged(() => RowCount);
            RaisePropertyChanged(() => ColumnCount);
        }

        public int ColumnCount { get { return ResourceList.Count % 7 == 0 ? ResourceList.Count / 7 : (ResourceList.Count / 7) + 1; } }
        public int RowCount { get { return 7; } }
    }
}
