using System;

namespace Migration
{
    public class MigrationEventArgs : EventArgs
    {
        public string Message { get; set; }

        public string DestinationTable { get; set; }

        public int Package { get; set; }

        public int MigratedRecords { get; set; }
    }
}
