using System;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Globalization;
using System.IO;
using System.Linq;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Initialization;
using Samba.Infrastructure.Data;
using Samba.Infrastructure.Data.SQL;
using Samba.Infrastructure.Data.Text;
using Samba.Infrastructure.Settings;

namespace Samba.Persistance.Data
{
    public static class WorkspaceFactory
    {
        private static TextFileWorkspace _textFileWorkspace;
        private static string _connectionString;

        static WorkspaceFactory()
        {
            UpdateConnection(LocalSettings.ConnectionString);
        }

        public static void UpdateConnection(string connectionString)
        {
            _connectionString = connectionString;
            Database.SetInitializer(new Initializer());

            if (string.IsNullOrEmpty(_connectionString))
            {
                if (LocalSettings.IsSqlce40Installed())
                    _connectionString = string.Format("data source={0}\\{1}.sdf", LocalSettings.DocumentPath, LocalSettings.AppName);
                else _connectionString = GetTextFileName();
            }
            if (_connectionString.EndsWith(".sdf"))
            {
                if (!_connectionString.ToLower().Contains("data source") && !_connectionString.Contains(":\\"))
                    _connectionString = string.Format("data source={0}\\{1}", LocalSettings.DocumentPath, _connectionString);

                Database.DefaultConnectionFactory = new SqlCeConnectionFactory("System.Data.SqlServerCe.4.0", "", _connectionString);
            }
            else if (_connectionString.EndsWith(".txt"))
            {
                _textFileWorkspace = GetTextFileWorkspace();
            }
            else if (!string.IsNullOrEmpty(_connectionString))
            {
                var cs = _connectionString;
                if (!cs.Trim().EndsWith(";"))
                    cs += ";";
                if (!cs.ToLower().Contains("multipleactiveresultsets"))
                    cs += " MultipleActiveResultSets=True;";
                if (!cs.ToLower(CultureInfo.InvariantCulture).Contains("user id") &&
                    (!cs.ToLower(CultureInfo.InvariantCulture).Contains("integrated security")))
                    cs += " Integrated Security=True;";
                if (cs.ToLower(CultureInfo.InvariantCulture).Contains("user id") &&
                    !cs.ToLower().Contains("persist security info"))
                    cs += " Persist Security Info=True;";
                Database.DefaultConnectionFactory = new SqlConnectionFactory(cs);
            }
        }

        public static IWorkspace Create()
        {
            if (_textFileWorkspace != null) return _textFileWorkspace;
            return new EFWorkspace(new DataContext(false));
        }

        public static IReadOnlyWorkspace CreateReadOnly()
        {
            if (_textFileWorkspace != null) return _textFileWorkspace;
            return new ReadOnlyEFWorkspace(new DataContext(true));
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

    public class Initializer : IDatabaseInitializer<DataContext>
    {

        public void InitializeDatabase(DataContext context)
        {
            if (!context.Database.Exists())
            {
                Create(context);
            }
            //#if DEBUG
            //            else if (!context.Database.CompatibleWithModel(false))
            //            {
            //                context.Database.Delete();
            //                Create(context);
            //            }
            //#else
            else
            {
                Migrate(context);
            }
            //#endif
            var version = context.ObjContext().ExecuteStoreQuery<long>("select top(1) Version from VersionInfo order by version desc").FirstOrDefault();
            LocalSettings.CurrentDbVersion = version;
        }

        private static void Create(CommonDbContext context)
        {
            context.Database.Create();
            context.ObjContext().ExecuteStoreCommand("CREATE TABLE VersionInfo (Version bigint not null)");
            context.ObjContext().ExecuteStoreCommand("CREATE NONCLUSTERED INDEX IX_Tickets_LastPaymentDate ON Tickets(LastPaymentDate)");
            context.ObjContext().ExecuteStoreCommand("CREATE UNIQUE INDEX IX_EntityStateValue_EntityId ON EntityStateValues (EntityId)");
            context.ObjContext().SaveChanges();
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

            var preVersion = context.ObjContext().ExecuteStoreQuery<long>("select top(1) Version from VersionInfo order by version desc").FirstOrDefault();
            var db = context.Database.Connection.ConnectionString.Contains(".sdf") ? "sqlserverce" : "sqlserver";
            if (preVersion < 18 && db == "sqlserverce") ApplyV16Fix(context);

            IAnnouncer announcer = new TextWriterAnnouncer(Console.Out);

            IRunnerContext migrationContext =
                new RunnerContext(announcer)
                {
                    ApplicationContext = context,
                    Connection = context.Database.Connection.ConnectionString,
                    Database = db,
                    Target = LocalSettings.AppPath + "\\Samba.Persistance.DbMigration.dll"
                };

            new TaskExecutor(migrationContext).Execute();

            File.Delete(LocalSettings.UserPath + "\\migrate.txt");
        }

        private static void ApplyV16Fix(CommonDbContext context)
        {
            try
            {
                context.ObjContext().ExecuteStoreCommand("Alter Table PaymentTypeMaps drop column DisplayAtPaymentScreen");
            }
            catch { }

            try
            {
                context.ObjContext().ExecuteStoreCommand("Alter Table PaymentTypeMaps drop column DisplayUnderTicket");
            }
            catch { }
        }
    }
}
