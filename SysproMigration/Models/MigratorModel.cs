using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Hosting;
using System.Web.Security;
using log4net;
using Migration;
using Migration.Common;
using Migration.Enums;
using Newtonsoft.Json;
using Syspro.Core.Helper.Logging;
using SysproMigration.Utility;

namespace SysproMigration.Models
{
    /// <summary>
    /// delegate for event
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void MigrationProcessBeginHandler(object sender, MigrationEventArgs e);
    public delegate void MigrationProcessEndHandler(object sender, MigrationEventArgs e);
    public delegate void TableMigratingHandler(object sender, MigrationEventArgs e);
    public delegate void TableMigratedHandler(object sender, MigrationEventArgs e);
    public delegate void PackageMigratingHandler(object sender, MigrationEventArgs e);
    public delegate void PackageMigratedHandler(object sender, MigrationEventArgs e);
    public delegate void FixingHandler(object sender, MigrationEventArgs e);
    public delegate void FixedHanlder(object sender, MigrationEventArgs e);
    public delegate void ErrorHandler(object sender, MigrationEventArgs e);

    /// <summary>
    /// Migration class
    /// </summary>
    public class MigratorModel
    {
        private const string TRUNCATE_TEMPLATE = "truncate table {0}";

        private int _batchSize;
        private bool _running;

        public bool Running
        {
            get { return _running; }
            set { _running = value; }
        }

        public bool Error { get; set; }

        public bool Truncatable { get; set; }

        public MigratorStatus MigrateStatus { get; set; }

        private List<FieldsMap> _fieldsMapsSetupData;

        public List<FieldsMap> FieldsMapsSetupData
        {
            get { return _fieldsMapsSetupData; }
            set { _fieldsMapsSetupData = value; }
        }
        private List<FieldsMap> _fieldsMapsRecordData;

        public List<FieldsMap> FieldsMapsRecordData
        {
            get { return _fieldsMapsRecordData; }
            set { _fieldsMapsRecordData = value; }
        }
        private List<FieldsMap> _fieldsMapsUpdateLastest;

        public List<FieldsMap> FieldsMapsUpdateLastest
        {
            get { return _fieldsMapsUpdateLastest; }
            set { _fieldsMapsUpdateLastest = value; }
        }

        public short IsMigrateSetup { get; set; }

        public List<FieldsMap> FieldsMaps
        {
            get
            {
                var result = new List<FieldsMap>();
                if (_fieldsMapsSetupData != null && _fieldsMapsSetupData.Any() && IsMigrateSetup > 0)
                {
                    result.AddRange(_fieldsMapsSetupData);
                }
                if (_fieldsMapsRecordData != null && _fieldsMapsRecordData.Any())
                {
                    result.AddRange(_fieldsMapsRecordData);
                }
                if (_fieldsMapsUpdateLastest != null && _fieldsMapsUpdateLastest.Any())
                {
                    result.AddRange(_fieldsMapsUpdateLastest);
                }
                return result;
            }
        }

        private string _soureConnectionString;

        public string SourceConnectionString
        {
            get { return _soureConnectionString; }
            set { _soureConnectionString = value; }
        }
        private string _destinationConnectionString;

        public string DestinationConnectionString
        {
            get { return _destinationConnectionString; }
            set { _destinationConnectionString = value; }
        }

        public string TenantIdList { get; set; }

        public bool Success { get; set; }

        public string SourceUserAdapt { get; set; }

        public string SourcePassAdapt { get; set; }

        public string SourceDbSystemAdapt { get; set; }

        public string DestinationServer { get; set; }

        public string SystemDbNewCrm { get; set; }

        public string DestinationDb { get; set; }

        /// <summary>
        /// events
        /// </summary>
        public event MigrationProcessBeginHandler MigrationProcessBegin;
        public event MigrationProcessEndHandler MigrationProcessEnd;
        public event TableMigratingHandler TableMigrating;
        public event TableMigratedHandler TableMigrated;
        public event PackageMigratingHandler PackageMigrating;
        public event PackageMigratedHandler PackageMigrated;
        public event FixingHandler Fixing;
        public event FixedHanlder Fixed;
        public event ErrorHandler MigrationError;

