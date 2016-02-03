using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Migration.Enums
{
    public enum MigratorStatus
    {
        Running = 0,
        CompletedMigrate = 1,
        CompleteTruncate = 2,
        CompletedCopyFiles = 3
    }
}
