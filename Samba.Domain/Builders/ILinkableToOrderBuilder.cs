namespace Samba.Domain.Builders
{
    public interface ILinkableToOrderBuilder<T> where T : ILinkableToOrderBuilder<T>
    {
        void Link(OrderBuilder orderBuilder);
        OrderBuilderFor<T> AddOrder();
    }
}