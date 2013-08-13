namespace Samba.Domain.Builders
{
    public class MenuItemBuilderFor<T> where T : ILinkableToMenuItemBuilder<T>
    {
        private readonly T _parent;
        private readonly MenuItemBuilder _menuItemBuilder;

        private MenuItemBuilderFor(T parent, string menuItemName)
        {
            _parent = parent;
            _menuItemBuilder = new MenuItemBuilder(menuItemName);
        }

        public static MenuItemBuilderFor<T> Create(string menuItemName, T parent)
        {
            return new MenuItemBuilderFor<T>(parent, menuItemName);
        }

        public MenuItemBuilderFor<T> AddPortion(string portionName, decimal price)
        {
            _menuItemBuilder.AddPortion(portionName, price);
            return this;
        }

        public MenuItemBuilderFor<T> WithGroupCode(string groupCode)
        {
            _menuItemBuilder.WithGroupCode(groupCode);
            return this;
        }

        public MenuItemBuilderFor<T> WithProductTag(string productTag)
        {
            _menuItemBuilder.WithProductTag(productTag);
            return this;
        }

        public T Do()
        {
            _parent.Link(_menuItemBuilder.Build());
            return _parent;
        }
    }
}