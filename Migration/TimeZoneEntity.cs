using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Migration
{
    public class TimeZoneEntity
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string BaseUtcOffSet { get; set; }
    }
}
