using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Migration;

namespace SysproMigration.Controllers
{
    public class DownloadController : Controller
    {
        //
        // GET: /Download/

        public ActionResult Index()
        {
            return View();
        }

        public FileResult ToolExportFileNew(string title, string fileName)
        {
            var fileFolder = ConfigurationManager.AppSettings["FolderUpload"] + Constants.CustomerLog;
            var isExists = Directory.Exists(Server.MapPath(fileFolder));
            if (!isExists)
                Directory.CreateDirectory(Server.MapPath(fileFolder));

            FileInfo fileInfo = new FileInfo(Path.Combine(Server.MapPath(fileFolder), fileName));

            if (fileInfo.Exists)
            {
                byte[] file = System.IO.File.ReadAllBytes(Path.Combine(Server.MapPath(fileFolder), fileName));
                //fileInfo.Delete();
                return File(file, "text/plain", title + ".txt");
            }
            return null;
        }

    }
}
