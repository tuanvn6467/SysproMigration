using System;
using System.Configuration;
using System.Web;
using log4net;

namespace Syspro.Core.Helper.Logging
{
    public static class Logging
    {
        //private static readonly ILog Log = LogManager.GetLogger("log4netLogger");
        private static readonly ILog Log = LogManager.GetLogger("ForAllApplication");

        public static void PutError(string message, Exception e)
        {
            if (HttpContext.Current != null)
            {
                message = message + "; Url: " + HttpContext.Current.Request.Url.AbsoluteUri + "; Error: ";
            }
            else
            {
                message = message + "; Error: ";
            }
            Log.Error(message, e);

        }
        public static void PushString(string message)
        {
            Log.Error(message);
        }

        public static void PushInfo(string message)
        {
            Log.Info(message);
        }
        // Other Custom Logging Functions

    }
}
