using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Migration
{
    public static class Constants
    {
        public static string CustomerLog = "/CustomerLog";

        public static string SchemaConnection =
            "Data Source={0};Persist Security Info=True;User ID={1};Password={2};Max Pool Size=1000;Connection Timeout=1800;Pooling=true;";
        public static string DbConnection =
            "Data Source={0};Initial Catalog={1};Persist Security Info=True;User ID={2};Password={3};Max Pool Size=1000;Connection Timeout=1800;Pooling=true;";

        public static string DestinationConfig = "Destination";
        public static string SourceConfig = "Source";
        public static string MigrationConnectionConfig = "SysproMigration";
        public static string BatchSizeConfig = "BatchSize";

        public static string FieldMappingFileCompany = "fieldMappingFile_Company";
        public static string FieldMappingFileSecurity = "fieldMappingFile_Security";
        public static string FieldMappingFileSystem = "fieldMappingFile_System";

        public static string UserSqlAdapt = "userSQLAdapt";
        public static string PassSqlAdapt = "passSQLAdapt";
        public static string SystemDbAdapt = "systemDbAdapt";

        public static string SystemDbNewCrm = "SystemDbNewCrm";
        public static string DestinationServer = "DestinationServer";
        public const string AdminConnectionConfig = "DestinationAdmin";

        public static string AdaptDbPrefix = "adaptv3";

        public static string LogFileFormat = "Log_" + AdaptDbPrefix + "{0}_to_{1}_{2}";

        public const string PasswordQuestion = "Default password question?";
        public const string PasswordAnswer = "Default password answer";

        public static List<string> ListColSupportStatic = new List<string>
        {
            "TargetServer",
            "TargetCompany",
        };

        public static string MiggrateSupportTable = "[tempdb].[dbo].[MigrateSupport]";

    }
}
