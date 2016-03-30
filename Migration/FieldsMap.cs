using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Xml;
using Migration.Common;

namespace Migration
{
    public class FieldsMap
    {
        public FieldsMap()
        {
            NeedTruncate = true;
            IsGetKeys = false;
            WhereGlobal = "";
        }

        public int Id { get; set; }

        public Source Source { get; set; }

        public Source Destination { get; set; }

        public Dictionary<string, string> Map { get; set; }
        
        public bool NeedTruncate { get; set; }

        public bool IsGetKeys { get; set; }

        public string WhereGlobal { get; set; }

        public Dictionary<string, string> MapKeys { get; set; }

        public int Size { get; set; }

        public string ReplaceKeySpecial(string key, int tenantID = 0)
        {
            var result = key;
            
            if (result.Contains("Syspro_TenantID"))
            {
                result = result.Replace("Syspro_TenantID", string.Format("convert(int, {0})", tenantID));
            }else if (result.Contains("_Syspro_TF"))
            {
                var fieldName = result.Replace("_Syspro_TF", "");
                result = string.Format("case when {0} = 'T' then 1 else 0 end {1}", fieldName,
                    fieldName.Replace(".", ""));
            }else if (result.Contains("_Syspro_FD")) //convert float to decimal(15,4)
            {
                var fieldName = result.Replace("_Syspro_FD", "");
                result = string.Format("convert(decimal(15,4),{0}) {1}", fieldName,fieldName.Replace(".", ""));
            }else if (result.Contains("_Syspro_FBI")) //convert float to bigint
            {
                var fieldName = result.Replace("_Syspro_FBI", "");
                result = string.Format("convert(bigint,{0}) {1}", fieldName,fieldName.Replace(".", ""));
            }else if (result.Contains("_Syspro_FI")) //convert float to int
            {
                var fieldName = result.Replace("_Syspro_FI", "");
                result = string.Format("convert(int,{0}) {1}", fieldName,fieldName.Replace(".", ""));
            }
            else if (result.Contains(".Notes") && !result.Contains(".Notes)") && !result.Contains(".Notes,"))
            {
                var nameField = result;
                result = string.Format("RTRIM(LTRIM(convert(varchar(max),isnull({0},'')))) Notes", nameField);
            }
            else if (result.Contains(".Created_Date") && !result.Contains(".Created_Date)") && !result.Contains(".Created_Date,"))
            {
                var nameField = result.Replace(".Created_Date","");
                result = string.Format(
                    "case when {0}.Created_Date not like '%0000%' " +
                    "then convert(datetime,substring({0}.Created_Date, 1, 4) + '-' + substring({0}.Created_Date, 5, 2) + '-' + substring({0}.Created_Date, 7, 2) " +
                    "+ ' ' + substring({0}.Created_Time, 1, 2) + ':' + substring({0}.Created_Time, 3, 2) + ':' + substring({0}.Created_Time, 5, 2)) else null end Created_Date",
                    nameField);
            }
            else if (result.Contains(".Modified_Date") && !result.Contains(".Modified_Date)") && !result.Contains(".Modified_Date,"))
            {
                var nameField = result.Replace(".Modified_Date", "");
                result = string.Format(
                    "case when {0}.Modified_Date not like '%0000%' " +
                    "then convert(datetime,substring({0}.Modified_Date, 1, 4) + '-' + substring({0}.Modified_Date, 5, 2) + '-' + substring({0}.Modified_Date, 7, 2) " +
                    "+ ' ' + substring({0}.Modified_Time, 1, 2) + ':' + substring({0}.Modified_Time, 3, 2) + ':' + substring({0}.Modified_Time, 5, 2)) else null end Modified_Date",
                    nameField);
            }
            return result;
        }

        public string CreateQueryFromSource(int page, int pageSize, int tenantID = 0, bool isWriteQueue = false)
        {
            var res = Map.Keys.Aggregate("select ", (current, key) => current + (ReplaceKeySpecial(key, tenantID) + ",\n"));
            res = string.Format(res + "ROW_NUMBER() over ({0}) as rownum",
                !string.IsNullOrEmpty(Source.OrderBy) ? Source.OrderBy : "");
            res = res.TrimEnd(',');
            res += " from " + Source.Tables;

            var acronymTable = Source.Tables.Split(' ').LastOrDefault() ?? Guid.NewGuid().ToString();

            if (!string.IsNullOrEmpty(Source.JoinTotal))
            {
                res += " " + Source.JoinTotal;
            }
            //JoinUser example: Contact_Manager,Created_By,Modified_By
            if (!string.IsNullOrEmpty(Source.JoinUser))
            {
                var lstFieldUser = Source.JoinUser.Split(',').ToList();
                foreach (var fieldUser in lstFieldUser)
                {
                    /*res += string.Format("\nleft join [adaptv3system].[dbo].[users] {0} on {0}.User_ID = {1}.{2}", 
                        ParseData.GetAcronymString(fieldUser), acronymTable, fieldUser);
                    res +=
                        string.Format(
                            "\nleft join  [tempdb].[dbo].[MigrateSupport] ms_{0} on ms_{0}.TargetServer='.' and ms_{0}.OldValTable = {0}.Email_Address",
                            ParseData.GetAcronymString(fieldUser));*/
                    res += string.Format("\nouter apply (select top 1 * from [adaptv3system].[dbo].[users] {0} where {0}.User_ID = {1}.{2}) as {0}", 
                        ParseData.GetAcronymString(fieldUser), acronymTable, fieldUser);
                    res +=
                        string.Format(
                            "\nouter apply (select top 1 * from [tempdb].[dbo].[MigrateSupport] ms_{0} where ms_{0}.TargetServer='.' and ms_{0}.OldValTable = {0}.Email_Address) ms_{0}",
                            ParseData.GetAcronymString(fieldUser));
                }
            }

            if (!string.IsNullOrEmpty(Source.Where))
            {
                Source.Where = string.Format(Source.Where);
                Source.Where = Source.Where.Contains("()") ? "" : Source.Where;
                res += Source.Where == "" ? "" : " where " + Source.Where;
            }
            if (!isWriteQueue)
            {
                res = string.Format("select * from({0}) t where {1} t.rownum >{2} and t.rownum <={3}", res,
                !string.IsNullOrEmpty(WhereGlobal) ? WhereGlobal + " and " : "",
                page * pageSize, (page + 1) * pageSize);
                return res;
            }
            return res;
        }

        public string CreateQueryFromDestination(int tenantID = 0)
        {
            var res = MapKeys.Keys.Aggregate("select top " + Destination.Top + " ", (current, key) => current + (ReplaceKeySpecial(key,tenantID) + ","));
            res = res.TrimEnd(',');
            res += " from " + Destination.Tables;
            
            if (!string.IsNullOrEmpty(Destination.Where))
            {
                res += " where " + Destination.Where;
                if (Destination.Where.Contains("TenantID"))
                {
                    res = string.Format(res, tenantID);
                }
            }

            if (!string.IsNullOrEmpty(Destination.OrderBy))
            {
                res += Destination.OrderBy;
            }
            return res;
        }

    }
}
