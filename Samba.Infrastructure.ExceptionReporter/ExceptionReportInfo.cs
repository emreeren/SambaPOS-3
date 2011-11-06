using System;
using System.Collections.Generic;
using System.Reflection;

namespace Samba.Infrastructure.ExceptionReporter
{
    public class ExceptionReportInfo : Disposable
	{
		private readonly List<Exception> _exceptions = new List<Exception>();

        public Exception MainException
        {
            get { return _exceptions.Count > 0 ? _exceptions[0] : null; }
            set
            {
                _exceptions.Clear();
                _exceptions.Add(value);
            }
        }

        public IList<Exception> Exceptions
        {
            get { return _exceptions.AsReadOnly(); }
        }

        public void SetExceptions(IEnumerable<Exception> exceptions)
        {
            _exceptions.Clear();
            _exceptions.AddRange(exceptions);
        }

        public string CustomMessage { get; set; }

        public string MachineName { get; set; }
        public string UserName { get; set; }
        public DateTime ExceptionDate { get; set; }
        public string UserExplanation { get; set; }
        public Assembly AppAssembly { get; set; }
        public bool TakeScreenshot { get; set; }


	    public ExceptionReportInfo()
        {
            SetDefaultValues();
        }

        private void SetDefaultValues()
        {
            TakeScreenshot = false;
        }
    }

    public static class DefaultLabelMessages
    {
        public const string DefaultExplanationLabel = "Please enter a brief explanation of events leading up to this exception";
        public const string DefaultContactMessageTop = "The following details can be used to obtain support for this application";
	}
}