        //true if migrate only specifict tables
        public bool MigrateSpec { get; set; }

        //the specific tables that migrated only
        public List<string> SpecTables { get; set; }

        private string _migratedFilePath;
        private string _migratedFileFolder;

        public int TableMigratedCount { get; set; }

        public int TenantID { get; set; }

        public int DatabaseID { get; set; }

        public string MigrationConnectionString { get; set; }

        public bool isMigrateCustomData { get; set; }

        public string TimeZoneString { get; set; }

        public string TimeZone { get; set; }

        public MigratorModel()
        {
            Init();
        }

        /// <summary>
        /// read config, init parameters
        /// read fieldsMapping.json file
        /// </summary>
        private void Init()
        {
            _batchSize = Convert.ToInt32(ConfigurationManager.AppSettings[Constants.BatchSizeConfig]);
            //_soureConnectionString = ConfigurationManager.ConnectionStrings[Constants.SourceConfig].ConnectionString;
            //_destinationConnectionString = ConfigurationManager.ConnectionStrings[Constants.DestinationConfig].ConnectionString;
            //_migratedFileFolder = Path.Combine(HostingEnvironment.ApplicationPhysicalPath, ConfigurationManager.AppSettings["Server"]);
            //_migratedFilePath = Path.Combine(_migratedFileFolder, ConfigurationManager.AppSettings["sspMigratedRestFile"]);

            IsMigrateSetup = ParseData.GetShort(ConfigurationManager.AppSettings[Constants.IsMigrateSetup]) ?? 0;
            SourceUserAdapt = ConfigurationManager.AppSettings[Constants.UserSqlAdapt];
            SourcePassAdapt = ConfigurationManager.AppSettings[Constants.PassSqlAdapt];
            SourceDbSystemAdapt = ConfigurationManager.AppSettings[Constants.SystemDbAdapt];

            DestinationServer = ConfigurationManager.AppSettings[Constants.DestinationServer];
            SystemDbNewCrm = ConfigurationManager.AppSettings[Constants.SystemDbNewCrm];
            MigrationConnectionString = ConfigurationManager.ConnectionStrings[Constants.MigrationConnectionConfig].ConnectionString;
            LoadFieldsMap();

            SpecTables = new List<string>();
        }

        public void LoadFieldsMap()
        {
            //read db fields map setup data
            using (
                var sr =
                    new StreamReader(Path.Combine(HostingEnvironment.ApplicationPhysicalPath,
                        ConfigurationManager.AppSettings[Constants.FieldsMapping_SetupData])))
            {
                var mapString = sr.ReadToEnd();
                _fieldsMapsSetupData = JsonConvert.DeserializeObject<List<FieldsMap>>(mapString) ??
                                     new List<FieldsMap>();
            }
            //read db fields map record data
            using (
                var sr =
                    new StreamReader(Path.Combine(HostingEnvironment.ApplicationPhysicalPath,
                        ConfigurationManager.AppSettings[Constants.FieldsMapping_RecordData])))
            {
                var mapString = sr.ReadToEnd();
                _fieldsMapsRecordData = JsonConvert.DeserializeObject<List<FieldsMap>>(mapString) ??
                                    new List<FieldsMap>();
            }

            //read db fields map update lastest data
            if (isMigrateCustomData)
            {
                using (
                    var sr =
                        new StreamReader(Path.Combine(HostingEnvironment.ApplicationPhysicalPath,
                            ConfigurationManager.AppSettings[Constants.FieldsMapping_UpdateLastest])))
                {
                    var mapString = sr.ReadToEnd();
                    _fieldsMapsUpdateLastest = JsonConvert.DeserializeObject<List<FieldsMap>>(mapString) ??
                                        new List<FieldsMap>();
                }
            }
        }

