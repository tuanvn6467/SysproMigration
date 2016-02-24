using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Xml;

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

        public string ReplaceKeySpecial(string key, int tenantID = 0)
        {
            var result = key;
            switch (key)
            {
                case "Created_By":
                    result =
                        "(select top 1 NewValTable from [tempdb].[dbo].[MigrateSupport] where TargetServer='.' and OldValTable = (select Email_Address from [adaptv3system].[dbo].[users] where User_ID = Created_By) order by Id desc) Created_By";
                    break;
                case "Modified_By":
                    result =
                        "(select top 1 NewValTable from [tempdb].[dbo].[MigrateSupport] where TargetServer='.' and OldValTable = (select Email_Address from [adaptv3system].[dbo].[users] where User_ID = Modified_By) order by Id desc) Modified_By";
                    break;
                case "Created_Date":
                    result =
                        "case when Created_Date not like '%0000%' then convert(datetime,substring(Created_Date, 1, 4) + '-' + substring(Created_Date, 5, 2) + '-' + substring(Created_Date, 7, 2) + ' ' + substring(Created_Time, 1, 2) + ':' + substring(Created_Time, 3, 2) + ':' + substring(Created_Time, 5, 2)) else null end Created_Date";
                    break;
                case "Modified_Date":
                    result =
                        "case when Modified_Date not like '%0000%' then convert(datetime,substring(Modified_Date, 1, 4) + '-' + substring(Modified_Date, 5, 2) + '-' + substring(Modified_Date, 7, 2) + ' ' + substring(Modified_Time, 1, 2) + ':' + substring(Modified_Time, 3, 2) + ':' + substring(Modified_Time, 5, 2)) else null end Modified_Date";
                    break;
                case "Notes":
                    result =
                        "case when Notes is not null then RTRIM(LTRIM(Notes)) else null end Notes";
                    break;
                case "Notes_Text":
                    result =
                        "Notes";
                    break;
                    /*case "Syspro_TenantID":
                    result = string.Format("convert(int, {0}) TenantID", tenantID);
                    break;*/
            }
            if (result.Contains("Syspro_TenantID"))
            {
                result = result.Replace("Syspro_TenantID", string.Format("convert(int, {0})", tenantID));
            }else if (result.Contains("_Syspro_TF"))
            {
                var fieldName = result.Replace("_Syspro_TF", "");
                result = string.Format("case when {0} = 'T' then 1 else 0 end {0}", fieldName);
            }
            else if (result.Contains("_Syspro_GetUserID"))
            {
                var userField = result.Replace("_Syspro_GetUserID", "");
                result = string.Format("(select top 1 NewValTable from [tempdb].[dbo].[MigrateSupport] where TargetServer='.' and OldValTable = (select Email_Address from [adaptv3system].[dbo].[users] where User_ID = {0}) order by Id desc) {0}", userField);
            }
            else if (result.Contains("_Syspro_Address"))
            {
                var nameField = result.Replace("_Syspro_Address", "");
                result = string.Format("(select top 1 {0} from [dbo].[address] where Attached_To = 'A' and Account_Number = Account_Number and Address_Record_Number = Primary_Address_Record) {0}", nameField);
            }
            return result;
        }

        public string CreateQueryFromSource(int page, int pageSize, int tenantID = 0)
        {
            var res = Map.Keys.Aggregate("select ", (current, key) => current + (ReplaceKeySpecial(key, tenantID) + ","));
            res = string.Format(res + "ROW_NUMBER() over ({0}) as rownum",
                !string.IsNullOrEmpty(Source.OrderBy) ? Source.OrderBy : "");
            res = res.TrimEnd(',');
            res += " from " + Source.Tables;

            if (!string.IsNullOrEmpty(Source.Join))
            {
                res += " " + Source.Join;
            }

            if (!string.IsNullOrEmpty(Source.Where))
            {
                Source.Where = string.Format(Source.Where);
                Source.Where = Source.Where.Contains("()") ? "" : Source.Where;
                res += Source.Where == "" ? "" : " where " + Source.Where;
            }
            res = string.Format("select * from({0}) t where {1} t.rownum >{2} and t.rownum <={3}", res,
                !string.IsNullOrEmpty(WhereGlobal) ? WhereGlobal + " and " : "",
                page*pageSize, (page + 1)*pageSize);
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
