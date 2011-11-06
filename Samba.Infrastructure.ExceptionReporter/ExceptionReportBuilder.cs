using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Samba.Infrastructure.ExceptionReporter.SystemInfo;
using Samba.Infrastructure.Settings;

namespace Samba.Infrastructure.ExceptionReporter
{
    internal class ExceptionReportBuilder
    {
        private readonly ExceptionReportInfo _reportInfo;
        private StringBuilder _stringBuilder;
        private readonly IEnumerable<SysInfoResult> _sysInfoResults;

        public ExceptionReportBuilder(ExceptionReportInfo reportInfo)
        {
            _reportInfo = reportInfo;
        }

        public ExceptionReportBuilder(ExceptionReportInfo reportInfo, IEnumerable<SysInfoResult> sysInfoResults)
            : this(reportInfo)
        {
            _sysInfoResults = sysInfoResults;
        }

        public string Build()
        {
            _stringBuilder = new StringBuilder().AppendLine("-----------------------------");

            BuildGeneralInfo();
            BuildExceptionInfo();
            BuildAssemblyInfo();
            BuildSysInfo();

            return _stringBuilder.ToString();
        }

        private void BuildGeneralInfo()
        {
            _stringBuilder.AppendLine("[General Info]")
                .AppendLine()
                .AppendLine("Application: SambaPOS")
                .AppendLine("Version:     " + LocalSettings.AppVersion)
                .AppendLine("Region:      " + LocalSettings.CurrentLanguage)
                .AppendLine("DB:          " + LocalSettings.DatabaseLabel)
                .AppendLine("Machine:     " + _reportInfo.MachineName)
                .AppendLine("User:        " + _reportInfo.UserName)
                .AppendLine("Date:        " + _reportInfo.ExceptionDate.ToShortDateString())
                .AppendLine("Time:        " + _reportInfo.ExceptionDate.ToShortTimeString())
                .AppendLine();

            _stringBuilder.AppendLine("User Explanation:")
                .AppendLine()
                .AppendFormat("{0} said \"{1}\"", _reportInfo.UserName, _reportInfo.UserExplanation)
                .AppendLine().AppendLine("-----------------------------").AppendLine();
        }

        private void BuildExceptionInfo()
        {
            for (var index = 0; index < _reportInfo.Exceptions.Count; index++)
            {
                var exception = _reportInfo.Exceptions[index];

                _stringBuilder.AppendLine(string.Format("[Exception Info {0}]", index + 1))
                    .AppendLine()
                    .AppendLine(ExceptionHierarchyToString(exception))
                    .AppendLine().AppendLine("-----------------------------").AppendLine();
            }
        }

        private void BuildAssemblyInfo()
        {
            _stringBuilder.AppendLine("[Assembly Info]")
                .AppendLine()
                .AppendLine(CreateReferencesString(_reportInfo.AppAssembly))
                .AppendLine("-----------------------------").AppendLine();
        }

        private void BuildSysInfo()
        {
            _stringBuilder.AppendLine("[System Info]").AppendLine();
            _stringBuilder.Append(SysInfoResultMapper.CreateStringList(_sysInfoResults));
            _stringBuilder.AppendLine("-----------------------------").AppendLine();
        }

        public static string CreateReferencesString(Assembly assembly)
        {
            var stringBuilder = new StringBuilder();

            foreach (var assemblyName in assembly.GetReferencedAssemblies())
            {
                stringBuilder.AppendLine(string.Format("{0}, Version={1}", assemblyName.Name, assemblyName.Version));
            }

            return stringBuilder.ToString();
        }

        private static string ExceptionHierarchyToString(Exception exception)
        {
            var currentException = exception;
            var stringBuilder = new StringBuilder();
            var count = 0;

            while (currentException != null)
            {
                if (count++ == 0)
                    stringBuilder.AppendLine("Top-level Exception");
                else
                    stringBuilder.AppendLine("Inner Exception " + (count - 1));

                stringBuilder.AppendLine("Type:        " + currentException.GetType())
                             .AppendLine("Message:     " + currentException.Message)
                             .AppendLine("Source:      " + currentException.Source);

                if (currentException.StackTrace != null)
                    stringBuilder.AppendLine("Stack Trace: " + currentException.StackTrace.Trim());

                stringBuilder.AppendLine();
                currentException = currentException.InnerException;
            }

            var exceptionString = stringBuilder.ToString();
            return exceptionString.TrimEnd();
        }
    }
}
