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
        }

        public Source Source { get; set; }

        public Source Destination { get; set; }

        public Dictionary<string, string> Map { get; set; }
        
        public bool NeedTruncate { get; set; }

        public bool IsGetKeys { get; set; }

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
                        "convert(datetime,substring(Created_Date, 1, 4) + '-' + substring(Created_Date, 5, 2) + '-' + substring(Created_Date, 7, 2) + ' ' + substring(Created_Time, 1, 2) + ':' + substring(Created_Time, 3, 2) + ':' + substring(Created_Time, 5, 2)) Created_Date";
                    break;
                case "Modified_Date":
                    result =
                        "convert(datetime,substring(Modified_Date, 1, 4) + '-' + substring(Modified_Date, 5, 2) + '-' + substring(Modified_Date, 7, 2) + ' ' + substring(Modified_Time, 1, 2) + ':' + substring(Modified_Time, 3, 2) + ':' + substring(Modified_Time, 5, 2)) Modified_Date";
                    break;
                /*case "Syspro_TenantID":
                    result = string.Format("convert(int, {0}) TenantID", tenantID);
                    break;*/
            }
            if (result.Contains("Syspro_TenantID"))
            {
                result = result.Replace("Syspro_TenantID", string.Format("convert(int, {0})", tenantID));
            }
            return result;
        }

        public string CreateQueryFromSource(int page, int pageSize, string conditionWhere = "", int tenantID = 0)
        {
            var res = Map.Keys.Aggregate("select ", (current, key) => current + (ReplaceKeySpecial(key, tenantID) + ","));
            res = string.Format(res + "ROW_NUMBER() over ({0}) as rownum",
                !string.IsNullOrEmpty(Source.OrderBy) ? Source.OrderBy : "");
            res = res.TrimEnd(',');
            res += " from " + Source.Tables;

            /*if (!string.IsNullOrEmpty(Source.Join))
            {
                res += " " + Source.Join;
                if (!string.IsNullOrEmpty(tenantIdList))
                {
                    res = string.Format(res, tenantIdList);
                }
            }*/

            if (!string.IsNullOrEmpty(Source.Where))
            {
                Source.Where = string.Format(Source.Where, conditionWhere);
                Source.Where = Source.Where.Contains("()") ? "" : Source.Where;
                res += Source.Where == "" ? "" : " where " + Source.Where;
            }
            res = string.Format("select * from({0}) t where t.rownum >{1} and t.rownum <={2}", res, page*pageSize,
                (page + 1)*pageSize);
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
