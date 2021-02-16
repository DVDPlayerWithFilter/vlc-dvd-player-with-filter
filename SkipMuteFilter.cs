using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVDPlayerWithFilter
{
    public class SkipMuteFilterFile
    {
        public List<SkipMuteFilter> Filters { get; set; }
        public string Title { get; set; }
        public string Version { get; set; }
        public TimeSpan FilterOffset { get; set;  }
    }

    public class SkipMuteFilter
    {
        public string Description { get; set; }
        public string Type { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}
