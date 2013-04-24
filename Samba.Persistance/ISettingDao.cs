using System.Collections.Generic;
using Samba.Domain.Models.Settings;

namespace Samba.Persistance
{
    public interface ISettingDao
    {
        string GetNextString(int numeratorId);
        int GetNextNumber(int numeratorId);
        IEnumerable<Terminal> GetTerminals();
    }
}
