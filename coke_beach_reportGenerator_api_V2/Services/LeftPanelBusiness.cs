using Azure.Storage.Blobs;
using coke_beach_reportGenerator_api.Models;
using coke_beach_reportGenerator_api.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace coke_beach_reportGenerator_api.Services
{
    public class LeftPanelBusiness : ILeftPanelBusiness
    {
        private readonly ILeftPanelService _leftPanelService;
        public LeftPanelBusiness(ILeftPanelService leftPanelService)
        {
            _leftPanelService = leftPanelService;
        }
        public LeftPanel GetGeoLeftPanelData()
        {
            DataSet leftPanelDataSet = _leftPanelService.GetData("SPLeftPanelFilters_GeographyFilter");
            LeftPanel geoPanelObject = BuildGeoLeftPanelModel(leftPanelDataSet);

            return geoPanelObject;
        }
        public LeftPanel SetJsonLeftPanelData()
        {
            DataSet leftPanelDataSet = _leftPanelService.GetData("SPLeftPanelJson");
            LeftPanel leftPanelObject = BuildLeftPanelModel(leftPanelDataSet);

            return leftPanelObject;
        }
        public static LeftPanel BuildLeftPanelModel(DataSet dataSet)
        {
            LeftPanel _leftPanel = new LeftPanel();

            if (dataSet.Tables.Count > 0)
            {
                _leftPanel.geographyMenu = BuildGeographyModel(dataSet.Tables[0]);
            }
            if (dataSet.Tables.Count > 1)
            {
                _leftPanel.timeperiodMenu = BuildTimeperiodModel(dataSet.Tables[1]);
            }
            if (dataSet.Tables.Count > 2)
            {
                _leftPanel.benchmarkMenu = BuildBenchmarkComparisionModel(dataSet.Tables[2], "BENCHMARK");
                _leftPanel.comparisonMenu = BuildBenchmarkComparisionModel(dataSet.Tables[2], "COMPARISION");
            }
            if (dataSet.Tables.Count > 3)
            {
                _leftPanel.filterMenu = BuildFilterModel(dataSet.Tables[3]);
            }
            return _leftPanel;
        }
        public LeftPanel GetLeftPanelData(string countryId)
        {
            DataSet leftPanelDataSet = _leftPanelService.GetData("SPLeftPanelFilters_BasedOnSelection", countryId);
            LeftPanel leftPanelObject = BuildBenchamrkLeftPanelModel(leftPanelDataSet);

            return leftPanelObject;
        }
        public static LeftPanel BuildGeoLeftPanelModel(DataSet dataSet)
        {
            LeftPanel _leftPanel = new LeftPanel();
            if (dataSet.Tables.Count > 0)
            {
                _leftPanel.geographyMenu = BuildGeographyModel(dataSet.Tables[0]);
            }
            
            return _leftPanel;
        }
        public static LeftPanel BuildBenchamrkLeftPanelModel(DataSet dataSet)
        {
            LeftPanel _leftPanel = new LeftPanel();

            if (dataSet.Tables.Count > 0)
            {
                _leftPanel.timeperiodMenu = BuildTimeperiodModel(dataSet.Tables[0]);
            }
            if (dataSet.Tables.Count > 1)
            {
                _leftPanel.benchmarkMenu = BuildBenchmarkComparisionModel(dataSet.Tables[1], "BENCHMARK");
                _leftPanel.comparisonMenu = BuildBenchmarkComparisionModel(dataSet.Tables[1], "COMPARISION");
            }
            if (dataSet.Tables.Count > 2)
            {
                _leftPanel.filterMenu = BuildFilterModel(dataSet.Tables[2]);
            }
            return _leftPanel;
        }
        public static LeftPanelMenu BuildGeographyModel(DataTable dataTable)
        {
            LeftPanelMenu _geographyMenu = new LeftPanelMenu();
            if (dataTable == null || dataTable.Rows.Count == 0)
            {
                return _geographyMenu;
            }
            PopupLevel level1 = new PopupLevel();
            level1.data = new List<PopupLevelData>();
            level1.levelId = 1;
            level1.data.Add(new PopupLevelData() { id = 1, hasChild = false, text = "Total", metricId = 1, isSelectable = true, parentId = -1000, type = "Total", GeographyId = "Total",detailedText="Total" });
            level1.data.Add(new PopupLevelData() { id = 2, hasChild = true, text = "OU", metricId = 2, isSelectable = false, parentId = -1000, type = "OU", hasChildSelected = false });
            level1.data.Add(new PopupLevelData() { id = 3, hasChild = true, text = "Country", metricId = 3, isSelectable = false, parentId = -1000, type = "Country", hasChildSelected = false });
            level1.data.Add(new PopupLevelData() { id = 4, hasChild = true, text = "Region", metricId = 4, isSelectable = false, parentId = -1000, type = "Region", hasChildSelected = false });
            level1.data.Add(new PopupLevelData() { id = 5, hasChild = true, text = "Bottler", metricId = 5, isSelectable = false, parentId = -1000, type = "Bottler", hasChildSelected = false });

            _geographyMenu.data = new List<PopupLevel>();
            _geographyMenu.data.Add(level1); 

            var ouList = dataTable.AsEnumerable().Select(row => new { ouId = row.Field<int>("OUId"), ouName = row.Field<string>("OU") }).Distinct().OrderBy(a=>a.ouName);
            var countryList = dataTable.AsEnumerable().Select(row => new { countryCode = row.Field<string>("CountryName"), countryId = row.Field<int>("CountryId"), country = row.Field<string>("Country") }).Distinct().OrderBy(a=>a.country);
            var regionList = dataTable.AsEnumerable().Where(row => !(new List<string> { "NA", "Prefer not to answer", "None of the above" }.Contains(row.Field<string>("Region").ToLower(), StringComparer.OrdinalIgnoreCase))).Select(row => new { countryCode = row.Field<string>("CountryName"), countryId = row.Field<int>("CountryId"), country = row.Field<string>("Country"), regionId = row.Field<int>("RegionId"), region = row.Field<string>("Region") }).Distinct().OrderBy(a => a.country).ThenBy(a=>a.region);
            var bottlerList = dataTable.AsEnumerable().Where(row => !(new List<string> { "NA", "#N/A" }.Contains(row.Field<string>("Bottler").ToLower(),StringComparer.OrdinalIgnoreCase))).Select(row => new { countryCode = row.Field<string>("CountryName"), countryId = row.Field<int>("CountryId"), country = row.Field<string>("Country"), bottlerId = row.Field<int>("BottlerId"), bottler = row.Field<string>("Bottler") }).Distinct().OrderBy(a => a.country).ThenBy(a=>a.bottler);

            PopupLevel level2 = new PopupLevel();
            level2.data = new List<PopupLevelData>();
            level2.levelId = 2;


            PopupLevel level3 = new PopupLevel();
            level3.data = new List<PopupLevelData>();
            level3.levelId = 3;
            int uniqueId = 6;

            foreach (var ou in ouList)
            {
                level2.data.Add(new PopupLevelData() { id = uniqueId, hasChild = false, text = ou.ouName, metricId = ou.ouId, isSelectable = true, parentId = 2, type = "OU", GeographyId = ou.ouName, isSelected = false, detailedText= ou.ouName });
                uniqueId++;
            }
            foreach (var country in countryList)
            {
                level2.data.Add(new PopupLevelData() { id = uniqueId, hasChild = false, text = country.country, metricId = country.countryId, isSelectable = true, parentId = 3, type = "Country", GeographyId = country.countryCode, isSelected = false, detailedText = country.country });
                uniqueId++;
            }
            Dictionary<int, int> regionCountryAdded = new Dictionary<int, int>();
            foreach (var region in regionList)
            {
                if (!regionCountryAdded.ContainsKey(region.countryId))
                {
                    regionCountryAdded.Add(region.countryId, uniqueId);
                    level2.data.Add(new PopupLevelData() { id = uniqueId, hasChild = true, text = region.country, metricId = region.countryId, isSelectable = false, parentId = 4, type = "Country", GeographyId = region.countryCode, isSelected = false, hasChildSelected = false, detailedText = region.country });
                    uniqueId++;
                }

                level3.data.Add(new PopupLevelData() { id = uniqueId, hasChild = false, text = region.region, metricId = region.regionId, isSelectable = true, parentId = regionCountryAdded[region.countryId], type = "Region", GeographyId = region.countryCode, isSelected = false, parentMetricId = region.countryId.ToString(), detailedText = region.country+" - "+ region.region });
                uniqueId++;
            }
            Dictionary<int, int> bottlerCountryAdded = new Dictionary<int, int>();
            foreach (var bottler in bottlerList)
            {
                if (!bottlerCountryAdded.ContainsKey(bottler.countryId))
                {
                    bottlerCountryAdded.Add(bottler.countryId, uniqueId);
                    level2.data.Add(new PopupLevelData() { id = uniqueId, hasChild = true, text = bottler.country, metricId = bottler.countryId, isSelectable = false, parentId = 5, type = "Country", GeographyId = bottler.countryCode, isSelected = false, hasChildSelected = false, detailedText = bottler.country });
                    uniqueId++;
                }

                level3.data.Add(new PopupLevelData() { id = uniqueId, hasChild = false, text = bottler.bottler, metricId = bottler.bottlerId, isSelectable = true, parentId = bottlerCountryAdded[bottler.countryId], type = "Bottler", GeographyId = bottler.countryCode, isSelected = false, parentMetricId = bottler.countryId.ToString(), detailedText = bottler.country + " - " + bottler.bottler });
                uniqueId++;
            }
            _geographyMenu.data.Add(level2);
            _geographyMenu.data.Add(level3);
            return _geographyMenu;
        }
        public static List<CountryOUMapping> BuildCountryOUMapping(DataTable dataTable)
        {
            List<CountryOUMapping> countryOUMapping = new List<CountryOUMapping>();
            if (dataTable == null || dataTable.Rows.Count == 0)
            {
                return countryOUMapping;
            }
            var ouCountryList = dataTable.AsEnumerable().Select(row => new { ouId = row.Field<int>("OUId"), countryId = row.Field<int>("CountryId") }).Distinct();

            foreach (var ouCountry in ouCountryList)
            {
                countryOUMapping.Add(new CountryOUMapping() { ouId = ouCountry.ouId, countryId = ouCountry.countryId });
            }
            return countryOUMapping;
        }
        public static List<TimePeriod> BuildTimeperiodModel(DataTable dataTable)
        {
            List<TimePeriod> _timeperiodList = new List<TimePeriod>();
            if (dataTable == null || dataTable.Rows.Count == 0)
            {
                return _timeperiodList;
            }
            foreach (DataRow row in dataTable.Rows)
            {
                _timeperiodList.Add(new TimePeriod()
                {
                    id = row.Field<int>("TimeperiodId"),
                    text = row.Field<string>("TimePeriodText"),
                    type = row.Field<string>("TimePeriodType"),
                    geography = row.Field<string>("Geography")
                });
            }
            return _timeperiodList;
        }
        public static LeftPanelMenu BuildBenchmarkComparisionModel(DataTable dataTable, string benchmarkCompare)
        {
            LeftPanelMenu leftPanelMenu = new LeftPanelMenu();
            leftPanelMenu.data = new List<PopupLevel>();
            if (dataTable == null || dataTable.Rows.Count == 0)
            {
                return leftPanelMenu;
            }
            var filterDataTable = dataTable.AsEnumerable().Where(row => row.Field<string>("MetricType").Equals(benchmarkCompare, StringComparison.OrdinalIgnoreCase)).CopyToDataTable();
            var levels = filterDataTable.AsEnumerable().Select(row => new { levelId = row.Field<int>("levelId"), levelName = row.Field<string>("levelName") }).Distinct().OrderByDescending(row => row.levelId).ToList();
            Dictionary<long, bool> idList = new Dictionary<long, bool>();
            int maxLevel = filterDataTable.AsEnumerable().Select(r => new { levelid = r.Field<int>("levelId") }).Max(r => r.levelid);

            PopupLevel consumptionPopupLevel = new PopupLevel();
            consumptionPopupLevel.data = new List<PopupLevelData>();
            consumptionPopupLevel.levelId = maxLevel + 1;
            consumptionPopupLevel.levelName = "Consumption";

            long maxid = filterDataTable.AsEnumerable().Where(row => row.Field<int>("levelId") == maxLevel).Select(r => new { id = r.Field<long>("Id") }).Max(r => r.id);

            maxid = maxid + 1000;

            foreach (var level in levels)
            {
                PopupLevel popupLevel = new PopupLevel();
                popupLevel.data = new List<PopupLevelData>();
                popupLevel.levelId = level.levelId;
                popupLevel.levelName = level.levelName;
                DataTable levelTable = filterDataTable.AsEnumerable().Where(row => row.Field<int>("levelId") == level.levelId).CopyToDataTable();

                bool brandLevelSelected = false;
                if (level.levelId == maxLevel)
                {
                    brandLevelSelected = true;
                }

                foreach (DataRow row in levelTable.Rows)
                {
                    bool hasChild = false;
                    long parentId = row.Field<long?>("parentId") ?? -1;
                    long id = row.Field<long>("Id");
                    if (idList.ContainsKey(parentId))
                    {
                        //hasChild = idList[parentId];
                    }
                    else
                    {
                        idList.Add(parentId, true);
                    }
                    hasChild = idList.ContainsKey(id) ? idList[id] : false;
                    if (brandLevelSelected)
                    {
                        consumptionPopupLevel.data.AddRange(new List<PopupLevelData>() {
                            new PopupLevelData()
                        {
                            id = maxid,
                            text = "Observed Drinkers",
                            type = "Consumption",
                            isSelectable = true,
                            metricId=1,
                            isMultiSelect = false,
                            parentMap = row.Field<string>("parentMap"),
                            parentId= (long)row.Field<long>("Id"),
                            GeographyId = row.Field<string>("GeographyType"),
                            hasChild=false,
                            isSelected=true,
                            searchText = row.Field<string>("SearchText")+": Observed Drinkers"
                        },
                        new PopupLevelData()
                        {
                            id = maxid+1,
                            text = "Daily",
                            type = "Consumption",
                            isSelectable = true,
                            metricId=2,
                            isMultiSelect = false,
                            parentMap = row.Field<string>("parentMap"),
                            parentId= (long)row.Field<long>("Id"),
                            GeographyId = row.Field<string>("GeographyType"),
                            hasChild=false,
                            isSelected=false,
                            searchText = row.Field<string>("SearchText")+": Daily"
                        },
                        new PopupLevelData()
                        {
                            id = maxid+2,
                            text = "Weekly",
                            type = "Consumption",
                            isSelectable = true,
                            metricId=3,
                            isMultiSelect = false,
                            parentMap = row.Field<string>("parentMap"),
                            parentId= (long)row.Field<long>("Id"),
                            GeographyId = row.Field<string>("GeographyType"),
                            hasChild=false,
                            isSelected=false,
                            searchText = row.Field<string>("SearchText")+": Weekly"
                        },
                        new PopupLevelData()
                        {
                            id = maxid+3,
                            text = "Weekly+",
                            type = "Consumption",
                            isSelectable = true,
                            metricId=4,
                            isMultiSelect = false,
                            parentMap = row.Field<string>("parentMap"),
                            parentId= (long)row.Field<long>("Id"),
                            GeographyId = row.Field<string>("GeographyType"),
                            hasChild=false,
                            isSelected=false,
                            searchText = row.Field<string>("SearchText")+": Weekly+"
                        },
                        //new PopupLevelData()
                        //{
                        //    id = maxid+4,
                        //    text = "Monthly",
                        //    type = "Consumption",
                        //    isSelectable = true,
                        //    metricId=5,
                        //    isMultiSelect = false,
                        //    parentMap = row.Field<string>("parentMap"),
                        //    parentId= (long)row.Field<long>("Id"),
                        //    GeographyId = row.Field<string>("GeographyType"),
                        //    hasChild=false,
                        //    isSelected=false
                        //},
                        //new PopupLevelData()
                        //{
                        //    id = maxid+5,
                        //    text = "Occasional",
                        //    type = "Consumption",
                        //    isSelectable = true,
                        //    metricId=6,
                        //    isMultiSelect = false,
                        //    parentMap = row.Field<string>("parentMap"),
                        //    parentId= (long)row.Field<long>("Id"),
                        //    GeographyId = row.Field<string>("GeographyType"),
                        //    hasChild=false,
                        //    isSelected=false
                        //}
                        });
                        maxid = maxid + 6;
                    }
                    popupLevel.data.Add(new PopupLevelData()
                    {
                        id = (long)row.Field<long>("Id"),
                        text = row.Field<string>("MetricName"),
                        type = level.levelName,
                        isSelectable = row.Field<bool>("isSelectable"),
                        isMultiSelect = false,
                        metricId = (long)row.Field<int>("MetricId"),
                        parentMap = row.Field<string>("parentMap"),
                        parentId = (long)parentId,
                        GeographyId = row.Field<string>("GeographyType"),
                        hasChild = brandLevelSelected ? true : hasChild,
                        isSelected = false,
                        consumptionId = 1,
                        consumptionName = "Observed Drinkers",
                        hasChildSelected = false,
                        searchText = row.Field<string>("SearchText") + ": Observed Drinkers"
                    });
                }
                leftPanelMenu.data.Insert(0, popupLevel);
                //leftPanelMenu.data.Add(popupLevel);
            }
            leftPanelMenu.data.Add(consumptionPopupLevel);
            return leftPanelMenu;
        }
        public static LeftPanelMenu BuildFilterModel(DataTable dataTable)
        {
            LeftPanelMenu leftPanelMenu = new LeftPanelMenu();
            leftPanelMenu.data = new List<PopupLevel>();
            if (dataTable == null || dataTable.Rows.Count == 0)
            {
                return leftPanelMenu;
            }
            var levels = dataTable.AsEnumerable().Select(row => new { levelId = row.Field<int>("levelId") }).Distinct().OrderByDescending(row => row.levelId).ToList();
            Dictionary<long, bool> idList = new Dictionary<long, bool>();
            Dictionary<long, bool> beverageList = new Dictionary<long, bool>();
            int maxLevel = dataTable.AsEnumerable().Select(r => new { levelid = r.Field<int>("levelId") }).Max(r => r.levelid);

            PopupLevel consumptionPopupLevel = new PopupLevel();
            consumptionPopupLevel.data = new List<PopupLevelData>();
            consumptionPopupLevel.levelId = maxLevel + 1;
            consumptionPopupLevel.levelName = "Consumption";

            long maxid = dataTable.AsEnumerable().Where(row => row.Field<int>("levelId") == maxLevel).Select(r => new { id = r.Field<long>("Id") }).Max(r => r.id);

            maxid = maxid + 1000;
            foreach (var level in levels)
            {
                PopupLevel popupLevel = new PopupLevel();
                popupLevel.data = new List<PopupLevelData>();
                popupLevel.levelId = level.levelId;
                popupLevel.levelName = "";
                DataTable levelTable = dataTable.AsEnumerable().Where(row => row.Field<int>("levelId") == level.levelId).CopyToDataTable();
                bool brandLevelSelected = false;
                if (level.levelId == maxLevel)
                {
                    brandLevelSelected = true;
                }
                foreach (DataRow row in levelTable.Rows)
                {
                    bool hasChild = false;
                    long parentId = row.Field<int?>("parentId") ?? -1;
                    long id = row.Field<long>("Id");
                    if (idList.ContainsKey(parentId))
                    {
                        //hasChild = idList[parentId];
                    }
                    else
                    {
                        idList.Add(parentId, true);
                    }



                    if ((level.levelId == 5 || level.levelId == 4) && (!beverageList.ContainsKey(parentId)))
                    {
                        beverageList.Add(parentId, true);
                    }

                    else if (beverageList.ContainsKey(id) && (!beverageList.ContainsKey(parentId)))
                    {
                        beverageList.Add(parentId, true);
                    }

                    hasChild = idList.ContainsKey(id) ? idList[id] : false;
                    string type = "Filter";
                    if (level.levelId == 5 || beverageList.ContainsKey(id))
                    {
                        type = "BeverageFilter";
                    }
                    if((level.levelId == 2) && type == "Filter" && !hasChild)
                    {
                        continue;
                    }
                    if (brandLevelSelected)
                    {
                        consumptionPopupLevel.data.AddRange(new List<PopupLevelData>() {
                            new PopupLevelData()
                        {
                            id = maxid,
                            text = "Observed Drinkers",
                            type = "Consumption",
                            isSelectable = true,
                            metricId=1,
                            isMultiSelect = false,
                            parentMap = row.Field<string>("parentMap"),
                            parentId= (long)row.Field<long>("Id"),
                            GeographyId = row.Field<string>("GeographyType"),
                            hasChild=false,
                            isSelected=true
                        },
                        new PopupLevelData()
                        {
                            id = maxid+1,
                            text = "Daily",
                            type = "Consumption",
                            isSelectable = true,
                            metricId=2,
                            isMultiSelect = false,
                            parentMap = row.Field<string>("parentMap"),
                            parentId= (long)row.Field<long>("Id"),
                            GeographyId = row.Field<string>("GeographyType"),
                            hasChild=false,
                            isSelected=false
                        },
                        new PopupLevelData()
                        {
                            id = maxid+2,
                            text = "Weekly",
                            type = "Consumption",
                            isSelectable = true,
                            metricId=3,
                            isMultiSelect = false,
                            parentMap = row.Field<string>("parentMap"),
                            parentId= (long)row.Field<long>("Id"),
                            GeographyId = row.Field<string>("GeographyType"),
                            hasChild=false,
                            isSelected=false
                        },
                        new PopupLevelData()
                        {
                            id = maxid+3,
                            text = "Weekly+",
                            type = "Consumption",
                            isSelectable = true,
                            metricId=4,
                            isMultiSelect = false,
                            parentMap = row.Field<string>("parentMap"),
                            parentId= (long)row.Field<long>("Id"),
                            GeographyId = row.Field<string>("GeographyType"),
                            hasChild=false,
                            isSelected=false
                        },
                        //new PopupLevelData()
                        //{
                        //    id = maxid+4,
                        //    text = "Monthly",
                        //    type = "Consumption",
                        //    isSelectable = true,
                        //    metricId=5,
                        //    isMultiSelect = false,
                        //    parentMap = row.Field<string>("parentMap"),
                        //    parentId= (long)row.Field<long>("Id"),
                        //    hasChild=false,
                        //    isSelected=false
                        //},
                        //new PopupLevelData()
                        //{
                        //    id = maxid+5,
                        //    text = "Occasional",
                        //    type = "Consumption",
                        //    isSelectable = true,
                        //    metricId=6,
                        //    isMultiSelect = false,
                        //    parentMap = row.Field<string>("parentMap"),
                        //    parentId= (long)row.Field<long>("Id"),
                        //    hasChild=false,
                        //    isSelected=false
                        //}
                        });
                        maxid = maxid + 6;
                    }
                    popupLevel.data.Add(new PopupLevelData()
                    {
                        id = (long)row.Field<long>("Id"),
                        text = row.Field<string>("MetricName"),
                        type = type,
                        isSelectable = row.Field<bool>("isSelectable"),
                        isMultiSelect = false,
                        metricId = (long)row.Field<int>("MetricId"),
                        parentId = (long)parentId,
                        GeographyId = row.Field<string>("GeographyType"),
                        hasChild = brandLevelSelected ? true : hasChild,
                        isSelected = false,
                        consumptionId = 1,
                        consumptionName = "Observed Drinkers",
                        parentMap = row.Field<string>("parentMap"),
                        hasChildSelected = false,
                    });
                }
                leftPanelMenu.data.Insert(0, popupLevel);
                //leftPanelMenu.data.Add(popupLevel);
            }
            leftPanelMenu.data.Add(consumptionPopupLevel);
            return leftPanelMenu;
        }
        public DataSet SetJsonMappingForLeftPanel()
        {
            DataSet dset = null;
            dset = _leftPanelService.GetData("SPJsonMappingForLeftPanel");
            return dset;
        }
    }
}
