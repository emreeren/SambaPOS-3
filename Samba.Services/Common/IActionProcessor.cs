using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Samba.Services.Common
{
    public interface IActionProcessor
    {
        bool Handles(string actionType);
        void Process(ActionData actionData);
    }
}
