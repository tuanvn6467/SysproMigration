using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Hosting;
using System.Web.SessionState;
using Migration;
using Migration.Common;
using Migration.Helper;
using Syspro.Core.Helper.Logging;
using SysproMigration.Models;

namespace SysproMigration.Utility
{
    public static class Utils
    {
        public static List<DatabaseModel> GetListDatabase(string sqlServerName, string sqlUserLogin, string sqlUserPassword, bool fromSource, ref List<User> userModels)
        {
            var lstDatabase = new List<DatabaseModel>();
            using (
                var con =
                    new SqlConnection(string.Format(Constants.SchemaConnection, sqlServerName, sqlUserLogin, sqlUserPassword))
                )
            {
                con.Open();
                DataTable databases = con.GetSchema("Databases");
                var temp = databases.Rows;
                foreach (DataRow database in databases.Rows)
                {
                    var databaseName = database.Field<String>("database_name");
                    var dbID = database.Field<short>("dbid");
                    var creationDate = database.Field<DateTime>("create_date");
                    lstDatabase.Add(new DatabaseModel
                    {
                        DatabaseID = dbID,
                        DatabaseName = databaseName,
                        CreatedDate = creationDate,
                    });
                }
                if (fromSource)
                {
                    userModels = GetListUserAdapt(con);
                }
                con.Close();
            }
            return lstDatabase;
        }

