using System.Collections.Generic;

namespace Samba.Infrastructure.Data
{
    public interface IEntityCreator<out TModel>
    {
        IEnumerable<TModel> CreateItems(IEnumerable<string> data);
    }
}