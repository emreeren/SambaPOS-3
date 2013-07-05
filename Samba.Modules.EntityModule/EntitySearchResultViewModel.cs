using Samba.Domain.Models.Entities;
using Samba.Presentation.Common;
using Samba.Presentation.ViewModels;

namespace Samba.Modules.EntityModule
{
    public class EntitySearchResultViewModel : ObservableObject
    {
        public Entity Model { get; set; }
        public EntityType EntityType { get; set; }

        private EntityCustomDataViewModel _accountCustomDataViewModel;
        public EntityCustomDataViewModel AccountCustomDataViewModel
        {
            get { return _accountCustomDataViewModel ?? (_accountCustomDataViewModel = new EntityCustomDataViewModel(Model, EntityType)); }
        }

        public EntitySearchResultViewModel(Entity model, EntityType template)
        {
            EntityType = template;
            Model = model;
        }

        public string this[string index]
        {
            get { return AccountCustomDataViewModel.GetValue(index); }
        }

        public int Id { get { return Model.Id; } }

        public string NameDisplay
        {
            get { return EntityType.FormatEntityName(Model.Name); }
        }

        public string Name
        {
            get
            {
                return Model.Name;
            }
            set
            {
                Model.Name = value;
                RaisePropertyChanged(() => Name);
            }
        }

    }
}