        public static List<User> GetListUserAdapt(SqlConnection con)
        {
            var userModels = new List<User>();
            var query = QueryConstants.QueryAdaptGetUsers.GetTextInQueryFixedFolder();
            using (SqlCommand command = new SqlCommand(query, con))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        userModels.Add(new User
                        {
                            UserID = ParseData.GetInt(reader["UserID"].ToString()) ?? 0,
                            UserName = ParseData.GetString(reader["UserName"]),
                            Email = ParseData.GetString(reader["Email"]),
                            TenantID = 0,
                            UserGUID = null,
                            FirstName = ParseData.GetString(reader["FirstName"]),
                            LastName = ParseData.GetString(reader["LastName"]),
                            StatusID = ParseData.GetShort(reader["StatusID"]),
                            LicenseType = ParseData.GetShort(reader["LicenseType"]),
                            JobTitle = ParseData.GetString(reader["JobTitle"]),
                            Department = ParseData.GetString(reader["Department"]),
                            WorkPhone = ParseData.GetString(reader["WorkPhone"]),
                            WorkPhoneExt = ParseData.GetString(reader["WorkPhoneExt"]),
                            HomePhone = ParseData.GetString(reader["HomePhone"]),
                            MobilePhone = ParseData.GetString(reader["MobilePhone"]),
                            FaxPhone = ParseData.GetString(reader["FaxPhone"]),
                            MailingAddress = ParseData.GetShort(reader["MailingAddress"]),
                            AddressLine1 = ParseData.GetString(reader["AddressLine1"]),
                            AddressLine2 = ParseData.GetString(reader["AddressLine2"]),
                            AddressLine3 = ParseData.GetString(reader["AddressLine3"]),
                            City = ParseData.GetString(reader["City"]),
                            County = ParseData.GetLong(reader["County"]) ?? 0,
                            State = ParseData.GetLong(reader["State"]) ?? 0,
                            PostalCode = ParseData.GetString(reader["PostalCode"]),
                            Country = ParseData.GetLong(reader["Country"]) ?? 0,
                            CreatedDate = ParseData.GetDateTime(reader["CreatedDate"]),
                            FullName = ParseData.GetString(reader["FullName"]),
                            IsRegionalSettings = ParseData.GetShort(reader["IsRegionalSettings"]) ?? 0,
                            DateFormatType = ParseData.GetShort(reader["DateFormatType"]) ?? 0,
                            DatePictureFormat = ParseData.GetString(reader["DatePictureFormat"]),
                            TimeFormatType = ParseData.GetShort(reader["TimeFormatType"]) ?? 0,
                            TimePictureFormat = ParseData.GetString(reader["TimePictureFormat"]),
                            PhoneNumberFormat = ParseData.GetString(reader["PhoneNumberFormat"]),
                            IsUsePostalCodeTable = ParseData.GetShort(reader["IsUsePostalCodeTable"]) ?? 0,
                            DialFromLocation = ParseData.GetString(reader["DialFromLocation"]),
                            DefaultTemporaryDirectory = ParseData.GetString(reader["DefaultTemporaryDirectory"]),
                            DefaultCalendarID = ParseData.GetInt(reader["DefaultCalendarID"]),
                            DefaultActivityTypeID = ParseData.GetInt(reader["DefaultActivityTypeID"]),
                            DefaultAppointmentTypeID = ParseData.GetInt(reader["DefaultAppointmentTypeID"]),
                            DefaultTaskPriorityID = ParseData.GetInt(reader["DefaultTaskPriorityID"]) ?? 0,
                            DefaultEmailMethod = ParseData.GetInt(reader["DefaultEmailMethod"]) ?? 0,
                            DefaultFaxMethod = ParseData.GetInt(reader["DefaultFaxMethod"]) ?? 0,
                            DefaultEmailAccount = ParseData.GetInt(reader["DefaultEmailAccount"]) ?? 0,
                            IsOutlookSyncInboundEmails = ParseData.GetShort(reader["IsOutlookSyncInboundEmails"]) ?? 0,
                            IsOutlookAutoSyncFlag = ParseData.GetShort(reader["IsOutlookAutoSyncFlag"]) ?? 0,
                            IsOutlookSyncOnLogin = ParseData.GetShort(reader["IsOutlookSyncOnLogin"]) ?? 0,
                            IsWriteAppointmentsToOutlook = ParseData.GetShort(reader["IsWriteAppointmentsToOutlook"]) ?? 0,
                            IsWriteContactsToOutlook = ParseData.GetShort(reader["IsWriteContactsToOutlook"]) ?? 0,
                            WriteTasksToOutlook = ParseData.GetShort(reader["WriteTasksToOutlook"]) ?? 0,
                            OutlookAutoSyncMinutes = ParseData.GetInt(reader["OutlookAutoSyncMinutes"]) ?? 0,
                            IsEnableAlarm = ParseData.GetShort(reader["IsEnableAlarm"]) ?? 0,
                            IsAlarmOnNewAppt = ParseData.GetShort(reader["IsAlarmOnNewAppt"]) ?? 0,
                            AlarmOnApptDuePeriod = ParseData.GetShort(reader["AlarmOnApptDuePeriod"]) ?? 0,
                            AlarmOnApptDueUnits = ParseData.GetShort(reader["AlarmOnApptDueUnits"]) ?? 0,
                            AlarmOnNewApptPriority = ParseData.GetShort(reader["AlarmOnNewApptPriority"]) ?? 0,
                            IsAlarmOnNewTask = ParseData.GetShort(reader["IsAlarmOnNewTask"]) ?? 0,
                            AlarmOnTaskDuePeriod = ParseData.GetShort(reader["AlarmOnTaskDuePeriod"]) ?? 0,
                            AlarmOnTaskDueUnits = ParseData.GetShort(reader["AlarmOnTaskDueUnits"]) ?? 0,
                            AlarmOnNewTaskPriority = ParseData.GetShort(reader["AlarmOnNewTaskPriority"]) ?? 0,
                            DefaultSnoozePeriod = ParseData.GetShort(reader["DefaultSnoozePeriod"]) ?? 0,
                            DefaultSnoozeUnits = ParseData.GetShort(reader["DefaultSnoozeUnits"]) ?? 0,
                            HomeCountry = ParseData.GetLong(reader["HomeCountry"]),
                        });
                    }
                }
            }
            return userModels;
        }

        public static List<QueueMigrate> GetQueueMigrates(SqlConnection queueConnection,QueueMigrate objectQueue)
        {
            var lst = new List<QueueMigrate>();
            using (SqlCommand cmd = new SqlCommand(QueryConstants.Get_QueueMigrate, queueConnection))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@SourceServerName", SqlDbType.VarChar).Value = objectQueue.SourceServerName;
                cmd.Parameters.Add("@SourceDatabaseCompany", SqlDbType.VarChar).Value = objectQueue.SourceDatabaseCompany;
                cmd.Parameters.Add("@TargetServerName", SqlDbType.VarChar).Value = objectQueue.TargetServerName;
                cmd.Parameters.Add("@TargetDatabaseCompany", SqlDbType.VarChar).Value = objectQueue.TargetDatabaseCompany;

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lst.Add(new QueueMigrate
                        {
                           Id = ParseData.GetInt(reader["Id"]) ?? 0,
                           SourceServerName = ParseData.GetString(reader["SourceServerName"]) ?? "",
                           SourceDatabaseCompany = ParseData.GetString(reader["SourceDatabaseCompany"]) ?? "",
                           SourceTable = ParseData.GetString(reader["SourceTable"]) ?? "",
                           TargetServerName = ParseData.GetString(reader["TargetServerName"]) ?? "",
                           TargetDatabaseCompany = ParseData.GetString(reader["TargetDatabaseCompany"]) ?? "",
                           TargetTable = ParseData.GetString(reader["TargetTable"]) ?? "",
                           SqlQuery = ParseData.GetString(reader["SqlQuery"]) ?? "",
                           IsGetKeys = ParseData.GetShort(reader["IsGetKeys"]) ?? 0,
                           IsLastRecord = ParseData.GetShort(reader["IsLastRecord"]) ?? 0,
                           FieldsMapId = ParseData.GetShort(reader["FieldsMapId"]) ?? 0,
                           Status = ParseData.GetShort(reader["Status"]) ?? 0,
                           Exception = ParseData.GetString(reader["Exception"]) ?? "", 
                           CreatedDate = ParseData.GetDateTime(reader["CreatedDate"]),
                           UpdatedDate = ParseData.GetDateTime(reader["UpdatedDate"]),
                        });
                    }
                }
            }
            return lst;
        }

        public static void CreateUniqueIndex(SqlConnection con)
        {
            Logging.PushInfo("Create Unique Index In New CRM");
            var query = QueryConstants.QueryCRM_CreateUniqueIndex.GetTextInQueryFixedFolder();
            query = string.Format(query, con.Database);
            using (SqlCommand command = new SqlCommand(query, con))
            {
                command.ExecuteNonQuery();
            }
        }

        public static void DropUniqueIndex(SqlConnection con)
        {
            Logging.PushInfo("Drop Unique Index In New CRM");
            var query = QueryConstants.QueryCRM_DropUniqueIndex.GetTextInQueryFixedFolder();
            query = string.Format(query, con.Database);
            using (SqlCommand command = new SqlCommand(query, con))
            {
                command.ExecuteNonQuery();
            }
        }

        public static void CreateSupportTempDb(SqlConnection con)
        {
            var queryTable = QueryConstants.QueryCreateTableTempDb.GetTextInQueryFixedFolder();
            using (SqlCommand command = new SqlCommand(queryTable, con))
            {
                command.ExecuteNonQuery();
            }
            //function get new value
            CheckFunctionExist(con, FunctionConstants.GetNewValue);
            var queryFunctionGetNewValue = QueryConstants.QueryAdaptFunctionGetNewValue.GetTextInQueryFixedFolder();
            Execute(con, queryFunctionGetNewValue);
            //function get state id
            CheckFunctionExist(con, FunctionConstants.GetStateID);
            var queryFunctionGetStateID = QueryConstants.QueryAdaptFunctionGetStateID.GetTextInQueryFixedFolder();
            Execute(con, queryFunctionGetStateID);
            //function get county id
            CheckFunctionExist(con, FunctionConstants.GetCountyID);
            var queryFunctionGetCountyID = QueryConstants.QueryAdaptFunctionGetCountyID.GetTextInQueryFixedFolder();
            Execute(con, queryFunctionGetCountyID);
            //function get postal code id
            CheckFunctionExist(con, FunctionConstants.GetPostalCodeID);
            var queryFunctionGetPostalCodeID = QueryConstants.QueryAdaptFunctionGetPostalCodeID.GetTextInQueryFixedFolder();
            Execute(con, queryFunctionGetPostalCodeID);
            //view pivot pref1 required table
            CheckViewExist(con, ViewConstants.Pref1_Pivot_Required);
            var queryViewPref1Pivot_Required = QueryConstants.QueryAdaptViewPref1Pivot_Required.GetTextInQueryFixedFolder();
            Execute(con, queryViewPref1Pivot_Required);
            //view pivot pref1 default table
            CheckViewExist(con, ViewConstants.Pref1_Pivot_Default);
            var queryViewPref1Pivot_Default = QueryConstants.QueryAdaptViewPref1Pivot_Default.GetTextInQueryFixedFolder();
            Execute(con, queryViewPref1Pivot_Default);
            //function compare module
            CheckFunctionExist(con, FunctionConstants.CompareModuleID);
            var queryFunctionCompareModuleID = QueryConstants.QueryAdaptFunctionCompareModuleID.GetTextInQueryFixedFolder();
            Execute(con, queryFunctionCompareModuleID);
            //function convert field name
            CheckFunctionExist(con, FunctionConstants.ConvertFieldName);
            var queryFunctionConvertFieldName = QueryConstants.QueryAdaptConvertFieldName.GetTextInQueryFixedFolder();
            Execute(con, queryFunctionConvertFieldName);
        }

        private static void CheckViewExist(SqlConnection con,string viewName)
        {
            var queryCheckViewExist = string.Format(QueryConstants.QueryAdaptCheckViewExist, viewName);
            using (SqlCommand command = new SqlCommand(queryCheckViewExist, con))
            {
                command.ExecuteNonQuery();
            }
        }
        private static void CheckFunctionExist(SqlConnection con,string functionName)
        {
            var queryCheckFunctionExist = string.Format(QueryConstants.QueryAdaptCheckFunctionExist, functionName);
            using (SqlCommand command = new SqlCommand(queryCheckFunctionExist, con))
            {
                command.ExecuteNonQuery();
            }
        }
        public static void Execute(SqlConnection con,string query)
        {
            using (SqlCommand command = new SqlCommand(query, con))
            {
                command.ExecuteNonQuery();
            }
        }
        

        public static List<DatabaseModel> GetListCompany(string sqlServerName, string dbName,string sqlUserLogin,
            string sqlUserPassword, bool isCompanyAdapt = true)
        {
            var lstCompany = new List<DatabaseModel>();
            using (
                var con =
                    new SqlConnection(string.Format(Constants.DbConnection, sqlServerName, dbName, sqlUserLogin, sqlUserPassword))
                )
            {
                con.Open();

                var query = isCompanyAdapt
                    ? string.Format(QueryConstants.QueryAdaptGetCompany,
                        ConfigurationManager.AppSettings[Constants.SystemDbAdapt])
                    : string.Format(QueryConstants.QueryNewCrmGetCompany,
                        ConfigurationManager.AppSettings[Constants.SystemDbNewCrm],sqlUserLogin,sqlUserPassword);

                using (SqlCommand command = new SqlCommand(query, con))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lstCompany.Add(new DatabaseModel
                            {
                                DatabaseID = ParseData.GetInt(reader["DatabaseID"].ToString()) ?? 0 ,
                                DatabaseName = reader["DatabaseName"].ToString(),
                                PhysicalName = reader["PhysicalName"].ToString(),
                                TenantID = ParseData.GetInt(reader["TenantID"].ToString()) ?? 0,
                            });
                        }
                    }
                }

                con.Close();
            }
            return lstCompany;
        }

        public static string ConvertTitleToFileName(string stringUnicode)
        {
            const string FindText = "áàảãạâấầẩẫậăắằẳẵặđéèẻẽẹêếềểễệíìỉĩịóòỏõọôốồổỗộơớờởỡợúùủũụưứừửữựýỳỷỹỵÁÀẢÃẠÂẤẦẨẪẬĂẮẰẲẴẶĐÉÈẺẼẸÊẾỀỂỄỆÍÌỈĨỊÓÒỎÕỌÔỐỒỔỖỘƠỚỜỞỠỢÚÙỦŨỤƯỨỪỬỮỰÝỲỶỸỴ";
            const string ReplText = "aaaaaaaaaaaaaaaaadeeeeeeeeeeeiiiiiooooooooooooooooouuuuuuuuuuuyyyyyAAAAAAAAAAAAAAAAADEEEEEEEEEEEIIIIIOOOOOOOOOOOOOOOOOUUUUUUUUUUUYYYYY";
            int index = -1;
            char[] arrChar = FindText.ToCharArray();
            while ((index = stringUnicode.IndexOfAny(arrChar)) != -1)
            {
                int index2 = FindText.IndexOf(stringUnicode[index]);
                stringUnicode = stringUnicode.Replace(stringUnicode[index], ReplText[index2]);
            }
            stringUnicode = stringUnicode.Replace(":", "_");
            stringUnicode = Regex.Replace(stringUnicode, "[^a-zA-Z0-9_.]+", " ", RegexOptions.Compiled);
            return stringUnicode.Trim();
        }

        public static bool CheckExistEmail(SqlConnection desConn, int tennantID, string email)
        {
            var isValid = true;
            using (SqlCommand cmd = new SqlCommand(QueryConstants.sec_IsValid_User_Email, desConn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@UserID", SqlDbType.Int).Value = 0;
                cmd.Parameters.Add("@TenantID", SqlDbType.Int).Value = tennantID;
                cmd.Parameters.Add("@Email", SqlDbType.VarChar).Value = email;
                cmd.Parameters.Add("@IsValid", SqlDbType.Bit).Direction = ParameterDirection.Output;
                cmd.ExecuteNonQuery();
                // read output
                isValid = ParseData.GetBool(cmd.Parameters["@IsValid"].Value);
            }
            return isValid;
        }

        public static int UpdateUserInfo(SqlConnection desConn, User user)
        {
            using (SqlCommand cmd = new SqlCommand(QueryConstants.sec_Update_UserInfo, desConn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@UpdateUserID", SqlDbType.Int).Value = 0;
                cmd.Parameters.Add("@XML", SqlDbType.VarChar).Value = XMLHelper.SerializeXML<User>(user);
                cmd.Parameters.Add("@UserID", SqlDbType.Int).Direction = ParameterDirection.Output;
                cmd.ExecuteNonQuery();
                // read output
                var userId = ParseData.GetInt(cmd.Parameters["@UserID"].Value) ?? -1;
                return userId;
            }
        }

        public static int UpdateQueueTable(SqlConnection conn, QueueMigrate queueMigrate)
        {
            using (SqlCommand cmd = new SqlCommand(QueryConstants.Update_QueueMigrate, conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = queueMigrate.Id;
                cmd.Parameters.Add("@SourceServerName", SqlDbType.VarChar).Value = queueMigrate.SourceServerName;
                cmd.Parameters.Add("@SourceDatabaseCompany", SqlDbType.VarChar).Value = queueMigrate.SourceDatabaseCompany;
                cmd.Parameters.Add("@SourceTable", SqlDbType.VarChar).Value = queueMigrate.SourceTable;
                cmd.Parameters.Add("@TargetServerName", SqlDbType.VarChar).Value = queueMigrate.TargetServerName;
                cmd.Parameters.Add("@TargetDatabaseCompany", SqlDbType.VarChar).Value = queueMigrate.TargetDatabaseCompany;
                cmd.Parameters.Add("@TargetTable", SqlDbType.VarChar).Value = queueMigrate.TargetTable;
                cmd.Parameters.Add("@SqlQuery", SqlDbType.NVarChar).Value = queueMigrate.SqlQuery;
                cmd.Parameters.Add("@IsLastRecord", SqlDbType.SmallInt).Value = queueMigrate.IsLastRecord;
                cmd.Parameters.Add("@IsGetKeys", SqlDbType.SmallInt).Value = queueMigrate.IsGetKeys;
                cmd.Parameters.Add("@FieldsMapId", SqlDbType.SmallInt).Value = queueMigrate.FieldsMapId;
                cmd.Parameters.Add("@Status", SqlDbType.SmallInt).Value = queueMigrate.Status;
                cmd.Parameters.Add("@Exception", SqlDbType.NVarChar).Value = queueMigrate.Exception;
                return cmd.ExecuteNonQuery();
            }
        }

        public static string GetTextInQueryFixedFolder(this string fileName)
        {
            using (
                var sr =
                    new StreamReader(Path.Combine(HostingEnvironment.ApplicationPhysicalPath,
                        ConfigurationManager.AppSettings["QueryFixedFolder"] + fileName)))
            {
                var mapString = sr.ReadToEnd();
                return mapString;
            }
        }

        public static string ReplaceSpecialCharacter(this string text)
        {
            return Regex.Replace(text, "[^0-9a-zA-Z]+", "");
        }

        public static SqlConnectionStringBuilder GetObjectConnection(this string conn)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(conn);
            return builder;
        }

        public static void CloseConnection(this SqlConnection connection)
        {
            if (connection != null && connection.State == ConnectionState.Open)
            {
                connection.Close();
                connection.Dispose();
                connection = null;
            }
        }
        public static SqlConnection CreateAndOpenConnection(this string connectionString, string targetConnection)
        {
            var conn = new SqlConnection(connectionString);

            Logging.PushInfo(string.Format("Connecting to {0} SQL Server", targetConnection));

            conn.Open();

            Logging.PushInfo("Connected");

            return conn;
        }
    }
}