        /// <summary>
        /// execute the migration process
        /// iterate the fieldsMaps, query the source and bulkCopy t des
        /// </summary>
        public void Run()
        {
            MigrateStatus = MigratorStatus.Running;

            var start = DateTime.Now;

            Logging.PushInfo("=======================================================================\n\nStart Migrating at : " + start.ToString(CultureInfo.InvariantCulture));

            _running = true;
            try
            {
                //raise migration process begin event
                if (MigrationProcessBegin != null)
                {
                    TableMigratedCount = 0;
                    MigrationProcessBegin(this, new MigrationEventArgs() { Message = "Migration process begin..." });
                }

                // Disable all triggers
                /*using (var conn = new SqlConnection(_destinationConnectionString))
                {
                    Logging.PushInfo("Start Disable all triggers");
                    conn.Open();
                    const string disableTriggerSql = "sp_MSforeachtable 'alter table ? disable trigger all'";
                    Logging.PushInfo("Command : " + disableTriggerSql);
                    var disableTriggerCmd = new SqlCommand(disableTriggerSql, conn);
                    disableTriggerCmd.CommandTimeout = Int32.MaxValue;
                    disableTriggerCmd.ExecuteNonQuery();

                    Logging.PushInfo("End Disable all triggers");
                }*/

                //open connections
                var sourceConn = new SqlConnection(_soureConnectionString);

                sourceConn.Open();

                Utils.CreateIndexTableAdapt(sourceConn);

                Utils.CreateSupportTempDb(sourceConn);


                var desConn = new SqlConnection(_destinationConnectionString);

                desConn.Open();

                Utils.CreateUniqueIndex(desConn);

                var usersAdapt = Utils.GetListUserAdapt(sourceConn);

                //var listEmailDuplicate = new List<string>();

                if (usersAdapt.Any())
                {
                    foreach (var userAdapt in usersAdapt)
                    {
                        var isValid = true;//Utils.CheckExistEmail(desConn, TenantID, userAdapt.Email);
                        if (isValid)
                        {
                            MembershipCreateStatus createStatus;
                            var user = Membership.CreateUser(userAdapt.Email,
                                ConfigurationManager.AppSettings["DefaultPasswordNewUser"], userAdapt.Email,
                                Constants.PasswordQuestion,
                                Constants.PasswordAnswer, true, null, out createStatus);
                            if (createStatus == MembershipCreateStatus.Success)
                            {
                                if (user != null)
                                {
                                    var query = string.Format(QueryConstants.QueryInsertUserTempdb,
                                        SourceDbSystemAdapt,
                                        "[dbo].[users]", DestinationServer, "st_Security", "[dbo].[sec_User]",
                                        userAdapt.Email, ParseData.GetGuid(user.ProviderUserKey), TenantID, DatabaseID);
                                    using (SqlCommand command = new SqlCommand(query, sourceConn))
                                    {
                                        //Logging.PushInfo(query);
                                        command.ExecuteNonQuery();
                                    }
                                }
                            }
                            else if (createStatus == MembershipCreateStatus.DuplicateEmail ||
                                     createStatus == MembershipCreateStatus.DuplicateUserName)
                            {
                                //listEmailDuplicate.Add("'" + userAdapt.Email + "'");
                            }
                        }
                    }
                }

                if (desConn.State == ConnectionState.Open)
                {
                    desConn.Close();
                    desConn.Dispose();
                }

                //insert query to queue table


                Logging.PushInfo("===============Start Insert Query To Queue Table===================");

                Logging.PushInfo("Enable identity insert on table [SysproMigration].[dbo].[QueueMigrate]");

                var migrationConn1 = MigrationConnectionString.CreateAndOpenConnection("Syspro Migration");

                var enableIdentityInsert = new SqlCommand("SET IDENTITY_INSERT [SysproMigration].[dbo].[QueueMigrate] ON", migrationConn1);
                enableIdentityInsert.ExecuteNonQuery();

                migrationConn1.CloseConnection();

                foreach (var fieldsMap in FieldsMaps)
                {
                    InsertQueryToQueue(fieldsMap, MigrationConnectionString);
                }

                Logging.PushInfo("===============End Insert Query To Queue Table===================");

                Logging.PushInfo("===============Start Read Query from Query Table===================");

                var migrationConn = MigrationConnectionString.CreateAndOpenConnection("Syspro Migration");

                var sourceConnObject = SourceConnectionString.GetObjectConnection();
                var desConnObject = DestinationConnectionString.GetObjectConnection();

                var objectQueue = new QueueMigrate
                {
                    SourceServerName = sourceConnObject.DataSource,
                    SourceDatabaseCompany = sourceConnObject.InitialCatalog,

                    TargetServerName = desConnObject.DataSource,
                    TargetDatabaseCompany = desConnObject.InitialCatalog
                };

                var lstQueue = Utils.GetQueueMigrates(migrationConn, objectQueue);

                migrationConn.CloseConnection();

                foreach (var queue in lstQueue)
                {
                    var fieldmap = FieldsMaps.FirstOrDefault(t => t.Id == queue.FieldsMapId);
                    MigrateFromQueue(queue, fieldmap);
                }

               /* //iterate the fieldsMaps and migrate every tables
                foreach (var fieldsMap in _fieldsMapsCompany)
                {
                    MigrateNew(fieldsMap, _destinationConnectionString, sourceConn,
                        listEmailDuplicate.Any() ? string.Join(",", listEmailDuplicate) : "");
                }*/



                //raise migration process end event
                if (MigrationProcessEnd != null)
                {
                    MigrationProcessEnd(this, new MigrationEventArgs() { Message = "Migration process done!!" });
                }

                desConn = DestinationConnectionString.CreateAndOpenConnection("Target");
                Utils.DropUniqueIndex(desConn);
                desConn.CloseConnection();

                sourceConn = SourceConnectionString.CreateAndOpenConnection("Source");
                Utils.DropIndexTableAdapt(sourceConn);
                sourceConn.CloseConnection();

                // Enable all triggers
                using (var conn = new SqlConnection(_destinationConnectionString))
                {
                    Logging.PushInfo("Start Enable all triggers");
                    conn.Open();
                    const string enableTriggerSql = "sp_MSforeachtable 'alter table ? enable trigger all'";
                    Logging.PushInfo("Command : " + enableTriggerSql);
                    var enableTriggerCmd = new SqlCommand(enableTriggerSql, conn);
                    enableTriggerCmd.ExecuteNonQuery();

                    Logging.PushInfo("End Enable all triggers");
                }

                //UpdateMigratedRestList(_migratedFilePath);
                Success = true;
                MigrateStatus = MigratorStatus.CompletedMigrate;
                var finish = DateTime.Now;
                Logging.PushInfo("=======================================================================\n\nFinish Migrating at : " + finish.ToString(CultureInfo.InvariantCulture));
                Logging.PushInfo("Process time : " + finish.Subtract(start));
            }
            catch (Exception e)
            {
                if (MigrationError != null)
                {
                    var evg = new MigrationEventArgs { Message = string.Format("{0}\n{1}", e.Message, e.StackTrace) };
                    MigrationError(this, evg);
                    Error = true;
                    Running = false;
                    Logging.PutError("MigrateError : ", e);
                }
                //log.Error("Run : ", e);
            }
            finally
            {
                _running = false;
            }
        }

