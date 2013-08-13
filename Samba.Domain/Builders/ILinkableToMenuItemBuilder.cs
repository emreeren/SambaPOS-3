using Samba.Domain.Models.Menus;

namespace Samba.Domain.Builders
{
    public interface ILinkableToMenuItemBuilder<T> where T : ILinkableToMenuItemBuilder<T>
    {
        void Link(MenuItem menuItem);
        MenuItemBuilderFor<T> CreateMenuItem(string menuItemName);
    }
}