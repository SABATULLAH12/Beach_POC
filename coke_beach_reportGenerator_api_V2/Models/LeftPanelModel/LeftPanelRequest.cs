using System;
using System.Collections.Generic;
using System.Text;

namespace coke_beach_reportGenerator_api.Models.LeftPanelModel
{
    public class LeftPanelRequest
    {
        public PopupLevelData GeographyMenu { get; set; }
        public TimePeriod TimeperiodMenu { get; set; }
        public PopupLevelData BenchmarkMenu { get; set; }
        public List<PopupLevelData> ComparisonMenu { get; set; }
        public List<PopupLevelData> FilterMenu { get; set; }
    }
}
