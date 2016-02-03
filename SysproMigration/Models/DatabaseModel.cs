using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SysproMigration.Models
{
    public class DatabaseModel
    {
        public int DatabaseID { get; set; }
        public string DatabaseName { get; set; }
        public string PhysicalName { get; set; }
        public int TenantID { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}