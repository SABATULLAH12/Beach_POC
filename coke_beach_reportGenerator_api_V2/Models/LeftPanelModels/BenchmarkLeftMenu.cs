using System;
using System.Collections.Generic;
using System.Text;

namespace coke_beach_reportGenerator_api.Models
{
    public class BenchmarkLeftMenu
    {
        public string GeographyType { get; set; }
        public long ID { get; set; }
        public string MetricType { get; set; }
        public int LevelID { get; set; }
        public string LevelName { get; set; }
        public int MetricID { get; set; }
        public string MetricName { get; set; }
        public string ParentMap { get; set; }
        public long ParentID { get; set; }
        public bool IsSelectable { get; set; }
    }
}
