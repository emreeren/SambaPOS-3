using System;
using System.Collections.Generic;
using System.Reflection;
using Samba.Infrastructure.ExceptionReporter.SystemInfo;

namespace Samba.Infrastructure.ExceptionReporter
{
	public class ExceptionReportGenerator : Disposable
	{
		private readonly ExceptionReportInfo _reportInfo;
		private readonly List<SysInfoResult> _sysInfoResults = new List<SysInfoResult>();

		public ExceptionReportGenerator(ExceptionReportInfo reportInfo)
		{
			if (reportInfo == null)
				throw new ExceptionReportGeneratorException("reportInfo cannot be null");

			_reportInfo = reportInfo;

			_reportInfo.ExceptionDate = DateTime.UtcNow;
			_reportInfo.UserName = Environment.UserName;
			_reportInfo.MachineName = Environment.MachineName;

            if (_reportInfo.AppAssembly == null)
			    _reportInfo.AppAssembly = Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();
		}

		public string CreateExceptionReport()
		{
			var sysInfoResults = GetOrFetchSysInfoResults();
			var reportBuilder = new ExceptionReportBuilder(_reportInfo, sysInfoResults);
			return reportBuilder.Build();
		}

		internal IList<SysInfoResult> GetOrFetchSysInfoResults()
		{
			if (_sysInfoResults.Count == 0)
				_sysInfoResults.AddRange(CreateSysInfoResults());

			return _sysInfoResults.AsReadOnly();
		}

		private static IEnumerable<SysInfoResult> CreateSysInfoResults()
		{
			var retriever = new SysInfoRetriever();
			var results = new List<SysInfoResult>
			              {
			              	retriever.Retrieve(SysInfoQueries.OperatingSystem).Filter(
			              		new[]
			              		{
			              			"CodeSet", "CurrentTimeZone", "FreePhysicalMemory",
			              			"OSArchitecture", "OSLanguage", "Version"
			              		}),
			              	retriever.Retrieve(SysInfoQueries.Machine).Filter(
			              		new[]
			              		{
			              			"Machine", "UserName", "TotalPhysicalMemory", "Manufacturer", "Model"
			              		}),
			              };
			return results;
		}

		protected override void DisposeManagedResources()
		{
			_reportInfo.Dispose();
			base.DisposeManagedResources();
		}
	}

    [Serializable]
	internal class ExceptionReportGeneratorException : Exception
	{
		public ExceptionReportGeneratorException(string message) : base(message)
		{ }
	}
}