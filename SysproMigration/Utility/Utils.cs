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

        public static void CreateSupportTempDb(SqlConnection con)
        {
            var queryTable = QueryConstants.QueryCreateTableTempDb.GetTextInQueryFixedFolder();
            var queryCheckFunctionExist = QueryConstants.QueryAdaptCheckFunctionExist;
            var queryFunction = QueryConstants.QueryAdaptFunctionSupport.GetTextInQueryFixedFolder();
            using (SqlCommand command = new SqlCommand(queryTable, con))
            {
                command.ExecuteNonQuery();
            }
            using (SqlCommand command = new SqlCommand(queryCheckFunctionExist, con))
            {
                command.ExecuteNonQuery();
            }
            using (SqlCommand command = new SqlCommand(queryFunction, con))
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
    }
}