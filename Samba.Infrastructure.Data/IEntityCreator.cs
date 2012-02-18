using System.Collections.Generic;

namespace Samba.Infrastructure.Data
{
    public interface IEntityCreator<TModel>
    {
        IEnumerable<TModel> CreateItems(IEnumerable<string> data);
    }
}