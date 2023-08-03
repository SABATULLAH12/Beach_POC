using System;
using System.Collections.Generic;
using System.Text;

namespace coke_beach_reportGenerator_api.Models
{
    public class FilterLeftPanel
    {
        public long ID { get; set; }
        public int LevelID { get; set; }
        public long MetricID { get; set; }
        public string MetricName { get; set; }
        public long ParentID { get; set; }
        public bool IsSelectable { get; set; }
    }
}
