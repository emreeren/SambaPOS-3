using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Samba.Services.Common
{
    public class OperationTestResult
    {
        public bool CanCompleteOperation { get { return string.IsNullOrEmpty(ErrorMessage); } }
        public string ErrorMessage { get; set; }

        public OperationTestResult(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }
    }
}
