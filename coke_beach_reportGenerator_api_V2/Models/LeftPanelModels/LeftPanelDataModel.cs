using System;
using System.Collections.Generic;
using System.Text;

namespace coke_beach_reportGenerator_api.Models
{
    public class LeftPanelDataModel
    {
        public List<GeographyLeftPanel> GeographyLeftPanels { get; set; }
        public List<TimePeriodLeftPanel> TimePeriodLeftPanels { get; set; }
        public List<BenchmarkLeftMenu> BenchmarkLeftMenus { get; set; }
        public List<FilterLeftPanel> FilterLeftPanels { get; set; }
    }
}
