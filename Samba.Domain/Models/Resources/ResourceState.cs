using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Resources
{
    public class ResourceState : Entity
    {
        public string Color { get; set; }

        private static ResourceState _empty;
        public static ResourceState Empty
        {
            get { return _empty ?? (_empty = new ResourceState { Color = "Silver" }); }
        }
    }
}
