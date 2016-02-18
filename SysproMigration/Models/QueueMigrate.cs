using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SysproMigration.Models
{
    public class QueueMigrate
    {
        public int Id { get; set; }
        public string SourceServerName { get; set; }
        public string SourceDatabaseCompany { get; set; }
        public string SourceTable { get; set; }
        public string TargetServerName { get; set; }
        public string TargetDatabaseCompany { get; set; }
        public string TargetTable { get; set; }
        public string SqlQuery { get; set; }
        public short? IsLastRecord { get; set; }
        public short? IsGetKeys { get; set; }
        public int FieldsMapId { get; set; }
        public short? Status { get; set; }
        public string Exception { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}