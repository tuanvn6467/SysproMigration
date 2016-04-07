using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Migration.Common
{
    public class ParseData
    {
        public static short? GetShort(object oShort)
        {
            if (oShort == null)
            {
                return null;
            }
            try
            {
                return short.Parse(oShort.ToString());
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public static int? GetInt(object oInt)
        {
            if (oInt == null)
            {
                return null;
            }
            try
            {
                return int.Parse(oInt.ToString());
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static long? GetLong(object oLong)
        {
            if (oLong == null)
            {
                return null;
            }
            try
            {
                return long.Parse(oLong.ToString());
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static decimal GetDecimal(object oDecimal)
        {
            if (oDecimal == null)
            {
                return 0;
            }
            try
            {
                return decimal.Parse(oDecimal.ToString());
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        public static float GetFloat(object oFloat)
        {
            if (oFloat == null)
            {
                return 0;
            }
            try
            {
                return float.Parse(oFloat.ToString());
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        public static double? GetDouble(object oDouble)
        {
            if (oDouble == null)
            {
                return null;
            }
            try
            {
                return double.Parse(oDouble.ToString());
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static bool GetBool(object oBool)
        {
            if (oBool == null)
            {
                return false;
            }
            try
            {
                return GetInt(oBool) == 1 || bool.Parse(oBool.ToString());
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static string GetString(object oText)
        {
            if (oText == null)
            {
                return string.Empty;
            }
            try
            {
                return oText.ToString().Trim();
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        public static string Replace(object baseText, object replaceText, object byText)
        {
            if (baseText == null || replaceText == null || byText == null || baseText.ToString().Length == 0 || replaceText.ToString().Length == 0)
            {
                return string.Empty;
            }
            try
            {
                return baseText.ToString().Replace(replaceText.ToString(), byText.ToString());
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        public static DateTime? GetDateTime(object oDateTime)
        {

            if (oDateTime == null)
            {
                return null;
            }
            try
            {
                if (oDateTime.ToString().Contains("GMT"))
                {
                    oDateTime = oDateTime.ToString().Substring(0, 24);
                }
                return DateTime.Parse(GetString(oDateTime));
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public static DateTime? GetDateTime(object oDateTime, string format)
        {
            if (oDateTime == null)
            {
                return null;
            }
            try
            {
                if (oDateTime.ToString().Contains("GMT"))
                {
                    return Convert.ToDateTime(oDateTime.ToString().Substring(0, 24));
                }
                return DateTime.ParseExact(GetString(oDateTime), format, CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static DateTime? GetDateTime(long? oDateTime)
        {
            if (oDateTime == null)
            {
                return null;
            }
            try
            {
                DateTime start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                DateTime date = start.AddMilliseconds(oDateTime.Value).ToLocalTime();
                return date;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public static Guid? GetGuid(object oGuid)
        {
            if (oGuid == null)
            {
                return null;
            }
            try
            {
                return Guid.Parse(GetString(oGuid).ToUpper());
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public static DateTime StampToDateTime(long timestamp)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return origin.ToLocalTime().AddMilliseconds(timestamp);
        }
        public static long DateTimeToStamp(DateTime dateTime)
        {
            //create Timespan by subtracting the value provided from
            //the Unix Epoch
            TimeSpan span = (dateTime - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime());

            //return the total seconds (which is a UNIX timestamp)
            return (long)span.TotalMilliseconds;
        }

        public static byte? GetByte(object oByte)
        {
            if (oByte == null)
            {
                return null;
            }
            try
            {
                return byte.Parse(oByte.ToString());
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static string GetAcronymString(string text)
        {
            return text.Replace("_", "").ToLower();
        }
        //replace acc.UserId = UserId
        /*public static string GetRealField(string text)
        {
            var indexCham = text.LastIndexOf(".");
            return text.Substring(indexCham >= 0 ? indexCham + 1 : 0, text.Length - indexCham - 1);
        }*/

        private static byte[] HexStringToBytes(string hexString)
        {
            if (hexString == null)
                throw new ArgumentNullException("hexString");
            if (hexString.Length % 2 != 0)
                throw new ArgumentException("hexString must have an even length", "hexString");
            var bytes = new byte[hexString.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                string currentHex = hexString.Substring(i * 2, 2);
                bytes[i] = Convert.ToByte(currentHex, 16);
            }
            return bytes;
        }

        public static string HexStringToString(byte[] bytes)
        {
            return Encoding.GetEncoding("ISO-8859-1").GetString(bytes);
        }
    }
}
