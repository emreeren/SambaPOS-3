using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Globalization;
using System.IO;
using System.Linq;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Initialization;
using Microsoft.Win32;
using Samba.Infrastructure.Data;
using Samba.Infrastructure.Data.SQL;
using Samba.Infrastructure.Data.Text;
using Samba.Infrastructure.Settings;

namespace Samba.Persistance.Data
{
    public static class WorkspaceFactory
    {
        private static TextFileWorkspace _textFileWorkspace;
        private static string _connectionString = LocalSettings.ConnectionString;

        static WorkspaceFactory()
        {
            Database.SetInitializer(new Initializer());

            if (string.IsNullOrEmpty(LocalSettings.ConnectionString))
            {
                if (IsSqlce40Installed())
                    LocalSettings.ConnectionString = string.Format("data source={0}\\{1}.sdf", LocalSettings.DocumentPath, LocalSettings.AppName);
                else LocalSettings.ConnectionString = GetTextFileName();
            }
            if (LocalSettings.ConnectionString.EndsWith(".sdf"))
            {
                Database.DefaultConnectionFactory =
                    new SqlCeConnectionFactory("System.Data.SqlServerCe.4.0", "", LocalSettings.ConnectionString);
            }
            else if (LocalSettings.ConnectionString.EndsWith(".txt"))
            {
                _textFileWorkspace = GetTextFileWorkspace();
            }
            else if (!string.IsNullOrEmpty(LocalSettings.ConnectionString))
            {
                var cs = LocalSettings.ConnectionString;
                if (!cs.Trim().EndsWith(";"))
                    cs += ";";
                if (!cs.ToLower().Contains("multipleactiveresultsets"))
                    cs += " MultipleActiveResultSets=True;";
                if (!cs.ToLower(CultureInfo.InvariantCulture).Contains("user id") && (!cs.ToLower(CultureInfo.InvariantCulture).Contains("integrated security")))
                    cs += " Integrated Security=True;";
                if (cs.ToLower(CultureInfo.InvariantCulture).Contains("user id") && !cs.ToLower().Contains("persist security info"))
                    cs += " Persist Security Info=True;";
                Database.DefaultConnectionFactory =
                    new SqlConnectionFactory(cs);
            }
        }

        private static bool IsSqlce40Installed()
        {
            var rk = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Microsoft SQL Server Compact Edition\\v4.0");
            return rk != null;
        }

        public static IWorkspace Create()
        {
            if (_textFileWorkspace != null) return _textFileWorkspace;
            return new EFWorkspace(new SambaContext(false));
        }

        public static IReadOnlyWorkspace CreateReadOnly()
        {
            if (_textFileWorkspace != null) return _textFileWorkspace;
            return new ReadOnlyEFWorkspace(new SambaContext(true));
        }

        private static TextFileWorkspace GetTextFileWorkspace()
        {
            var fileName = GetTextFileName();
            return new TextFileWorkspace(fileName, false);
        }

        private static string GetTextFileName()
        {
            return _connectionString.EndsWith(".txt")
                ? _connectionString
                : string.Format("{0}\\{1}.txt", LocalSettings.DocumentPath, LocalSettings.AppName);
        }

        public static void SetDefaultConnectionString(string cTestdataTxt)
        {
            _connectionString = cTestdataTxt;
            if (string.IsNullOrEmpty(_connectionString) || _connectionString.EndsWith(".txt"))
                _textFileWorkspace = GetTextFileWorkspace();
        }
    }

    public class Initializer : IDatabaseInitializer<SambaContext>
    {
        public void InitializeDatabase(SambaContext context)
        {
            if (!context.Database.Exists())
            {
                Create(context);
            }
            //#if DEBUG
            else if (!context.Database.CompatibleWithModel(false))
            {
                context.Database.Delete();
                Create(context);
            }
            //#else
            //            else
            //            {
            //                Migrate(context);
            //            }
            //#endif
            var version = context.ObjContext().ExecuteStoreQuery<long>("select top(1) Version from VersionInfo order by version desc").FirstOrDefault();
            LocalSettings.CurrentDbVersion = version;
        }

        private static void Create(CommonDbContext context)
        {
            context.Database.Create();
            context.ObjContext().ExecuteStoreCommand("CREATE TABLE VersionInfo (Version bigint not null)");
            context.ObjContext().ExecuteStoreCommand("CREATE NONCLUSTERED INDEX IX_Tickets_LastPaymentDate ON Tickets(LastPaymentDate)");
            GetMigrateVersions(context);
            LocalSettings.CurrentDbVersion = LocalSettings.DbVersion;
        }

        private static void GetMigrateVersions(CommonDbContext context)
        {
            for (var i = 0; i < LocalSettings.DbVersion; i++)
            {
                context.ObjContext().ExecuteStoreCommand("Insert into VersionInfo (Version) Values (" + (i + 1) + ")");
            }
        }

        private static void Migrate(CommonDbContext context)
        {
            if (!File.Exists(LocalSettings.UserPath + "\\migrate.txt")) return;

            var db = context.Database.Connection.ConnectionString.Contains(".sdf") ? "sqlserverce" : "sqlserver";

            using (IAnnouncer announcer = new TextWriterAnnouncer(Console.Out))
            {
                IRunnerContext migrationContext =
                    new RunnerContext(announcer)
                    {
                        Connection = context.Database.Connection.ConnectionString,
                        Database = db,
                        Target = LocalSettings.AppPath + "\\Samba.Persistance.DbMigration.dll"
                    };

                var executor = new TaskExecutor(migrationContext);
                executor.Execute();
            }
            File.Delete(LocalSettings.UserPath + "\\migrate.txt");
        }
    }
}
