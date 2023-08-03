using System;
using System.Collections.Generic;
using System.Text;

namespace coke_beach_reportGenerator_api.Models
{
    [Serializable]
    public class LeftPanel
    {
        public LeftPanelMenu geographyMenu { get; set; }
        public List<TimePeriod> timeperiodMenu { get; set; }
        public LeftPanelMenu benchmarkMenu { get; set; }
        public LeftPanelMenu comparisonMenu { get; set; }
        public LeftPanelMenu filterMenu { get; set; }
        public List<CountryOUMapping> countryOUMapping { get; set; }
    }
    [Serializable]
    public class LeftPanelMenu
    {
        public int id { get; set; }
        public string name { get; set; }
        public List<PopupLevel> data { get; set; }
    }
    [Serializable]
    public class PopupLevel
    {
        public int levelId { get; set; }
        public string levelName { get; set; }
        public List<PopupLevelData> data { get; set; }
    }
    [Serializable]
    public class PopupLevelData
    {
        public long id { get; set; }
        public string text { get; set; }
        public string type { get; set; }
        public long metricId { get; set; }
        public long parentId { get; set; }
        public bool isSelectable { get; set; }
        public bool hasChild { get; set; }
        public bool isMultiSelect { get; set; }
        public string parentMap { get; set; }
        public string GeographyId { get; set; }
        public long consumptionId { get; set; }
        public string consumptionName { get; set; }
        public bool isSelected { get; set; }
        public string parentMetricId { get; set; }
        public bool hasChildSelected { get; set; }
        public string detailedText { get; set; }
        public int levelId { get; set; }
        public string searchText { get; set; }
    }
    [Serializable]
    public class CountryOUMapping
    {
        public long ouId { get; set; }
        public long countryId { get; set; }
    }
    [Serializable]
    public class TimePeriod
    {
        public string type { get; set; }
        public int id { get; set; }
        public string text { get; set; }
        public int geographyId { get; set; }
        public string geography { get; set; }
    }
}
