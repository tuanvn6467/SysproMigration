using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using log4net;
using Migration;
using Newtonsoft.Json;
using Syspro.Core.Helper.Logging;
using SysproMigration.Models;
using SysproMigration.Utility;

namespace SysproMigration.Controllers
{
    public class MigrationController : Controller
    {
        private static string _log;
        private static MigratorModel _migrator = new MigratorModel();
        static DateTime _beginTime;
        static DateTime _endTime;

        //
        // GET: /Migration/

        public ActionResult Index()
        {
            //SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(_migrator.DestinationConnectionString);
            /*var lstDataBase = Utils.GetListDatabase(builder.DataSource, builder.UserID, builder.Password);
            ViewBag.LstDatabase = lstDataBase;*/
            ViewBag.TableMigrateCount = _migrator.FieldsMapsCompany.Count() + _migrator.FieldsMapsSystem.Count() +
                                        _migrator.FieldsMapsSecurity.Count();
            return View();
        }

        public async Task<ActionResult> MigrateData(string sourceSQLServerName, string sourceDb, string targetDb, int tennantID, int databaseID, string userName, string userPassword, bool convertAgain)
        {
            try
            {
                //check is migrated 
                if (!convertAgain)
                {
                    var titleFile = string.Format(Constants.LogFileFormat, sourceDb, targetDb, sourceSQLServerName.ReplaceSpecialCharacter());
                    var logFolder = ConfigurationManager.AppSettings["FolderUpload"] + Constants.CustomerLog;
                    if (!Directory.Exists(Server.MapPath(logFolder)))
                    {
                        Directory.CreateDirectory(Server.MapPath(logFolder));
                    }
                    var files = Directory.GetFiles(Server.MapPath(logFolder));
                    if (files.Any(t => t.Contains(titleFile)))
                    {
                        return Json(new { Status = -1, IsMigrated = true }, JsonRequestBehavior.AllowGet);
                    }    
                }
                //end check
                var sourceConn = string.Format(Constants.DbConnection, sourceSQLServerName,
                    string.Format(Constants.AdaptDbPrefix + "{0}", sourceDb), _migrator.SourceUserAdapt,
                    _migrator.SourcePassAdapt);
                var destinationConn = string.Format(Constants.DbConnection, _migrator.DestinationServer, targetDb,
                    userName, userPassword);
                if (_migrator.Running)
                {
                    throw new Exception("Migrator is running. Only one Migrator is ran at time.");
                }
                _migrator.Success = false;
                _migrator.MigrationProcessBegin += _migrator_MigrationProcessBegin;
                _migrator.MigrationProcessEnd += _migrator_MigrationProcessEnd;
                _migrator.PackageMigrated += _migrator_PackageMigrated;
                _migrator.PackageMigrating += _migrator_PackageMigrating;
                _migrator.TableMigrated += _migrator_TableMigrated;
                _migrator.TableMigrating += _migrator_TableMigrating;
                _migrator.Fixing += _migrator_Fixing;
                _migrator.Fixed += _migrator_Fixed;
                _migrator.MigrationError += _migrator_MigrationError;
                
                _migrator.LoadFieldsMap();

                _migrator.SourceConnectionString = sourceConn;
                _migrator.DestinationConnectionString = destinationConn;
                _migrator.DestinationDb = targetDb;
                _migrator.TenantID = tennantID;
                _migrator.DatabaseID = databaseID;
                _log = string.Empty;
                _beginTime = DateTime.Now;
                var thread = new Thread(_migrator.Run);
                thread.Start();
                //_migrator.Run();
                _endTime = DateTime.Now;
            }
            catch (Exception ex)
            {
                //_log += ex.Message + "\n" + ex.StackTrace;
                Logging.PutError(_log, ex);
                return Json(new {Status = 0, Message = ex.Message}, JsonRequestBehavior.AllowGet);
            }

            return Json(new {Status = 1, Message = "Running"}, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetListCompanySource(string sqlServerName)
        {
            var lstCompany = Utils.GetListCompany(sqlServerName, _migrator.SourceDbSystemAdapt,
                _migrator.SourceUserAdapt,
                _migrator.SourcePassAdapt);
            return Json(new { lstCompany = lstCompany });
        }

        [HttpPost]
        public ActionResult GetListCompanyDestination(string user, string password)
        {
            var lstCompany = Utils.GetListCompany(_migrator.DestinationServer, _migrator.SystemDbNewCrm, user, password,
                false);
            return Json(new { lstCompany = lstCompany });
        }

        [HttpPost]
        public ActionResult CheckExistDatabase(string dbSelected, string sqlServerName, bool fromSource = true,
            string physicalName = "")
        {
            var userModels = new List<User>();

            var lstDatabase = Utils.GetListDatabase(fromSource ? sqlServerName : _migrator.DestinationServer,
                _migrator.SourceUserAdapt, _migrator.SourcePassAdapt, fromSource, ref userModels);

            if (fromSource)
            {
                var userEmptyEmail = string.Empty;
                if (userModels.Any(t => string.IsNullOrEmpty(t.Email)))
                {
                    userEmptyEmail = string.Join(",",
                        userModels.Where(t => string.IsNullOrEmpty(t.Email)).Select(u => u.UserName));
                }
                if (!string.IsNullOrEmpty(userEmptyEmail.Trim()))
                {
                    return
                    Json(
                        new
                        {
                            IsHasEmptyEmail = true,
                            UserEmptyEmail = userEmptyEmail,
                            IsExist =
                                lstDatabase.Any(
                                    t =>
                                        t.DatabaseName.Equals(string.Format("{0}" + dbSelected, Constants.AdaptDbPrefix)))
                        });
                }
            }
            return
                Json(
                    new
                    {
                        IsExist =
                            lstDatabase.Any(
                                t =>
                                    t.DatabaseName.Equals(fromSource
                                        ? string.Format("{0}" + dbSelected, Constants.AdaptDbPrefix)
                                        : physicalName))
                    });
        }

        public ActionResult GetPercentageDone(string sourceDb, string targetDb, string serverName)
        {
            if (!_migrator.Running && _migrator.Success)
            {
                var logFolder = ConfigurationManager.AppSettings["FolderUpload"] + Constants.CustomerLog;
                var isExists = Directory.Exists(Server.MapPath(logFolder));
                if (!isExists)
                    Directory.CreateDirectory(Server.MapPath(logFolder));
                var title = string.Format(Constants.LogFileFormat, sourceDb, targetDb, serverName.ReplaceSpecialCharacter());
                var nameFile = Utils.ConvertTitleToFileName(title) +
                               Guid.NewGuid().ToString() + ".txt";
                var physicalPath = Path.Combine(Server.MapPath(logFolder), nameFile);
                System.IO.File.WriteAllText(physicalPath, _log);
                return Json(new
                {
                    TableMigratedCount = _migrator.TableMigratedCount,
                    Success = !_migrator.Running && _migrator.Success,
                    filename = nameFile,
                    fileDownload = Utils.ConvertTitleToFileName(title),
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new
            {
                TableMigratedCount = _migrator.TableMigratedCount, Success = !_migrator.Running && _migrator.Success, Error = _migrator.Error
            }, JsonRequestBehavior.AllowGet);
        }

        /*public ActionResult GetLog()
        {
            return Json(new { Log = _log, _migrator.Running, Success = !_migrator.Running && _migrator.Success, _migrator.MigrateStatus }, JsonRequestBehavior.AllowGet);
        }*/
        #region private function
        private void _migrator_Fixed(object sender, MigrationEventArgs e)
        {
            _log += e.Message + "\n";
        }

        private void _migrator_Fixing(object sender, MigrationEventArgs e)
        {
            _log += "\n============================\n" + e.Message + "\n";
        }

        private void _migrator_TableMigrating(object sender, MigrationEventArgs e)
        {
            _log += string.Format("{0:dd/MM/yyyy HH:mm:ss} : {1}\n", DateTime.Now, e.Message);
        }

        private void _migrator_TableMigrated(object sender, MigrationEventArgs e)
        {
            _log += string.Format("{0:dd/MM/yyyy HH:mm:ss} : {1}\n", DateTime.Now, e.Message);
        }

        private void _migrator_PackageMigrating(object sender, MigrationEventArgs e)
        {
            _log += string.Format("{0:dd/MM/yyyy HH:mm:ss} : Begin import package {1}, {2} records imported\n", DateTime.Now, e.Package, e.MigratedRecords);
        }

        private void _migrator_PackageMigrated(object sender, MigrationEventArgs e)
        {
            _log += string.Format("{0:dd/MM/yyyy HH:mm:ss} : End import package {1}, {2} records imported\n", DateTime.Now, e.Package, e.MigratedRecords);
        }

        private void _migrator_MigrationProcessEnd(object sender, MigrationEventArgs e)
        {
            _log += string.Format("{0:dd/MM/yyyy HH:mm:ss} : {1}\n", _endTime, e.Message);
            _log += string.Format("Total time: {0:hh\\:mm\\:ss}\n", (_endTime - _beginTime));
            _log += "\n======================================================================\n";
            _endTime = DateTime.Now;
            _migrator.MigrationProcessBegin -= _migrator_MigrationProcessBegin;
            _migrator.MigrationProcessEnd -= _migrator_MigrationProcessEnd;
            _migrator.PackageMigrated -= _migrator_PackageMigrated;
            _migrator.PackageMigrating -= _migrator_PackageMigrating;
            _migrator.TableMigrated -= _migrator_TableMigrated;
            _migrator.TableMigrating -= _migrator_TableMigrating;
            _migrator.Fixing -= _migrator_Fixing;
            _migrator.Fixed -= _migrator_Fixed;
            _migrator.MigrationError -= _migrator_MigrationError;
            _migrator.Running = false;
            _migrator.Success = true;
        }

        private void _migrator_MigrationProcessBegin(object sender, MigrationEventArgs e)
        {
            _log += string.Format("{0:dd/MM/yyyy HH:mm:ss} : {1}\n", _beginTime, e.Message);
        }

        private void _migrator_MigrationError(object sender, MigrationEventArgs e)
        {
            _migrator.MigrationProcessBegin -= _migrator_MigrationProcessBegin;
            _migrator.MigrationProcessEnd -= _migrator_MigrationProcessEnd;
            _migrator.PackageMigrated -= _migrator_PackageMigrated;
            _migrator.PackageMigrating -= _migrator_PackageMigrating;
            _migrator.TableMigrated -= _migrator_TableMigrated;
            _migrator.TableMigrating -= _migrator_TableMigrating;
            _migrator.Fixing -= _migrator_Fixing;
            _migrator.Fixed -= _migrator_Fixed;
            _migrator.MigrationError -= _migrator_MigrationError;
            _migrator.Running = false;
            _migrator.Error = true;
            _log += e.Message;
        }
        #endregion
    }
}