        private void InsertQueryToQueue(FieldsMap fieldsMap, string migrateConnString)
        {
            SqlConnection migrateConn = null;
            SqlConnection sourceConn = null;
            SqlConnection desConn = null;
            var sourceConnObject = SourceConnectionString.GetObjectConnection();
            var desConnObject = DestinationConnectionString.GetObjectConnection();
            try
            {
                sourceConn = SourceConnectionString.CreateAndOpenConnection("Source");

                desConn = DestinationConnectionString.CreateAndOpenConnection("Target");

                migrateConn = migrateConnString.CreateAndOpenConnection("Syspro Migration");

                if (fieldsMap.NeedTruncate)
                {
                    var sqlString = string.Format(TRUNCATE_TEMPLATE, fieldsMap.Destination.Tables);
                    var cmd = new SqlCommand(sqlString, desConn);
                    cmd.ExecuteNonQuery();

                    Logging.PushInfo("Truncate table " + fieldsMap.Destination.Tables);
                    Logging.PushInfo("Query : " + sqlString);
                }

                var p = 0;
                var sql = fieldsMap.CreateQueryFromSource(p, _batchSize, TimeZoneString, TimeZone, TenantID, true);

                var records = 1000;
                Logging.PushInfo("Write query from source table " + fieldsMap.Source.Tables);
                //insert query
                Utils.UpdateQueueTable(migrateConn, new QueueMigrate
                {
                    SourceServerName = sourceConnObject.DataSource,
                    SourceDatabaseCompany = sourceConnObject.InitialCatalog,
                    SourceTable = fieldsMap.Source.Tables,
                    TargetServerName = desConnObject.DataSource,
                    TargetDatabaseCompany = desConnObject.InitialCatalog,
                    TargetTable = fieldsMap.Destination.Tables,
                    SqlQuery = sql,
                    Status = (int) QueueStatusEnum.NotStart,
                    IsGetKeys = 0,
                    FieldsMapId = fieldsMap.Id,
                    IsLastRecord = 1,
                    Exception = ""
                });

                Logging.PushInfo("Writed");

                //get keys and insert to table migrate support

                if (fieldsMap.IsGetKeys)
                {
                    fieldsMap.Destination.Top = records.ToString(CultureInfo.InvariantCulture);
                    var doneGetKeys = false;
                    var sqlGetKeys = fieldsMap.CreateQueryFromDestination(TimeZoneString, TimeZone, TenantID);
                    var recordGetKeys = 0;
                    using (
                        var drGetKeys = QueryBlock(desConn, sqlGetKeys.QueryInsertQueue(), fieldsMap.IsGetKeys,
                            out doneGetKeys, out recordGetKeys)
                        )
                    {
                        Logging.PushInfo("Write query get keys");

                        //insert query
                        Utils.UpdateQueueTable(migrateConn, new QueueMigrate
                        {
                            SourceServerName = desConnObject.DataSource,
                            SourceDatabaseCompany = desConnObject.InitialCatalog,
                            SourceTable = fieldsMap.Destination.Tables,
                            TargetServerName = sourceConnObject.DataSource,
                            TargetDatabaseCompany = sourceConnObject.InitialCatalog,
                            TargetTable = fieldsMap.Source.Tables,
                            SqlQuery = sqlGetKeys,
                            Status = (int) QueueStatusEnum.NotStart,
                            IsGetKeys = 1,
                            FieldsMapId = fieldsMap.Id,
                            IsLastRecord = 0,
                            Exception = ""
                        });
                        drGetKeys.Close();
                        Logging.PushInfo("Writed get keys");
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.PutError("MigrateNew : ", ex);
                throw;
            }
            finally
            {
                sourceConn.CloseConnection();
                desConn.CloseConnection();
                migrateConn.CloseConnection();
                SqlConnection.ClearAllPools();
            }
        }

        private void MigrateFromQueue(QueueMigrate queue,FieldsMap fieldsMap)
        {
            SqlConnection sourceConn = null;
            SqlConnection desConn = null;
            SqlBulkCopy sbc = null;                //raise table migrating event
            try
            {
                sourceConn = SourceConnectionString.CreateAndOpenConnection("Source");

                desConn = DestinationConnectionString.CreateAndOpenConnection("Target");

                sbc = new SqlBulkCopy(queue.IsGetKeys > 0 ? SourceConnectionString : DestinationConnectionString,
                    SqlBulkCopyOptions.KeepIdentity)
                {
                    BatchSize = _batchSize,
                    BulkCopyTimeout = 0,
                    EnableStreaming = true
                };

                /*if (TableMigrating != null)
                {
                    TableMigrating(this,
                        new MigrationEventArgs()
                        {
                            Message =
                                (queue.IsLastRecord > 0 ? "Continue migrate table" : "Begin migrate table ") + (queue.IsGetKeys > 0 ? queue.TargetTable : queue.SourceTable)
                        });
                }*/

                //migrate table            
                sbc.DestinationTableName = queue.IsGetKeys > 0 ? Constants.MiggrateSupportTable : queue.TargetTable;
                var map = queue.IsGetKeys > 0
                    ? fieldsMap.MapKeys.ToArray<KeyValuePair<string, string>>()
                    : fieldsMap.Map.ToArray<KeyValuePair<string, string>>();
                sbc.ColumnMappings.Clear();
                for (var i = 0; i < map.Length; i++)
                {
                    sbc.ColumnMappings.Add(new SqlBulkCopyColumnMapping(i, map[i].Value));
                }
                var p = 0;
                var done = false; 

                var records = 0;
                while (!done)
                {
                    using (
                        var dr = QueryBlock(queue.IsGetKeys > 0 ? desConn : sourceConn,
                            queue.IsGetKeys > 0
                                ? queue.SqlQuery
                            : queue.SqlQuery.CreateQueryFromQueue(p, 
                                            fieldsMap.Size > 0 ? fieldsMap.Size : _batchSize, 
                                            fieldsMap.WhereGlobal)
                            , false,
                            out done,
                            out records))
                    {
                        done = queue.IsGetKeys > 0 || done;
                        Logging.PushInfo("Copying data to sqlserver");
                        sbc.WriteToServer(dr);
                        if (!string.IsNullOrEmpty(fieldsMap.Destination.Script))
                        {
                            desConn.CloseConnection();
                            desConn = DestinationConnectionString.CreateAndOpenConnection("Target With Script" + fieldsMap.Destination.Script);
                            Logging.PushInfo("\nStart run script at:"+DateTime.Now);
                            Utils.Execute(desConn, fieldsMap.Destination.Script.GetTextInQueryFixedFolder(), fieldsMap.Destination.TimeOut);
                            Logging.PushInfo("\nEnd Run script at:" + DateTime.Now);
                            desConn.CloseConnection();
                        }
                        dr.Close();
                        Logging.PushInfo("Copyied");
                    }
                    p++;
                }
                var migrationConn = MigrationConnectionString.CreateAndOpenConnection("Syspro Migration");
                queue.Status = (int)QueueStatusEnum.Success;
                Utils.UpdateQueueTable(migrationConn, queue);
                migrationConn.CloseConnection();
                //raise table migrated event
                if (TableMigrated != null && queue.IsLastRecord > 0)
                {
                    TableMigrated(this, new MigrationEventArgs() { Message = "Table " + fieldsMap.Destination.Tables + " migrated!" });
                    TableMigratedCount++;
                }
            }
            catch (Exception ex)
            {
                Logging.PutError("Migrate From Queue : ", ex);

                var migrationConn = MigrationConnectionString.CreateAndOpenConnection("Syspro Migration");
                queue.Status = (int)QueueStatusEnum.Error;
                queue.Exception = ex.ToString();
                Utils.UpdateQueueTable(migrationConn, queue);

                TableMigratedCount++;

                migrationConn.CloseConnection();
            }
            finally
            {
                var connStringAdmin =
                    string.Format(
                        ConfigurationManager.ConnectionStrings[Constants.AdminConnectionConfig].ConnectionString,
                        DestinationDb);
                const string strFeeBuffer = " DBCC DROPCLEANBUFFERS ";
                const string strFeeSession = " DBCC FREESESSIONCACHE WITH NO_INFOMSGS ";
                var connAdmin = connStringAdmin.CreateAndOpenConnection("Target Admin");

                var cmd = new SqlCommand(strFeeBuffer, connAdmin);
                cmd.ExecuteNonQuery();

                cmd = new SqlCommand(strFeeSession, connAdmin);
                cmd.ExecuteNonQuery();

                sourceConn.CloseConnection();
                desConn.CloseConnection();
                connAdmin.CloseConnection();
            }
        }

        private void MigrateNew(FieldsMap fieldsMap, string desconnectionString, SqlConnection sourceConn, string conditionWhere)
        {
            SqlBulkCopy sbc = null;                //raise table migrating event
            SqlBulkCopy sbcGetKeys = null;
            SqlConnection desConn = null;

            try
            {
                desConn = new SqlConnection(desconnectionString);
                Logging.PushInfo("Connecting to SQL Server");

                desConn.Open();
                Logging.PushInfo("Connected");



                if (fieldsMap.NeedTruncate)
                {
                    var sqlString = string.Format(TRUNCATE_TEMPLATE, fieldsMap.Destination.Tables);
                    var cmd = new SqlCommand(sqlString, desConn);
                    cmd.ExecuteNonQuery();

                    Logging.PushInfo("Truncate table " + fieldsMap.Destination.Tables);
                    Logging.PushInfo("Query : " + sqlString);
                }


                sbc = new SqlBulkCopy(desconnectionString, SqlBulkCopyOptions.KeepIdentity)
                {
                    BatchSize = _batchSize,
                    BulkCopyTimeout = 0,
                    EnableStreaming = true
                };

                if (TableMigrating != null)
                {
                    TableMigrating(this, new MigrationEventArgs() { Message = "Begin migrate table " + fieldsMap.Destination.Tables });
                }

                //migrate table            
                sbc.DestinationTableName = fieldsMap.Destination.Tables;
                var map = fieldsMap.Map.ToArray<KeyValuePair<string, string>>();
                sbc.ColumnMappings.Clear();
                for (var i = 0; i < map.Length; i++)
                {
                    sbc.ColumnMappings.Add(new SqlBulkCopyColumnMapping(i, map[i].Value));
                }
                var p = 0;
                var done = false;
                while (!done)
                {
                    var sql = fieldsMap.CreateQueryFromSource(p, _batchSize, TimeZoneString, TimeZone, TenantID);
                    if (PackageMigrating != null)
                    {
                        PackageMigrating(this,
                            new MigrationEventArgs()
                            {
                                Message = "Begin import package",
                                DestinationTable = fieldsMap.Destination.Tables,
                                Package = p + 1,
                                MigratedRecords = p * _batchSize
                            });
                    }

                    var records = 0;
                    using (var dr = QueryBlock(sourceConn, sql, fieldsMap.IsGetKeys, out done, out records))
                    {
                        Logging.PushInfo("Copying data to sqlserver");
                        sbc.WriteToServer(dr);

                        dr.Close();
                        Logging.PushInfo("Copyied");

                        //get keys and insert to table migrate support
                        if (fieldsMap.IsGetKeys)
                        {
                            fieldsMap.Destination.Top = records.ToString(CultureInfo.InvariantCulture);
                            sbcGetKeys = new SqlBulkCopy(sourceConn.ConnectionString, SqlBulkCopyOptions.KeepIdentity)
                            {
                                BatchSize = _batchSize,
                                BulkCopyTimeout = 0,
                                EnableStreaming = true
                            };
                            sbcGetKeys.DestinationTableName = Constants.MiggrateSupportTable;
                            var mapKeys = fieldsMap.MapKeys.ToArray();
                            sbcGetKeys.ColumnMappings.Clear();
                            for (var i = 0; i < mapKeys.Length; i++)
                            {
                                sbcGetKeys.ColumnMappings.Add(new SqlBulkCopyColumnMapping(i, mapKeys[i].Value));
                            }
                            var doneGetKeys = false;
                            var sqlGetKeys = fieldsMap.CreateQueryFromDestination(TimeZoneString, TimeZone, TenantID);
                            if (PackageMigrating != null)
                            {
                                PackageMigrating(this,
                                    new MigrationEventArgs()
                                    {
                                        Message = "Begin import package",
                                        DestinationTable = fieldsMap.Source.Tables,
                                        MigratedRecords = records
                                    });
                            }
                            var recordGetKeys = 0;
                            using (
                                var drGetKeys = QueryBlock(desConn, sqlGetKeys, fieldsMap.IsGetKeys, out doneGetKeys, out recordGetKeys)
                                )
                            {
                                Logging.PushInfo("Copying data to sqlserver");
                                sbcGetKeys.WriteToServer(drGetKeys);

                                drGetKeys.Close();
                                Logging.PushInfo("Copyied");
                            }
                        }
                    }

                    //raise package migrated event
                    if (PackageMigrated != null)
                    {
                        PackageMigrated(this, new MigrationEventArgs() { Message = "Begin import package", DestinationTable = fieldsMap.Destination.Tables, Package = p + 1, MigratedRecords = (p + 1) * _batchSize });
                    }
                    p++;
                }

                //raise table migrated event
                if (TableMigrated != null)
                {
                    TableMigrated(this, new MigrationEventArgs() { Message = "Table " + fieldsMap.Destination.Tables + " migrated!" });
                    TableMigratedCount++;
                }
            }
            catch (Exception ex)
            {
                Logging.PutError("MigrateNew : ", ex);
                throw;
            }
            finally
            {

                var connStringAdmin =
                    string.Format(
                        ConfigurationManager.ConnectionStrings[Constants.AdminConnectionConfig].ConnectionString,
                        DestinationDb);
                const string strFeeBuffer = " DBCC DROPCLEANBUFFERS ";
                const string strFeeSession = " DBCC FREESESSIONCACHE WITH NO_INFOMSGS ";
                var connAdmin = new SqlConnection(connStringAdmin);

                connAdmin.Open();

                var cmd = new SqlCommand(strFeeBuffer, connAdmin);
                cmd.ExecuteNonQuery();

                cmd = new SqlCommand(strFeeSession, connAdmin);
                cmd.ExecuteNonQuery();

                if (sbc != null)
                {
                    sbc.Close();
                    sbc = null;
                }
                if (sbcGetKeys != null)
                {
                    sbcGetKeys.Close();
                    sbcGetKeys = null;
                }

                if (desConn != null && desConn.State == ConnectionState.Open)
                {
                    desConn.Close();
                    desConn.Dispose();
                    desConn = null;
                }
                if (connAdmin.State == ConnectionState.Open)
                {
                    connAdmin.Close();
                    connAdmin.Dispose();
                    connAdmin = null;
                }
            }
        }

        /// <summary>
        /// Query a block of data
        /// </summary>
        /// <param name="conn">source connection</param>
        /// <param name="sql">sql query string</param>
        /// <param name="isGetKeys"></param>
        /// <param name="done"></param>
        /// <param name="records"></param>
        /// <returns></returns>
        private IDataReader QueryBlock(SqlConnection conn, string sql, bool isGetKeys, out bool done, out int records)
        {
            records = 0;
            var query = sql;
            Debug.WriteLine(query);
            var start = DateTime.Now;
            Logging.PushInfo("Start get data from sql source at : " + start);
            Logging.PushInfo(query);

            var cmd = new SqlCommand(query, conn) { CommandTimeout = int.MaxValue };

            var res = cmd.ExecuteReader();

            if (isGetKeys)
            {
                DataTable dt = new DataTable();
                dt.Load(res);
                records = dt.Rows.Count;
                res = null;
                res = cmd.ExecuteReader();
            }
            done = !res.HasRows;

            var end = DateTime.Now;
            Logging.PushInfo("End get data from sql source at : " + end);
            Logging.PushInfo("Finish Get Data from SQL source in " + end.Subtract(start));
            return res;
        }


    }
}
