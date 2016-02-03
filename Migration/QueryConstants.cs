using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Migration
{
    public static class QueryConstants
    {

        public static string sec_IsValid_User_Email = "[st_Security].[dbo].[sec_IsValid_User_Email]";

        public static string sec_Update_UserInfo = "[st_Security].[dbo].[sec_Update_UserInfo]";

        public static string QueryAdaptGetCompany =
            "select ROW_NUMBER() over (order by [Create_Date]) as DatabaseID,[Database_Name] as DatabaseName,'' as PhysicalName,0 as TenantID  from [{0}].[dbo].[databases]";

        public static string QueryNewCrmGetCompany =
            "select [DatabaseID],[DatabaseName],[PhysicalName],[TenantID],[CreatedDate] from [{0}].[dbo].[utl_Database] where SQLUserName = '{1}' and SQLPassword = '{2}'";

        /*public static string QueryAdaptGetUsers =
            "select 0 UserID,User_ID UserName, Email_Address Email from [adaptv3system].[dbo].[users]";*/

        public static string QueryAdaptGetUsers = "QueryAdaptGetUsers.txt";

        public static string QueryCreateTableTempDb = "QueryAdaptCreateTempTable.txt";

        public static string QueryAdaptFunctionSupport = "QueryAdaptFunctionSupport.txt";

        public static string QueryAdaptCheckFunctionExist = "use tempdb " +
                                                            "if exists (select * from sys.objects where object_id = OBJECT_ID(N'[dbo].[GetNewValue]')  " +
                                                            "AND type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' )) " +
                                                            "begin " +
                                                            "drop function [dbo].[GetNewValue] " +
                                                            "end; ";

        public static string QueryInsertRecordTempDb =
            "Insert into " + Constants.MiggrateSupportTable + " " +
            "(SourceCompany,SourceTable,TargetServer,TargetCompany,TargetTable,OldValTable,NewValTable,TenantID,DatabaseID,CreatedDate) " +
            "values('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','" + DateTime.Now + "')";

        public static string QueryInsertUserTempdb =
            "if(not exists (select top 1 * from " + Constants.MiggrateSupportTable + " " +
                            "where SourceCompany = '{0}' and SourceTable = '{1}' " +
                            "and TargetServer = '{2}' and TargetCompany = '{3}' " +
                            "and TargetTable = '{4}' and OldValTable = '{5}' and TenantID = '{7}' and DatabaseID = '{8}'))" +
                "begin " +
                QueryInsertRecordTempDb +
                "end " +
                "else begin " +
                    "update " + Constants.MiggrateSupportTable + " set NewValTable = '{6}', CreatedDate = '" + DateTime.Now + "' " +
                            "where SourceCompany = '{0}' and SourceTable = '{1}' and TargetServer = '{2}' " +
                            "and TargetCompany = '{3}' and TargetTable = '{4}' and OldValTable = '{5}' " +
                            "and TenantID = '{7}' and DatabaseID = '{8}' end";

    }
}
