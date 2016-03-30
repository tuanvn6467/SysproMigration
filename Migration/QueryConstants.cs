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

        public static string Update_QueueMigrate = "Update_QueueMigrate";

        public static string Get_QueueMigrate = "Get_QueueMigrate";

        public static string QueryAdaptGetCompany =
            "select ROW_NUMBER() over (order by [Create_Date]) as DatabaseID,[Database_Name] as DatabaseName,'' as PhysicalName,0 as TenantID  from [{0}].[dbo].[databases]";

        public static string QueryNewCrmGetCompany =
            "select [DatabaseID],[DatabaseName],[PhysicalName],[TenantID],[CreatedDate] from [{0}].[dbo].[utl_Database] where SQLUserName = '{1}' and SQLPassword = '{2}' and StatusID = 1";

        /*public static string QueryAdaptGetUsers =
            "select 0 UserID,User_ID UserName, Email_Address Email from [adaptv3system].[dbo].[users]";*/

        public static string QueryAdaptGetUsers = "QueryAdaptGetUsers.txt";

        public static string QueryCreateTableTempDb = "QueryAdaptCreateTempTable.txt";

        public static string QueryAdaptFunctionGetNewValue = "QueryAdaptFunctionGetNewValue.txt";

        public static string QueryAdaptFunctionGetStateID = "QueryAdaptFunctionGetStateID.txt";

        public static string QueryAdaptFunctionGetCountyID = "QueryAdaptFunctionGetCountyID.txt";

        public static string QueryAdaptFunctionGetPostalCodeID = "QueryAdaptFunctionGetPostalCodeID.txt";

        public static string QueryAdaptViewPref1Pivot_Required = "QueryAdaptViewPref1Pivot_Required.txt";

        public static string QueryAdaptViewPref1Pivot_Default = "QueryAdaptViewPref1Pivot_Default.txt";

        public static string QueryAdaptViewPref6Pivot_Required = "QueryAdaptViewPref6Pivot_Required.txt";

        public static string QueryAdaptViewPref6Pivot_Default = "QueryAdaptViewPref6Pivot_Default.txt";

        public static string QueryAdaptViewPref3Pivot_Required = "QueryAdaptViewPref3Pivot_Required.txt";

        public static string QueryAdaptViewPref3Pivot_Default = "QueryAdaptViewPref3Pivot_Default.txt";

        public static string QueryAdaptViewServiceContractPrefPivot_Required = "QueryAdaptViewServiceContractPrefPivot_Required.txt";

        public static string QueryAdaptViewServiceContractPrefPivot_Default = "QueryAdaptViewServiceContractPrefPivot_Default.txt";

        public static string QueryAdaptViewPref2Pivot_Required = "QueryAdaptViewPref2Pivot_Required.txt";

        public static string QueryAdaptViewPref2Pivot_Default = "QueryAdaptViewPref2Pivot_Default.txt";

        public static string QueryAdaptViewPref5Pivot_Required = "QueryAdaptViewPref5Pivot_Required.txt";

        public static string QueryAdaptViewPref5Pivot_Default = "QueryAdaptViewPref5Pivot_Default.txt";

        public static string QueryCRM_CreateUniqueIndex = "QueryCRM_CreateUniqueIndex.txt";

        public static string QueryCRM_DropUniqueIndex = "QueryCRM_DropUniqueIndex.txt";

        public static string QueryAdapt_CreateIndexTable = "QueryAdapt_CreateIndexTable.txt";

        public static string QueryAdapt_DropIndexTable = "QueryAdapt_DropIndexTable.txt";

        public static string QueryAdaptFunctionCompareModuleID = "QueryAdaptFunctionCompareModuleID.txt";

        public static string QueryAdaptConvertFieldName = "QueryAdaptConvertFieldName.txt";

        public static string QueryAdaptFunctionConvertPhoneFax = "QueryAdaptFunctionConvertPhoneFax.txt";

        public static string QueryAdaptFunctionGetDate = "QueryAdaptFunctionGetDate.txt";

        public static string QueryAdaptFunctionGetDateTime = "QueryAdaptFunctionGetDateTime.txt";

        public static string QueryAdaptFunctionGetTime = "QueryAdaptFunctionGetTime.txt";

        public static string QueryAdaptFunctionGetCurrencyCodeID = "QueryAdaptFunctionGetCurrencyCodeID.txt";

        public static string QueryAdaptFunctionConvertValueByFieldType = "QueryAdaptFunctionConvertValueByFieldType.txt";

        public static string QueryAdaptFunctionConvertValueMultiple = "QueryAdaptFunctionConvertValueMultiple.txt";

        public static string QueryAdaptFunctionGetUserIDNewCRM = "QueryAdaptFunctionGetUserIDNewCRM.txt";

        public static string QueryAdaptFunctionGetRoleIDNewCRM = "QueryAdaptFunctionGetRoleIDNewCRM.txt";

        public static string QueryAdaptFunctionGetGroupIDNewCRM = "QueryAdaptFunctionGetGroupIDNewCRM.txt";

        public static string QueryAdaptCheckFunctionExist = "use tempdb " +
                                                            "if exists (select * from sys.objects where object_id = OBJECT_ID(N'[dbo].[{0}]')  " +
                                                            "AND type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' )) " +
                                                            "begin " +
                                                            "drop function [dbo].[{0}] " +
                                                            "end; ";

        public static string QueryAdaptCheckViewExist = "use tempdb " +
                                                            "if exists (select * from sys.objects where object_id = OBJECT_ID(N'[dbo].[{0}]')  " +
                                                            "AND type IN ( N'V')) " +
                                                            "begin " +
                                                            "drop view [dbo].[{0}] " +
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
