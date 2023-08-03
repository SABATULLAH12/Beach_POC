using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Nest;
using System.Linq;
using System.Collections.Generic;
using coke_beach_reportGenerator_api.ElasticHelper;
using System.Diagnostics.CodeAnalysis;
using System.Data;
using System.Reflection;
using coke_beach_reportGenerator_api.Services.Interfaces;
using coke_beach_reportGenerator_api.Models;
using coke_beach_reportGenerator_api.Models.LeftPanelModel;
using coke_beach_reportGenerator_api.Helper;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace coke_beach_reportGenerator_api.Functions.ElasticSearch
{
    public class BenchmarkComparison
    {
        public string Category { get; set; }
        public string? LowLevelCategory { get; set; }

        public string? Trademark { get; set; }

        public string? Brand { get; set; }
    }
    public class Geography
    {
        public string GeographyType { get; set; }
        public string Geo { get; set; }
    }
    public class Filter
    {
        public string AttributeType { get; set; }
        public List<string> Attributes { get; set; }

        public bool IsDemog { get; set; }
    }
    public class DemogShare
    {
        public string Country_ID { get; set; }
        public string Gender_Name { get; set; }
        public string Age_Range { get; set; }
        public string Ethnicity { get; set; }
        public string HH_Size_Group { get; set; }
        public string kids_count { get; set; }

        public string SEC_Group { get; set; }
        public long Index_resp { get; set; }

        public double Weight { get; set; }
    }

    public class EquitySlideMapping
    {
        public int AttributeGroupId { get; set; }
        public string AttributeType { get; set; }
        public int AttributeId { get; set; }
        public string Attribute { get; set; }
        public int AttributeGroupSortId { get; set; }
        public int AttributeSortId { get; set; }
        public int SlideNumber { get; set; }
    }

    public class DemogshareMulti
    {
        public string Country_ID { get; set; }
        public long Index_resp { get; set; }

        public double Weight { get; set; }
    }
    public class DemogShareWeighted
    {
        public string Country_ID { get; set; }
        public string AttributeType { get; set; }
        public string Attribute { get; set; }
        public double Weight { get; set; }
        public long Resp_Count { get; set; }
    }
    public class TotalShare
    {
        public string Country_ID { get; set; }
        public long Index_resp { get; set; }

        public double Weight { get; set; }
        public double Population { get; set; }
    }
    public class TotalShareMulti
    {
        public string Country_ID { get; set; }
        public long Index_resp { get; set; }

        public double Weight { get; set; }
        public double Population { get; set; }

    }
    public class TotalShareWeighted
    {
        public string Country_ID { get; set; }
        public double Weight { get; set; }
        public double Population { get; set; }
    }
    public class TotalSharePopulation
    {
        public string Country_ID { get; set; }
        public string AttributeType { get; set; }
        public string Attribute { get; set; }
        public double Population { get; set; }
    }
    public class TotalSharePopulationMulti
    {
        public string Country_ID { get; set; }

        public double Population { get; set; }
    }
    public class DemogShareEqualityComparer : IEqualityComparer<DemogShare>
    {
        public bool Equals(DemogShare x, DemogShare y)
        {
            // Compare objects based on specific properties
            return x.Index_resp == y.Index_resp;
        }

        public int GetHashCode(DemogShare obj)
        {
            return obj.Index_resp.GetHashCode();
        }
    }
    public class NumeratorDemogWeightedNewEqualityComparer : IEqualityComparer<NumeratorDemogWeightedNew>
    {
        public bool Equals(NumeratorDemogWeightedNew x, NumeratorDemogWeightedNew y)
        {
            // Compare objects based on specific properties
            return x.Selections == y.Selections && x.Index_resp == y.Index_resp && x.Attribute == y.Attribute
                                   && x.Country_ID == y.Country_ID && x.value == y.value && x.Weight == y.Weight;
        }

        public int GetHashCode(NumeratorDemogWeightedNew obj)
        {
            return obj.Selections.GetHashCode();
        }
    }

    public class TotalShareEqualityComparer : IEqualityComparer<TotalShare>
    {
        public bool Equals(TotalShare x, TotalShare y)
        {
            // Compare objects based on specific properties
            return x.Index_resp == y.Index_resp;
        }

        public int GetHashCode(TotalShare obj)
        {
            return obj.Index_resp.GetHashCode();
        }
    }

    public class NumeratorDemog
    {
        public string Selections { get; set; }
        public string Country_ID { get; set; }
        public string Gender_Name { get; set; }
        public string Age_Range { get; set; }
        public string Ethnicity { get; set; }
        public string HH_Size_Group { get; set; }
        public string kids_count { get; set; }

        public string SEC_Group { get; set; }
        public long Index_resp { get; set; }

        public double Weight { get; set; }
    }

    public class NumeratorDemogWeighted
    {
        public string Selections { get; set; }
        public string Country_ID { get; set; }
        public string AttributeType { get; set; }
        public string Attribute { get; set; }
        public double Weight { get; set; }
        public long Resp_Count { get; set; }
    }
    public class NumeratorDemogWeightedNew
    {
        public string Selections { get; set; }
        public string Country_ID { get; set; }
        public string Attribute { get; set; }
        public string value { get; set; }
        public double Weight { get; set; }
        public long Index_resp { get; set; }
        public string AttributeType { get; set; }
    }
    public class NumeratorDemogWeightedWithMetric
    {
        public string Selections { get; set; }
        public string Country_ID { get; set; }
        public string AttributeType { get; set; }
        public string Attribute { get; set; }
        public string MappingAttributeType { get; set; }
        public string MappingAttribute { get; set; }
        public int AttributeGroupSortId { get; set; }
        public int AttributeSortId { get; set; }
        public int SlideNumber { get; set; }
        public bool IsNumerator { get; set; }
        public double Weight { get; set; }
        public long Resp_Count { get; set; }
    }
    public class NumeratorDemogIntermediate
    {
        public string Selections { get; set; }
        public string Country_ID { get; set; }
        public string AttributeType { get; set; }
        public string Attribute { get; set; }
        public string MappingAttributeType { get; set; }
        public string MappingAttribute { get; set; }
        public int AttributeGroupSortId { get; set; }
        public int AttributeSortId { get; set; }
        public int SlideNumber { get; set; }
        public bool IsNumerator { get; set; }
        public long Resp_Count { get; set; }
        public double WeeklyWeight { get; set; }
        public double incidenceWeight { get; set; }
        public double Population { get; set; }
        public double DrinkerPopulation { get; set; }
    }

    public class NumeratorDemogFinal
    {
        public string Selections { get; set; }
        public string AttributeType { get; set; }
        /*public string Attribute { get; set; }*/
        public string MappingAttributeType { get; set; }
        public string MappingAttribute { get; set; }
        public int AttributeGroupSortId { get; set; }
        public int AttributeSortId { get; set; }
        public int SlideNumber { get; set; }
        public bool IsNumerator { get; set; }
        public long Resp_Count { get; set; }
        public double Weight { get; set; }
    }

    public class DenominatorDemogFinal
    {
        public string Selections { get; set; }
        public string AttributeType { get; set; }
        public string MappingAttributeType { get; set; }
        public int AttributeGroupSortId { get; set; }
        public int SlideNumber { get; set; }
        public long Resp_Count { get; set; }
        public double Weight { get; set; }
    }

    public class SelectionObj
    {
        public string SelectionType { get; set; }
        public int SelectionID { get; set; }
        public string SelectionName { get; set; }
        public int ConsumptionID { get; set; }
        public string ConsumptionName { get; set; }
    }
    public class DemogSelectionMapping
    {
        public string AttributeType { get; set; }
        public string Attribute { get; set; }
        public int AttributeGroupSortId { get; set; }
        public int AttributeSortId { get; set; }
        public int SlideNumber { get; set; }
        public string SelectionType { get; set; }
        public int SelectionID { get; set; }
        public string SelectionName { get; set; }
        public int ConsumptionID { get; set; }
        public string ConsumptionName { get; set; }
    }
    public class FinalIntermediateDemog
    {
        public string SelectionType { get; set; }
        public string Selection { get; set; }
        public string AttributeType { get; set; }
        /* public string Attribute { get; set; }*/
        public string MappingAttributeType { get; set; }
        public string MappingAttribute { get; set; }
        public int AttributeGroupSortId { get; set; }
        public int AttributeSortId { get; set; }
        public int SlideNumber { get; set; }
        public bool IsNumerator { get; set; }
        public double Numerator { get; set; }
        public double Denominator { get; set; }
        public double Percentage { get; set; }
        public long SampleSize { get; set; }
        public long BaseSize { get; set; }
    }
    public class MultiSelectMapping
    {
        public string SelectionType { get; set; }
        public string Selection { get; set; }
        public int SelectionID { get; set; }
        public string AttributeType { get; set; }
        public string Attribute { get; set; }
        public int SlideNumber { get; set; }
        public int AttributeGroupId { get; set; }
        public int AttributeId { get; set; }
        public int AttributeGroupSortId { get; set; }
        public int AttributeSortId { get; set; }
        public double TotalDrinker { get; set; }
        public int ConsumptionId { get; set; }
        public string ConsumptionName { get; set; }
    }
    public class FinalTableDemog
    {
        public string SelectionType { get; set; }
        public string Selection { get; set; }
        public string AttributeType { get; set; }
        /*public string Attribute { get; set; }*/
        public string MappingAttributeType { get; set; }
        public string MappingAttribute { get; set; }
        public int AttributeGroupSortId { get; set; }
        public int AttributeSortId { get; set; }
        public int SlideNumber { get; set; }
        public bool IsNumerator { get; set; }
        public double Numerator { get; set; }
        public double Denominator { get; set; }
        public double Percentage { get; set; }
        public double Significance { get; set; }
        public long SampleSize { get; set; }
        public long BaseSize { get; set; }
    }
    public class WeightedRespondentMultiSelect
    {
        public string Selections { get; set; }
        public string Country_ID { get; set; }
        public string Attribute { get; set; }
        public double WeightedRespondent { get; set; }
        public string AttributeType { get; set; }
    }
    public class WeightedRespondentMultiSelectDenom
    {
        public string Selections { get; set; }
        public string Country_ID { get; set; }
        public double WeightedRespondent { get; set; }
    }
    public class TotalDrinker
    {
        public string Selections { get; set; }
        public string Attribute { get; set; }
        public double TotalDrinkerSum { get; set; }
        public string AttributeType { get; set; }
    }

    public class EquityDenom
    {
        public string Selections { get; set; }
        public string Country_ID { get; set; }
        public double WeightedRespondent { get; set; }
    }
    public class DenominatorWeightedResponded
    {
        public string Selections { get; set; }
        public string Country_ID { get; set; }
        public double Weight { get; set; }
        public long Index_Resp { get; set; }
    }

    public class DenominatorEquityData
    {
        public string Selections { get; set; }
        public string Country_ID { get; set; }
        public double Weight { get; set; }
        public long Index_Resp { get; set; }
    }
    public class ObserveDrinkerPopulationDaily_Weekly
    {
        public string Selections { get; set; }
        public double Weight { get; set; }
        public double Population { get; set; }
    }

    public class EquityCondition
    {
        public string FieldName { get; set; }
        public Object Value { get; set; }
        public string Operator { get; set; }
    }
    public class EquityWeightNumerator
    {
        public string Selections { get; set; }
        public string AttributeType { get; set; }
        public string Attribute { get; set; }
        public int SlideNumber { get; set; }
        public long RespCount { get; set; }
        public double Weight { get; set; }
    }
    public class EquityFinalData
    {
        public int SelectionId { get; set; }
        public string SelectionType { get; set; }
        public string Selection { get; set; }
        public string AttributeType { get; set; }
        public string Attribute { get; set; }
        public int AttributeGroupSortId { get; set; }
        public int AttributeSortId { get; set; }
        public int AttributeGroupId { get; set; }
        public int AttributeId { get; set; }
        public int SlideNumber { get; set; }
        public int ConsumptionId { get; set; }
        public string ConsumptioName { get; set; }
        public double Percentage { get; set; }
        public long BaseSize { get; set; }
    }
    public class EquityWeightDenominator
    {
        public string Selections { get; set; }
        public long RespCount { get; set; }
        public double Weight { get; set; }
    }
    public class SlideMappingEqualityComparer : IEqualityComparer<DemogSelectionMapping>
    {
        public bool Equals(DemogSelectionMapping x, DemogSelectionMapping y)
        {
            // Compare objects based on specific properties
            return x.Attribute == y.Attribute && x.AttributeGroupSortId == y.AttributeGroupSortId && x.AttributeSortId == y.AttributeSortId
                   && x.AttributeType == y.AttributeType && x.ConsumptionID == y.ConsumptionID && x.ConsumptionName == y.ConsumptionName
                   && x.SelectionID == y.SelectionID && x.SelectionName == y.SelectionName && x.SelectionType == y.SelectionType && x.SlideNumber == y.SlideNumber;
        }

        public int GetHashCode(DemogSelectionMapping obj)
        {
            return obj.SelectionID.GetHashCode();
        }
    }

    public class ElasticSearchPracticeSlideCalculation
    {
        IElasticClient _elasticClient = null;
        ElasticMethod _elasticMethod = new ElasticMethod();
        private ILeftPanelMapping _leftpanelMapping;
        private IReportGeneratorBusiness _reportGeneratorBusiness;
        private string drinkIndex;
        private string equityIndex;
        private string drinkEquityIndex;
        private string logPath = ConstantPath.GetRootPath+ @"Logs\Log.txt";
        public ElasticSearchPracticeSlideCalculation(IElasticClient client, IReportGeneratorBusiness reportGeneratorBusiness, ILeftPanelMapping leftPanelMapping,IConfiguration config)
        {
            _reportGeneratorBusiness = reportGeneratorBusiness;
            _elasticClient = client;
            _leftpanelMapping = leftPanelMapping;
            drinkIndex = config["Values:drinksIndex"];
            equityIndex = config["Values:equityIndex"];
            drinkEquityIndex = config["Values:drinksEquityIndex"];

            int bucket_size = Convert.ToInt32(config["Values:maxBucketSize"]);

            var settings = new Dictionary<string, object> { { "search.max_buckets", bucket_size } };
            var updateSettingsRequest = new ClusterPutSettingsRequest { Transient = new Dictionary<string, object>(settings) };
            var response = _elasticClient.Cluster.PutSettings(updateSettingsRequest);
            if (!_leftpanelMapping.CheckLeftPanel())
            {
                var geoFile = ConstantPath.GetRootPath + @"Json\" + "geographyMapping.json";
                DataTable geoTable = null;
                using (StreamReader r = new StreamReader(geoFile))
                {
                    string data = r.ReadToEnd();
                    geoTable = JsonConvert.DeserializeObject<DataTable>(data);
                    r.Close();
                }
                var timeFile = ConstantPath.GetRootPath + @"Json\" + "timeperiodMapping.json";
                DataTable timeTable = null;
                using (StreamReader r = new StreamReader(timeFile))
                {
                    string data = r.ReadToEnd();
                    timeTable = JsonConvert.DeserializeObject<DataTable>(data);
                    r.Close();
                }
                var prodFile = ConstantPath.GetRootPath + @"Json\" + "productMapping.json";
                DataTable prodTable = null;
                using (StreamReader r = new StreamReader(prodFile))
                {
                    string data = r.ReadToEnd();
                    prodTable = JsonConvert.DeserializeObject<DataTable>(data);
                    r.Close();
                }
                var filterFile = ConstantPath.GetRootPath + @"Json\" + "attributeMapping.json";
                DataTable filterTable = null;
                using (StreamReader r = new StreamReader(filterFile))
                {
                    string data = r.ReadToEnd();
                    filterTable = JsonConvert.DeserializeObject<DataTable>(data);
                    r.Close();
                }
                var slideFile = ConstantPath.GetRootPath + @"Json\" + "SlideMapping.json";
                DataTable slideTable = null;
                using (StreamReader r = new StreamReader(slideFile))
                {
                    string data = r.ReadToEnd();
                    slideTable = JsonConvert.DeserializeObject<DataTable>(data);
                    r.Close();
                }
                DataSet dset = new DataSet();
                dset.Tables.Add(geoTable);
                dset.Tables.Add(timeTable);
                dset.Tables.Add(prodTable);
                dset.Tables.Add(filterTable);
                dset.Tables.Add(slideTable);
                _leftpanelMapping.SetLeftPanel(dset);
            }
        }
        public static DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);

            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Defining type of data column gives proper data table 
                var type = (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType);
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name, type);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            //put a breakpoint here and check datatable
            return dataTable;
        }
        public static double SignificanceValue(double BenchmarkData, long BenchmarkBase, double CompareData, long CompareBase)
        {
            if (BenchmarkData != null && BenchmarkBase != null)
            {
                try
                {
                    var P = ((CompareData * CompareBase) + (BenchmarkData * BenchmarkBase)) / (CompareBase + BenchmarkBase);
                    var SE = Math.Sqrt((P * (1 - P) * ((1 / CompareBase) + (1 / BenchmarkBase))));
                    var Z = (CompareData - BenchmarkData) / SE;
                }
                catch (Exception e)
                {
                    return 0;
                }
            }
            return 0;
        }
        public IActionResult ElasticSearchPracticeSampleSize(LeftPanelRequest leftPanelRequest)
        {            
            var sampleSizesList = new List<SampleSizeModel>();
            //var leftPanelRequest = (await req.GetBodyAsync<LeftPanelRequest>()).Value;
            var geographyDataTable = _leftpanelMapping.GetGeographyMapping();
            var timeperiodDataTable = _leftpanelMapping.GetTimeperiodMapping();
            var productDataTable = _leftpanelMapping.GetProductMapping();
            var filterDataTable = _leftpanelMapping.GetFilterMapping();

            List<PopupLevelData> geoPopupLevels = new List<PopupLevelData>();

            geoPopupLevels.Add(leftPanelRequest.GeographyMenu);

            DataTable geographyTable = GetTableOutputPPT(geoPopupLevels, "GeographyMenu", new DataTable());

            List<PopupLevelData> benchamrkPopupLevels = new List<PopupLevelData>();

            benchamrkPopupLevels.Add(leftPanelRequest.BenchmarkMenu);

            DataTable benchmarkTable = GetTableOutputPPT(benchamrkPopupLevels, "Benchmark", new DataTable());

            DataTable comparisonTable = GetTableOutputPPT(leftPanelRequest.ComparisonMenu, "Comparison", new DataTable());

            DataTable demogsTable = GetTableOutputPPT(leftPanelRequest.FilterMenu, "FilterMenu", new DataTable(), "Filter");

            DataTable beveragesTable = GetTableOutputPPT(leftPanelRequest.FilterMenu, "FilterMenu", new DataTable(), "BeverageFilter");

            benchmarkTable.Merge(comparisonTable);

            ElasticMethod _elasticMethod = new ElasticMethod();
            var listOfGeography = GetGeographySelection(geographyDataTable, geographyTable);
            var listOfMonths = GetTimeperiodSelection(timeperiodDataTable, leftPanelRequest.TimeperiodMenu.id);
            var benchmark = GetBenchmarkComparisons(productDataTable, benchmarkTable.Select("SelectionType='Benchmark'").CopyToDataTable())[0];
            var comparisonList = GetBenchmarkComparisons(productDataTable, comparisonTable);
            var filterList = GetFilterSelection(filterDataTable, demogsTable);

            string outerOperatorTypeDemogShare = "AND";
            string innerOperatorTypeDemogShare = "OR";
            var compositeSampleSizeParamNew = new List<string> { "index_resp" };

            var aggregationParamSampleSize = new Dictionary<string, Dictionary<string, string>>();
            var benchmarkParamSampleSize = new Dictionary<string, string>();
            var comaprisonParamSampleSize = new Dictionary<string, string>();
            benchmarkParamSampleSize.Add("Category", benchmark.Category);
            string benchmarkSelectionSampleSize = benchmark.Category;
            if (benchmark.LowLevelCategory != null)
            {
                benchmarkParamSampleSize.Add("Low Level Category", benchmark.LowLevelCategory);
                benchmarkSelectionSampleSize = benchmark.LowLevelCategory;
            }
            if (benchmark.Trademark != null)
            {
                benchmarkParamSampleSize.Add("Trademark", benchmark.Trademark);
                benchmarkSelectionSampleSize = benchmark.Trademark;
            }
            if (benchmark.Brand != null)
            {
                benchmarkParamSampleSize.Add("Brand", benchmark.Brand);
                benchmarkSelectionSampleSize = benchmark.Brand;
            }
            aggregationParamSampleSize.Add(benchmarkSelectionSampleSize + " -- " + "Benchmark", benchmarkParamSampleSize);

            comparisonList.ForEach(comparison =>
            {
                comaprisonParamSampleSize.Add("Category", comparison.Category);
                string comparisonSelection = comparison.Category;
                if (comparison.LowLevelCategory != null)
                {
                    comaprisonParamSampleSize.Add("Low Level Category", comparison.LowLevelCategory);
                    comparisonSelection = comparison.LowLevelCategory;
                }
                if (comparison.Trademark != null)
                {
                    comaprisonParamSampleSize.Add("Trademark", comparison.Trademark);
                    comparisonSelection = comparison.Trademark;
                }
                if (comparison.Brand != null)
                {
                    comaprisonParamSampleSize.Add("Brand", comparison.Brand);
                    comparisonSelection = comparison.Brand;
                }
                aggregationParamSampleSize.Add(comparisonSelection + " -- " + "Comparison", new Dictionary<string, string>(comaprisonParamSampleSize));
                comaprisonParamSampleSize.Clear();
            });

            var geoList = new List<string>();
            foreach (var geog in listOfGeography)
            {
                geoList.Add(geog.Geo);
            }
            #region DemogFilterCode
            var demogList = filterList.Where(fil => fil.IsDemog == true).ToList();
            #endregion
            Dictionary<string, object> demogShareParam = new Dictionary<string, object>();
            demogShareParam.Add(listOfGeography[0].GeographyType, geoList);
            demogShareParam.Add("Month_ID", listOfMonths);
            if (demogList.Count > 0)
            {
                foreach (var dem in demogList)
                {
                    demogShareParam.Add(dem.AttributeType, dem.Attributes);
                }
            }
            var sampleSizeList = _elasticClient.Search<dynamic>(x => x
            .Index(drinkIndex)
            .Query(q => _elasticMethod.GetQuery(demogShareParam, innerOperatorTypeDemogShare, outerOperatorTypeDemogShare, 0))
            .Source(false)
            .Aggregations(a => _elasticMethod.GetAggregationQuery(aggregationParamSampleSize, compositeSampleSizeParamNew, "SampleSize")));

            foreach (var key in aggregationParamSampleSize.Keys)
            {
                var sample = new Dictionary<string, string>();
                var innerDictionary = aggregationParamSampleSize[key];
                var response = sampleSizeList.Aggregations.Filter(key).Composite("SampleSize").Buckets;
                var selectionName = innerDictionary.ElementAt(0).Value;

                foreach (var b in response)
                {
                    foreach (var k in b.Key.AsEnumerable())
                    {
                        if (!sample.ContainsKey(k.Value.ToString()))
                        {
                            sample.Add(k.Value.ToString(), k.Key);
                        }
                    }

                }
                sampleSizesList.Add(new SampleSizeModel()
                {
                    SelectionID = 0,
                    SelectionName = selectionName,
                    SelectionType = key.ToString().Contains("Benchmark") ? "Benchmark" : "Comaprison",
                    SampleSize = sample.Count,
                    ConsumptionId = 0,
                    DetailedText = ""
                });
            }
            var res = sampleSizesList;
            var benchmarkSel = leftPanelRequest.BenchmarkMenu;
            var comparisonSel = leftPanelRequest.ComparisonMenu.ToList();
            foreach (var sel in sampleSizesList)
            {
                if (sel.SelectionType == "Benchmark")
                {
                    sel.ConsumptionId = (int)benchmarkSel.consumptionId;
                }
                else
                {
                    foreach (var compare in comparisonSel)
                    {
                        if (compare.text == sel.SelectionName)
                        {
                            sel.ConsumptionId = (int)compare.consumptionId;
                        }
                    }
                }
            }
            foreach (var el in sampleSizesList)
            {

                el.DetailedText = el.SelectionName + " ";
                switch (el.ConsumptionId)
                {
                    case 1: el.DetailedText += "- Observed Drinkers"; break;
                    case 2: el.DetailedText += "- Daily"; break;
                    case 3: el.DetailedText += "- Weekly"; break;
                    case 4: el.DetailedText += "- Weekly+"; break;
                    case 5: el.DetailedText += "- Monthly"; break;
                    case 6: el.DetailedText += "- Occasional"; break;
                    default: el.DetailedText += "- Observed Drinkers"; break;
                }
            }

            // Filter out the data which has sample size less than 30
            var newList = sampleSizesList.Where(x => x.SampleSize < 30).ToList();

            return new OkObjectResult(newList);
        }
        public List<Geography> GetGeographySelection(DataTable sourceTable, DataTable selection)
        {
            var listOfGeography = new List<Geography>();
            string type = "";
            if (selection.Rows[0].Field<Object>("OUID") != null)
            {
                type = "OU";
            }
            else if (selection.Rows[0].Field<Object>("CountryID") != null)
            {
                type = "Country";
            }
            if (selection.Rows[0].Field<Object>("RegionID") != null)
            {
                type = "Region";
            }
            else if (selection.Rows[0].Field<Object>("BottlerID") != null)
            {
                type = "Bottler";
            }

            var b = selection.Rows[0];
            if (type == "Country" || type == "OU")
            {
                string searchCondition = (type == "Country" ? "CountryID=" + b.Field<Object>("CountryID") : "OUID=" + b.Field<Object>("OUID"));

                var geoList = sourceTable.Select(searchCondition).Select(a => a.Field<string>("CountryName")).Distinct().ToList();
                foreach (var i in geoList)
                {
                    listOfGeography.Add(new Geography() { GeographyType = "Country_ID", Geo = i });
                }

            }
            else
            {
                string searchCondition = "CountryID=" + b.Field<Object>("CountryID") + " and ";
                searchCondition += (type == "Region" ? "RegionID=" + b.Field<Object>("RegionID") : "BottlerID=" + b.Field<Object>("BottlerID"));
                if (type == "Region")
                {
                    var geoList = sourceTable.Select(searchCondition).Select(a => new { Country = a.Field<string>("CountryName"), Region = a.Field<string>("Region") }).Distinct().ToList();
                    foreach (var i in geoList)
                    {
                        listOfGeography.Add(new Geography() { GeographyType = "Country_ID", Geo = i.Country });
                        listOfGeography.Add(new Geography() { GeographyType = "Region", Geo = i.Region });
                    }
                }
                else
                {
                    var geoList = sourceTable.Select(searchCondition).Select(a => new { Country = a.Field<string>("CountryName"), Bottler = a.Field<string>("Bottler") }).Distinct().ToList();
                    foreach (var i in geoList)
                    {
                        listOfGeography.Add(new Geography() { GeographyType = "Country_ID", Geo = i.Country });
                        listOfGeography.Add(new Geography() { GeographyType = "Bottler", Geo = i.Bottler });
                    }
                }
                // var geoList = sourceTable.Select(searchCondition).Select(a => new {a.Field<string>("CountryName")).Distinct().ToList();


            }

            return listOfGeography;
        }
        public List<string> GetTimeperiodSelection(DataTable sourceTable, int id)
        {
            return sourceTable.Select("id=" + id).Select(a => a.Field<string>("timeperiod")).Distinct().ToList();
        }
        public List<BenchmarkComparison> GetBenchmarkComparisons(DataTable sourceTable, DataTable selection)
        {
            var listOfBenchmarkComparisons = new List<BenchmarkComparison>();
            foreach (DataRow row in selection.Rows)
            {
                BenchmarkComparison obj = new BenchmarkComparison();
                obj.Category = sourceTable.Select("CategoryID=" + row.Field<int>("CategoryID")).Select(a => a.Field<string>("Category")).FirstOrDefault();
                obj.LowLevelCategory = row.Field<Object>("LowLevelCategoryID") == null ? null : sourceTable.Select("LowLevelCategoryID=" + row.Field<int>("LowLevelCategoryID")).Select(a => a.Field<string>("Low Level Category")).FirstOrDefault();
                obj.Trademark = row.Field<Object>("TrademarkID") == null ? null : sourceTable.Select("TrademarkID=" + row.Field<int>("TrademarkID")).Select(a => a.Field<string>("Trademark")).FirstOrDefault();
                obj.Brand = row.Field<Object>("BrandID") == null ? null : sourceTable.Select("BrandID=" + row.Field<int>("BrandID")).Select(a => a.Field<string>("Brand")).FirstOrDefault();
                listOfBenchmarkComparisons.Add(obj);
            }
            return listOfBenchmarkComparisons;
        }
        public List<Filter> GetFilterSelection(DataTable sourceTable, DataTable selection)
        {
            var list = new List<Filter>();
            foreach (DataRow row in selection.Rows)
            {
                var a = sourceTable.Select("AttributeGroupID=" + row.Field<int>("Attributegroupid") + " and AttributeId=" + row.Field<int>("Attributeid"))
                    .Select(a => new Filter
                    {
                        IsDemog = a.Field<string>("TableName") == "5 Demographics" ? true : false,
                        Attributes = new List<string> { a.Field<string>("AttributeName") },
                        AttributeType = a.Field<string>("columnName")
                    }).FirstOrDefault();
                list.Add(a);
            }
            var demogList = list.Where(a => a.IsDemog == true).GroupBy(a => new { a.IsDemog, a.AttributeType }).Select(a => new Filter { IsDemog = a.Key.IsDemog, AttributeType = a.Key.AttributeType, Attributes = a.Select(b => b.Attributes.FirstOrDefault()).ToList() });
            var occasionList = list.Where(a => a.IsDemog == false).Select(a => new Filter { IsDemog = false, AttributeType = a.Attributes.FirstOrDefault(), Attributes = new List<string>() { "1" } });
            var resultList = new List<Filter>();
            resultList.AddRange(demogList);
            resultList.AddRange(occasionList);
            return resultList;
        }
        public List<DemogShare> GetDemogShareData(List<Geography> listOfGeography,List<string> listOfMonths,List<Filter> demogList)
        {
            Dictionary<string, object> demogShareParam = new Dictionary<string, object>();
            var geoList = new List<string>();
            foreach (var geog in listOfGeography)
            {
                geoList.Add(geog.Geo);
            }
            demogShareParam.Add(listOfGeography[0].GeographyType, geoList);
            demogShareParam.Add("Month_ID", listOfMonths);
            if (demogList.Count > 0)
            {
                foreach (var dem in demogList)
                {
                    demogShareParam.Add(dem.AttributeType, dem.Attributes);
                }
            }

            string outerOperatorTypeDemogShare = "AND";
            string innerOperatorTypeDemogShare = "OR";

            var demogShareResult = _elasticClient.Search<dynamic>(s => s
                 .Index(drinkIndex)
                 .Query(q => _elasticMethod.GetQuery(demogShareParam, innerOperatorTypeDemogShare, outerOperatorTypeDemogShare, 0))
                 .Source(false)
                 .Aggregations(agg => agg.Composite("gr", a => a.Size(90000).Sources(a => a.Terms("Country_ID", f => f.Field("Country_ID"))
                                                                             .Terms("Gender_Name", f => f.Field("Gender_Name"))
                                                                             .Terms("Age_Range", f => f.Field("Age_Range"))
                                                                             .Terms("Ethnicity", f => f.Field("Ethnicity"))
                                                                             .Terms("HH_Size_Group", f => f.Field("HH_Size_Group"))
                                                                             .Terms("kids_count", f => f.Field("kids_count"))
                                                                             .Terms("SEC_Group", f => f.Field("SEC Group"))
                                                                             .Terms("Index_resp", f => f.Field("index_resp"))
                                                                             .Terms("Weight", f => f.Field("Weight"))
                                                                       )
                                                 )
                              )
                 );

            var demogShare1 = demogShareResult.Aggregations.Composite("gr").Buckets.Select(b => new DemogShare()
            {
                Country_ID = b.Key["Country_ID"].ToString(),
                Gender_Name = b.Key["Gender_Name"].ToString(),
                Age_Range = b.Key["Age_Range"].ToString(),
                Ethnicity = b.Key["Ethnicity"].ToString(),
                HH_Size_Group = b.Key["HH_Size_Group"].ToString(),
                kids_count = b.Key["kids_count"].ToString(),
                SEC_Group = b.Key["SEC_Group"].ToString(),
                Index_resp = Convert.ToInt64(b.Key["Index_resp"].ToString()),
                Weight = Convert.ToDouble(b.Key["Weight"].ToString())
            }).ToList();

            demogShareResult = _elasticClient.Search<dynamic>(s => s
                 .Index(equityIndex)
                 /*.Size(100000)*/
                 .Query(q => _elasticMethod.GetQuery(demogShareParam, innerOperatorTypeDemogShare, outerOperatorTypeDemogShare, 0))
                 //.Fields(f => f.Field("Country_ID").Field("Month_ID").Field("Gender_Name").Field("Weight"))
                 .Source(false)
                 .Aggregations(agg => agg.Composite("gr", a => a.Size(90000).Sources(a => a.Terms("Country_ID", f => f.Field("Country_ID"))
                                                                             .Terms("Gender_Name", f => f.Field("Gender_Name"))
                                                                             .Terms("Age_Range", f => f.Field("Age_Range"))
                                                                             .Terms("Ethnicity", f => f.Field("Ethnicity"))
                                                                             .Terms("HH_Size_Group", f => f.Field("HH_Size_Group"))
                                                                             .Terms("kids_count", f => f.Field("kids_count"))
                                                                             .Terms("SEC_Group", f => f.Field("SEC Group"))
                                                                             .Terms("Index_resp", f => f.Field("index_resp"))
                                                                             .Terms("Weight", f => f.Field("Weight"))
                                                                       )
                                                 )
                              )
                 );

            var demogShare2 = demogShareResult.Aggregations.Composite("gr").Buckets.Select(b => new DemogShare()
            {
                Country_ID = b.Key["Country_ID"].ToString(),
                Gender_Name = b.Key["Gender_Name"].ToString(),
                Age_Range = b.Key["Age_Range"].ToString(),
                Ethnicity = b.Key["Ethnicity"].ToString(),
                HH_Size_Group = b.Key["HH_Size_Group"].ToString(),
                kids_count = b.Key["kids_count"].ToString(),
                SEC_Group = b.Key["SEC_Group"].ToString(),
                Index_resp = Convert.ToInt64(b.Key["Index_resp"].ToString()),
                Weight = Convert.ToDouble(b.Key["Weight"].ToString())
            }).ToList();

            var demogShare = demogShare1.Union(demogShare2, new DemogShareEqualityComparer()).ToList();
            return demogShare;
        }
        public List<TotalShare> GetTotalShareData(List<Geography> listOfGeography, List<string> listOfMonths)
        {
            Dictionary<string, object> totalShareParam = new Dictionary<string, object>();
            var geoList = new List<string>();
            foreach (var geog in listOfGeography)
            {
                geoList.Add(geog.Geo);
            }
            totalShareParam.Add(listOfGeography[0].GeographyType, geoList);
            totalShareParam.Add("Month_ID", listOfMonths);

            string outerOperatorTypeTotalShare = "AND";
            string innerOperatorTypeTotalShare = "OR";

            var totalShareResult = _elasticClient.Search<dynamic>(s => s
                 .Index(drinkIndex)
                 //.Size(100000)
                 .Query(q => _elasticMethod.GetQuery(totalShareParam, innerOperatorTypeTotalShare, outerOperatorTypeTotalShare, 0))
                 //.Fields(f => f.Field("Country_ID").Field("Month_ID").Field("Gender_Name").Field("Weight"))
                 .Source(false)
                 .Aggregations(agg => agg.Composite("gr", a => a.Size(90000).Sources(a => a.Terms("Country_ID", f => f.Field("Country_ID"))
                                                                             .Terms("Index_resp", f => f.Field("index_resp"))
                                                                             .Terms("Weight", f => f.Field("Weight"))
                                                                             .Terms("Population", f => f.Field("Population"))
                                                                       )
                                                 )
                              )
                 );
            var totalShare1 = totalShareResult.Aggregations.Composite("gr").Buckets.Select(b => new TotalShare()
            {
                Country_ID = b.Key["Country_ID"].ToString(),
                Index_resp = Convert.ToInt64(b.Key["Index_resp"].ToString()),
                Population = Convert.ToDouble(b.Key["Population"].ToString()),
                Weight = Convert.ToDouble(b.Key["Weight"].ToString())
            }).ToList();

            totalShareResult = _elasticClient.Search<dynamic>(s => s
                .Index(equityIndex)
                //.Size(100000)
                .Query(q => _elasticMethod.GetQuery(totalShareParam, innerOperatorTypeTotalShare, outerOperatorTypeTotalShare, 0))
                //.Fields(f => f.Field("Country_ID").Field("Month_ID").Field("Gender_Name").Field("Weight"))
                .Source(false)
                .Aggregations(agg => agg.Composite("gr", a => a.Size(90000).Sources(a => a.Terms("Country_ID", f => f.Field("Country_ID"))
                                                                            .Terms("Index_resp", f => f.Field("index_resp"))
                                                                            .Terms("Weight", f => f.Field("Weight"))
                                                                            .Terms("Population", f => f.Field("Population"))
                                                                      )
                                                )
                             )
                );
            var totalShare2 = totalShareResult.Aggregations.Composite("gr").Buckets.Select(b => new TotalShare()
            {
                Country_ID = b.Key["Country_ID"].ToString(),
                Index_resp = Convert.ToInt64(b.Key["Index_resp"].ToString()),
                Population = Convert.ToDouble(b.Key["Population"].ToString()),
                Weight = Convert.ToDouble(b.Key["Weight"].ToString())
            }).ToList();

            var totalShare = totalShare1.Union(totalShare2, new TotalShareEqualityComparer()).ToList();
            return totalShare;
        }
        public List<WeightedRespondentMultiSelect> GetWeightedRespondentMultiSelect(string[] attributeList,DataTable cde, Dictionary<string, Dictionary<string, string>> aggregationParamNum, Dictionary<string, object> numeratorParam)
        {
            
            var weightedRespondentMultiSelect = new List<WeightedRespondentMultiSelect>();
            string outerOperatorTypeNumerator = "AND";
            string innerOperatorTypeNumerator = "OR";


            foreach (var att in attributeList)
            {
                var attributeDetailList = cde.AsEnumerable().Where(a => a.Field<string>("columnName") == att).Select(a => a.Field<string>("AttributeName")).Distinct().ToList(); // && !(new List<string> { "Working", "Others", "Eating", "Reading", "Dinner", "Active Leisure/Exercise" }.Contains(a.Field<string>("AttributeName"))

                foreach (var attDetail in attributeDetailList)
                {
                    var comParam = new List<string> { "Country_ID", "index_resp", "Weight" };
                    var detailParam = new Dictionary<string, object>(numeratorParam);
                    if (!detailParam.ContainsKey(attDetail))
                        detailParam.Add(attDetail, new List<string> { "1" });
                    comParam.Add(attDetail);
                    var numeratorCalculationWeighted = _elasticClient.Search<dynamic>(s => s
                    .Index(drinkIndex)
                    //.Size(100000)
                    .Query(q => _elasticMethod.GetQuery(detailParam, innerOperatorTypeNumerator, outerOperatorTypeNumerator, 0))
                    .Source(false)
                    .Aggregations(a => _elasticMethod.GetAggregationQuery(aggregationParamNum, comParam)));
                    var listOfnumeratorWeighted = new List<NumeratorDemogWeightedNew>();
                    foreach (var key in aggregationParamNum.Keys)
                    {
                        var bucket = numeratorCalculationWeighted.Aggregations.Filter(key).Composite("numerator").Buckets;
                        foreach (var b in bucket)
                        {
                            foreach (var k in b.Key.AsEnumerable())
                            {
                                if (new List<string>() { "Country_ID", "Weight", "index_resp" }.Contains(k.Key) || k.Value.ToString() == "0")
                                {
                                    continue;
                                }
                                if (!new List<string>() { "Country_ID", "Weight", "index_resp" }.Contains(k.Key))
                                {
                                    listOfnumeratorWeighted.Add(new NumeratorDemogWeightedNew()
                                    {
                                        Selections = key,
                                        Country_ID = b.Key["Country_ID"].ToString(),
                                        AttributeType = att,
                                        Attribute = k.Key,
                                        value = k.Value.ToString(),
                                        Weight = (double)b.Key["Weight"],
                                        Index_resp = (long)b.Key["index_resp"]
                                    });
                                }

                            }
                        }
                        //numeratorCalculation.Aggregations.Composite(key).Buckets;
                    }
                    var resTemp = listOfnumeratorWeighted.GroupBy(g => new { g.Selections, g.Country_ID, g.AttributeType, g.Attribute, g.Index_resp, g.Weight, g.value }).Select(s => new NumeratorDemogWeightedNew
                    {
                        Selections = s.Key.Selections,
                        Country_ID = s.Key.Country_ID,
                        AttributeType = s.Key.AttributeType,
                        Attribute = s.Key.Attribute,
                        Index_resp = s.Key.Index_resp,
                        value = s.Key.value,
                        Weight = s.Key.Weight
                    }).ToList();
                    var resTemp2 = resTemp.Where(x => x.value.Equals("1")).GroupBy(g => new { g.Selections, g.Country_ID, g.AttributeType, g.Attribute }).Select(s => new WeightedRespondentMultiSelect
                    {
                        Selections = s.Key.Selections,
                        Country_ID = s.Key.Country_ID,
                        AttributeType = s.Key.AttributeType,
                        Attribute = s.Key.Attribute,
                        WeightedRespondent = s.Sum(x => x.Weight)
                    }).ToList();
                    lock (weightedRespondentMultiSelect)
                        weightedRespondentMultiSelect.AddRange(resTemp2);
                }
            }
            return weightedRespondentMultiSelect;
        }
        public List<WeightedRespondentMultiSelectDenom> GetWeightedRespondentMultiSelectTotal(Dictionary<string, Dictionary<string, string>> aggregationParamNum, Dictionary<string, object> numeratorParam)
        {
            var weightedRespondentMultiSelectTotal = new List<WeightedRespondentMultiSelectDenom>();
            var comParamDenom = new List<string> { "Country_ID", "index_resp", "Weight" };
            string outerOperatorTypeNumerator = "AND";
            string innerOperatorTypeNumerator = "OR";

            var denominatorCalculationWeighted = _elasticClient.Search<dynamic>(s => s
                  .Index(drinkIndex)
                  //.Size(100000)
                  .Query(q => _elasticMethod.GetQuery(numeratorParam, innerOperatorTypeNumerator, outerOperatorTypeNumerator, 0))
                  .Source(false)
                  .Aggregations(a => _elasticMethod.GetAggregationQuery(aggregationParamNum, comParamDenom)));
            var listOfDenomWeighted = new List<DenominatorWeightedResponded>();
            foreach (var key in aggregationParamNum.Keys)
            {
                var bucket = denominatorCalculationWeighted.Aggregations.Filter(key).Composite("numerator").Buckets;
                foreach (var b in bucket)
                {
                    listOfDenomWeighted.Add(new DenominatorWeightedResponded()
                    {
                        Selections = key,
                        Country_ID = b.Key["Country_ID"].ToString(),
                        Weight = (double)b.Key["Weight"],
                        Index_Resp = (long)b.Key["index_resp"]
                    });
                }
            }

            var resDenom = listOfDenomWeighted.GroupBy(g => new { g.Selections, g.Country_ID }).Select(s => new WeightedRespondentMultiSelectDenom
            {
                Selections = s.Key.Selections,
                Country_ID = s.Key.Country_ID,
                WeightedRespondent = s.Sum(x => x.Weight)
            }).ToList();
            weightedRespondentMultiSelectTotal.AddRange(resDenom);
            return weightedRespondentMultiSelectTotal;
        }
        public List<WeightedRespondentMultiSelect> GetReportedDrinkMultiSelect(string[] attributeList, DataTable cde, Dictionary<string, Dictionary<string, string>> aggregationParamNum, Dictionary<string, object> numeratorParam)
        {
            var reportedDrinkSingleSelect = new List<WeightedRespondentMultiSelect>();
            string outerOperatorTypeNumerator = "AND";
            string innerOperatorTypeNumerator = "OR";

            foreach (var att in attributeList)
            {
                var attributeDetailList = cde.AsEnumerable().Where(a => a.Field<string>("columnName") == att).Select(a => a.Field<string>("AttributeName")).Distinct().ToList(); // && !(new List<string> { "Working", "Others", "Eating", "Reading", "Dinner", "Active Leisure/Exercise" }.Contains(a.Field<string>("AttributeName"))

                foreach (var attDetail in attributeDetailList)
                {
                    var comParam = new List<string> { "Country_ID" };
                    var detailParam = new Dictionary<string, object>(numeratorParam);
                    if (!detailParam.ContainsKey(attDetail))
                        detailParam.Add(attDetail, new List<string> { "1" });
                    comParam.Add(attDetail);
                    var numeratorCalculationWeighted = _elasticClient.Search<dynamic>(s => s
                    .Index(drinkIndex)
                    //.Size(100000)
                    .Query(q => _elasticMethod.GetQuery(detailParam, innerOperatorTypeNumerator, outerOperatorTypeNumerator, 0))
                    .Source(false)
                    .Aggregations(a => _elasticMethod.GetCustomAggregationQuery(aggregationParamNum, comParam)));
                    var listOfprodAggWeighted = new List<WeightedRespondentMultiSelect>();
                    foreach (var key in aggregationParamNum.Keys)
                    {
                        var productAggRes = numeratorCalculationWeighted.Aggregations.Filter(key).Sum("productAggregation").Value;
                        var bucket = numeratorCalculationWeighted.Aggregations.Filter(key).Composite("reportedDrinks").Buckets;
                        foreach (var b in bucket)
                        {
                            foreach (var k in b.Key.AsEnumerable())
                            {
                                if (new List<string>() { "Country_ID" }.Contains(k.Key) || k.Value.ToString() == "0")
                                {
                                    continue;
                                }
                                if (!new List<string>() { "Country_ID" }.Contains(k.Key))
                                {
                                    listOfprodAggWeighted.Add(new WeightedRespondentMultiSelect()
                                    {
                                        Selections = key,
                                        Country_ID = b.Key["Country_ID"].ToString(),
                                        AttributeType = att,
                                        Attribute = k.Key,
                                        WeightedRespondent = (double)productAggRes
                                    });
                                }

                            }
                        }
                    }

                    reportedDrinkSingleSelect.AddRange(listOfprodAggWeighted);
                }
            }
            return reportedDrinkSingleSelect;
        }
        public List<WeightedRespondentMultiSelectDenom> GetReportedDrinkMultiSelectTotal(Dictionary<string, Dictionary<string, string>> aggregationParamNum, Dictionary<string, object> numeratorParam)
        {
            var reportedRespondentMultiSelectTotal = new List<WeightedRespondentMultiSelectDenom>();
            string outerOperatorTypeNumerator = "AND";
            string innerOperatorTypeNumerator = "OR";

            var comParamDenomReported = new List<string> { "Country_ID" };
            var denomCalculationReported = _elasticClient.Search<dynamic>(s => s
                   .Index(drinkIndex)
                   .Query(q => _elasticMethod.GetQuery(numeratorParam, innerOperatorTypeNumerator, outerOperatorTypeNumerator, 0))
                   .Source(false)
                   .Aggregations(a => _elasticMethod.GetCustomAggregationQuery(aggregationParamNum, comParamDenomReported)));
            var listOfDenomReported = new List<WeightedRespondentMultiSelectDenom>();
            foreach (var key in aggregationParamNum.Keys)
            {
                var productAggRes = denomCalculationReported.Aggregations.Filter(key).Sum("productAggregation").Value;
                var bucket = denomCalculationReported.Aggregations.Filter(key).Composite("reportedDrinks").Buckets;
                foreach (var b in bucket)
                {
                    listOfDenomReported.Add(new WeightedRespondentMultiSelectDenom()
                    {
                        Selections = key,
                        Country_ID = b.Key["Country_ID"].ToString(),
                        WeightedRespondent = (double)productAggRes
                    });
                }

            }

            reportedRespondentMultiSelectTotal.AddRange(listOfDenomReported);
            return reportedRespondentMultiSelectTotal;
        }
        public List<NumSurveyResult> GetNumSurveys(List<Geography> listOfGeography, List<string> listOfMonths, List<Filter> demogList)
        {
            Dictionary<string, object> demogShareParam = new Dictionary<string, object>();
            var geoList = new List<string>();
            foreach (var geog in listOfGeography)
            {
                geoList.Add(geog.Geo);
            }
            demogShareParam.Add(listOfGeography[0].GeographyType, geoList);
            demogShareParam.Add("Month_ID", listOfMonths);
            if (demogList.Count > 0)
            {
                foreach (var dem in demogList)
                {
                    demogShareParam.Add(dem.AttributeType, dem.Attributes);
                }
            }

            string outerOperatorTypeDemogShare = "AND";
            string innerOperatorTypeDemogShare = "OR";
            var numSurveyResult = _elasticClient.Search<dynamic>(x => x
               .Index(drinkIndex)
               .Size(10000)
               .Query(q => _elasticMethod.GetQuery(demogShareParam, innerOperatorTypeDemogShare, outerOperatorTypeDemogShare, 0))
               .Fields(f => f.Fields("Country_ID").Field("Resp_Static_ID").Field("Respondent_ID"))
               .Source(false)
               .Aggregations(aggs => aggs.Composite("gr", g => g.Size(90000).Sources(s => s.Terms("Country_ID", f => f.Field("Country_ID"))
                                                                                           .Terms("Resp_Static_ID", f => f.Field("Resp_Static_ID"))
               ).Aggregations(aggs => aggs.Cardinality("Respondent_Distinct", g => g.Field("Respondent_ID")))
               )
               )
               );
            var numSurveyWeeklyDaily = numSurveyResult.Aggregations.Composite("gr").Buckets.Select(b => new NumSurveyResult()
            {
                Country_ID = b.Key["Country_ID"].ToString(),
                NumSurveys = (double)b.Cardinality("Respondent_Distinct").Value,
                Resp_Static_ID = b.Key["Resp_Static_ID"].ToString()
            }).ToList();
            return numSurveyWeeklyDaily;
        }
        public List<NumSurveyMaxWp> GetWpWeeks(List<Geography> listOfGeography, List<string> listOfMonths, List<Filter> demogList, Dictionary<string, Dictionary<string, string>> aggregationParamNum)
        {
            Dictionary<string, object> demogShareParam = new Dictionary<string, object>();
            var geoList = new List<string>();
            foreach (var geog in listOfGeography)
            {
                geoList.Add(geog.Geo);
            }
            demogShareParam.Add(listOfGeography[0].GeographyType, geoList);
            demogShareParam.Add("Month_ID", listOfMonths);
            if (demogList.Count > 0)
            {
                foreach (var dem in demogList)
                {
                    demogShareParam.Add(dem.AttributeType, dem.Attributes);
                }
            }

            string outerOperatorTypeDemogShare = "AND";
            string innerOperatorTypeDemogShare = "OR";
            List<NumSurveyWp> numSurveyWps = new List<NumSurveyWp>();
            var compositeNumSurveyParam = new List<string> { "Country_ID", "Resp_Static_ID", "Respondent_ID", "Freq_Numeric", "Weight", "num_surveys" };
            var numSurvey = _elasticClient.Search<dynamic>(x => x
             .Index(drinkIndex)
             .Query(q => _elasticMethod.GetQuery(demogShareParam, innerOperatorTypeDemogShare, outerOperatorTypeDemogShare, 0))
             .Source(false)
             .Aggregations(a => _elasticMethod.GetAggregationQuery(aggregationParamNum, compositeNumSurveyParam, "GroupBy")));

            foreach (var key in aggregationParamNum.Keys)
            {
                var numSurveyDic = new Dictionary<string, string>();
                var innerDictionary = aggregationParamNum[key];
                var response = numSurvey.Aggregations.Filter(key).Composite("GroupBy").Buckets;
                var selectionName = innerDictionary.ElementAt(0).Value;

                numSurveyWps.AddRange(numSurvey.Aggregations.Filter(key).Composite("GroupBy").Buckets.Select(b => new NumSurveyWp()
                {
                    Selection = key,
                    Country_ID = b.Key["Country_ID"].ToString(),
                    Resp_Static_ID = b.Key["Resp_Static_ID"].ToString(),
                    Respondent_ID = b.Key["Respondent_ID"].ToString(),
                    Freq_Numeric = Convert.ToInt32(b.Key["Freq_Numeric"].ToString()),
                    weight = Convert.ToDouble(b.Key["Weight"].ToString()),
                    num_surveys = Convert.ToInt32(b.Key["num_surveys"].ToString())
                }));
            }

            var numSurvey_Max = numSurveyWps
                       .GroupBy(x => new { Country_ID = x.Country_ID, Resp_Static_ID = x.Resp_Static_ID, Respondent_ID = x.Respondent_ID, Selection = x.Selection })
                       .Select(x => new NumSurveyMaxWp
                       {
                           Selection = x.Key.Selection,
                           Respondent_ID = x.Key.Respondent_ID,
                           Country_ID = x.Key.Country_ID,
                           Resp_Static_ID = x.Key.Resp_Static_ID,
                           weight = x.Max(m => m.weight),
                           Freq_Numeric = x.Max(m => m.Freq_Numeric),
                       }).ToList();

            var wpWeekWeeklyDaily = numSurvey_Max.GroupBy(x => new { Country_ID = x.Country_ID, Resp_Static_ID = x.Resp_Static_ID, Selection = x.Selection })
                       .Select(x => new NumSurveyMaxWp
                       {
                           Selection = x.Key.Selection,
                           Country_ID = x.Key.Country_ID,
                           Resp_Static_ID = x.Key.Resp_Static_ID,
                           weight = x.Sum(m => m.weight),
                           Freq_Numeric = x.Sum(m => m.Freq_Numeric)
                       }).ToList();
            return wpWeekWeeklyDaily;
        }
        public List<EquityWeightNumerator> GetEquityNumerator(List<EquitySlideMapping> equityMappingList, Dictionary<string, Dictionary<string, string>> aggregationParamNum, Dictionary<string,object> numeratorParam)
        {
            string outerOperatorTypeNumerator = "AND";
            string innerOperatorTypeNumerator = "OR";
            Dictionary<string, List<EquityCondition>> equityList = new Dictionary<string, List<EquityCondition>>();

            equityList.Add("Affinity", new List<EquityCondition>(){
                 new EquityCondition() { FieldName = "Affinity_ID", Value = new List<int> { 6, 7 }, Operator = "" } ,
                 new EquityCondition() { FieldName = "Fam_Aware", Value = new List<int> { 1}, Operator = "AND" }
             });

            equityList.Add("Uniqueness", new List<EquityCondition>()
            {
                new EquityCondition() { FieldName = "Unique_ID", Value = new List<int> { 6, 7 }, Operator = "" } ,
                new EquityCondition() { FieldName = "Fam_Aware", Value = new List<int> { 1}, Operator = "AND" }

            });
            equityList.Add("Meets Needs", new List<EquityCondition>()
            {
                new EquityCondition() { FieldName = "MNeeds_ID", Value = new List<int> { 6, 7 }, Operator = "" } ,
                new EquityCondition() { FieldName = "Fam_Aware", Value = new List<int> { 1}, Operator = "AND" }

            });
            equityList.Add("Price", new List<EquityCondition>()
            {
                new EquityCondition() { FieldName = "Price_ID", Value = new List<int> { 6, 7 }, Operator = "" } ,
                new EquityCondition() { FieldName = "Fam_Aware", Value = new List<int> { 1}, Operator = "AND" }

            });
            equityList.Add("Sets Trends", new List<EquityCondition>()
            {
                new EquityCondition() { FieldName = "Dynamic_ID", Value = new List<int> { 6, 7 }, Operator = "" } ,
                new EquityCondition() { FieldName = "Fam_Aware", Value = new List<int> { 1}, Operator = "AND" }

            });
            equityList.Add("Consideration", new List<EquityCondition>()
            {
                new EquityCondition() { FieldName = "Cons_ID", Value = new List<int> { 4 }, Operator = "" } ,
                new EquityCondition() { FieldName = "Fam_Aware", Value = new List<int> { 1}, Operator = "AND" }

            });
            equityList.Add("Worth More", new List<EquityCondition>()
            {
                new EquityCondition() { FieldName = "Worth_ID", Value = new List<int> { 1}, Operator = "" } ,
                new EquityCondition() { FieldName = "Fam_Aware", Value = new List<int> { 1}, Operator = "AND" }

            });
            equityList.Add("Worth Same", new List<EquityCondition>()
            {
                new EquityCondition() { FieldName = "Worth_ID", Value = new List<int> { 2 }, Operator = "" } ,
                new EquityCondition() { FieldName = "Fam_Aware", Value = new List<int> { 1}, Operator = "AND" }

            });
            equityList.Add("Worth Less", new List<EquityCondition>()
            {
                new EquityCondition() { FieldName = "Worth_ID", Value = new List<int> { 3}, Operator = "" } ,
                new EquityCondition() { FieldName = "Fam_Aware", Value = new List<int> { 1}, Operator = "AND" }

            });
            equityList.Add("Top of Mind", new List<EquityCondition>()
            {
                new EquityCondition() { FieldName = "Aware", Value = new List<int> { 1 }, Operator = "" } ,
                //new EquityCondition() { FieldName = "Aware_Fup", Value = new List<int> {1,2,3,4,5}, Operator = "OR" }

            });
            equityList.Add("Spontaneous", new List<EquityCondition>()
            {
                new EquityCondition() { FieldName = "Aware", Value = new List<int> { 1,2,3,4,5 }, Operator = "" } ,
                new EquityCondition() { FieldName = "Aware_Fup", Value = new List<int> {1,2,3,4,5}, Operator = "OR" }

            });
            equityList.Add("Total", new List<EquityCondition>()
            {
                new EquityCondition() { FieldName = "Fam_Aware", Value = new List<int> { 1 }, Operator = "" }

            });
            equityList.Add("Tried", new List<EquityCondition>()
            {
                new EquityCondition() { FieldName = "Fam_Tried", Value = new List<int> { 1 }, Operator = "" }

            });
            //equityList.Add("Unique", new List<EquityCondition>()
            //{
            //    new EquityCondition() { FieldName = "Fam_Tried", Value = new List<int> { 1 }, Operator = "" } ,

            //});
            //equityList.Add("Unique", new List<EquityCondition>()
            //{
            //    new EquityCondition() { FieldName = "Aware", Value = new List<int> { 1,2,3,4,5 }, Operator = "" } ,
            //    new EquityCondition() { FieldName = "Aware_Fup", Value = new List<int> {1,2,3,4,5}, Operator = "AND" }

            //});



            var equityNumeratorData = new List<EquityWeightNumerator>();
            foreach (var equity in equityList.Keys)
            {
                var cond = equityList[equity];
                var outerOperatorEquity = cond[cond.Count - 1].Operator;
                var innerOperatorEquity = "OR";
                var equityParam = new Dictionary<string, Object>();
                foreach (var item in cond)
                {
                    equityParam.Add(item.FieldName, item.Value);
                }
                QueryContainer query = _elasticMethod.GetQuery(numeratorParam, innerOperatorTypeNumerator, outerOperatorTypeNumerator, 0);
                QueryContainer equityQuery = _elasticMethod.GetQuery(equityParam, innerOperatorEquity, outerOperatorEquity, 0);

                QueryContainer resultQuery = Query<object>.Bool(bl => bl.Must(new List<QueryContainer> { query, equityQuery }.ToArray()));

                var numeratorCalculationEquityNew = _elasticClient.Search<dynamic>(s => s
                 .Index(equityIndex)
                 //.Size(100000)
                 .Query(q => resultQuery)
                 .Source(false)
                 .Aggregations(a => _elasticMethod.GetAggregationQuery(aggregationParamNum, "Weight", "index_resp")));

                foreach (var key in aggregationParamNum.Keys)
                {

                    equityNumeratorData.Add(new EquityWeightNumerator()
                    {
                        Selections = key,
                        AttributeType = equityMappingList.FindAll(a => a.Attribute == equity).FirstOrDefault().AttributeType,
                        SlideNumber = new List<string> { "Top of Mind", "Spontaneous", "Total", "Tried" }.Contains(equity) ? 13 : 12,
                        Attribute = equity,
                        RespCount = (long)numeratorCalculationEquityNew.Aggregations.Filter(key).Cardinality("colCount").Value,
                        Weight = (double)numeratorCalculationEquityNew.Aggregations.Filter(key).Sum("weightSum").Value
                    });


                }

            }
            return equityNumeratorData;
        }
        public List<EquitySlideMapping> GetEquitySlideMappings()
        {
            var equityMappingList = new List<EquitySlideMapping>();
            equityMappingList.Add(new EquitySlideMapping() { AttributeGroupId = 1000, AttributeType = "Equity", AttributeId = 1, Attribute = "Affinity", AttributeGroupSortId = 1, AttributeSortId = 1, SlideNumber = 12 });
            equityMappingList.Add(new EquitySlideMapping() { AttributeGroupId = 1000, AttributeType = "Equity", AttributeId = 2, Attribute = "Uniqueness", AttributeGroupSortId = 1, AttributeSortId = 2, SlideNumber = 12 });
            equityMappingList.Add(new EquitySlideMapping() { AttributeGroupId = 1000, AttributeType = "Equity", AttributeId = 3, Attribute = "Meets Needs", AttributeGroupSortId = 1, AttributeSortId = 3, SlideNumber = 12 });
            equityMappingList.Add(new EquitySlideMapping() { AttributeGroupId = 1000, AttributeType = "Equity", AttributeId = 5, Attribute = "Price", AttributeGroupSortId = 1, AttributeSortId = 5, SlideNumber = 12 });
            equityMappingList.Add(new EquitySlideMapping() { AttributeGroupId = 1000, AttributeType = "Equity", AttributeId = 6, Attribute = "Sets Trends", AttributeGroupSortId = 1, AttributeSortId = 6, SlideNumber = 12 });
            equityMappingList.Add(new EquitySlideMapping() { AttributeGroupId = 1000, AttributeType = "Equity", AttributeId = 7, Attribute = "Consideration", AttributeGroupSortId = 1, AttributeSortId = 7, SlideNumber = 12 });
            equityMappingList.Add(new EquitySlideMapping() { AttributeGroupId = 1000, AttributeType = "Equity", AttributeId = 8, Attribute = "Worth More", AttributeGroupSortId = 2, AttributeSortId = 1, SlideNumber = 12 });
            equityMappingList.Add(new EquitySlideMapping() { AttributeGroupId = 1000, AttributeType = "Equity", AttributeId = 9, Attribute = "Worth Same", AttributeGroupSortId = 2, AttributeSortId = 2, SlideNumber = 12 });
            equityMappingList.Add(new EquitySlideMapping() { AttributeGroupId = 1000, AttributeType = "Equity", AttributeId = 10, Attribute = "Worth Less", AttributeGroupSortId = 2, AttributeSortId = 3, SlideNumber = 12 });
            equityMappingList.Add(new EquitySlideMapping() { AttributeGroupId = 1000, AttributeType = "Awareness", AttributeId = 11, Attribute = "Top of Mind", AttributeGroupSortId = 1, AttributeSortId = 1, SlideNumber = 13 });
            equityMappingList.Add(new EquitySlideMapping() { AttributeGroupId = 1000, AttributeType = "Awareness", AttributeId = 12, Attribute = "Spontaneous", AttributeGroupSortId = 1, AttributeSortId = 2, SlideNumber = 13 });
            equityMappingList.Add(new EquitySlideMapping() { AttributeGroupId = 1000, AttributeType = "Awareness", AttributeId = 13, Attribute = "Total", AttributeGroupSortId = 1, AttributeSortId = 3, SlideNumber = 13 });
            equityMappingList.Add(new EquitySlideMapping() { AttributeGroupId = 1001, AttributeType = "Consumption", AttributeId = 1, Attribute = "Weekly+", AttributeGroupSortId = 2, AttributeSortId = 1, SlideNumber = 13 });
            equityMappingList.Add(new EquitySlideMapping() { AttributeGroupId = 1001, AttributeType = "Consumption", AttributeId = 2, Attribute = "Weekly+(Obs. Pop.)", AttributeGroupSortId = 2, AttributeSortId = 2, SlideNumber = 13 });
            equityMappingList.Add(new EquitySlideMapping() { AttributeGroupId = 1001, AttributeType = "Consumption", AttributeId = 3, Attribute = "Daily", AttributeGroupSortId = 2, AttributeSortId = 3, SlideNumber = 13 });
            equityMappingList.Add(new EquitySlideMapping() { AttributeGroupId = 1001, AttributeType = "Consumption", AttributeId = 4, Attribute = "Daily+(Obs. Pop.)", AttributeGroupSortId = 2, AttributeSortId = 4, SlideNumber = 13 });
            equityMappingList.Add(new EquitySlideMapping() { AttributeGroupId = 1000, AttributeType = "Familiarity", AttributeId = 1, Attribute = "Tried", AttributeGroupSortId = 3, AttributeSortId = 1, SlideNumber = 13 });

            return equityMappingList;
        }
        public IActionResult ElasticSearchPracticeCalc(LeftPanelRequest leftPanelRequest)
        {
            //var leftPanelRequest = (await req.GetBodyAsync<LeftPanelRequest>()).Value;
            var geographyDataTable = _leftpanelMapping.GetGeographyMapping();
            var timeperiodDataTable = _leftpanelMapping.GetTimeperiodMapping();
            var productDataTable = _leftpanelMapping.GetProductMapping();
            var filterDataTable = _leftpanelMapping.GetFilterMapping();

            List<PopupLevelData> geoPopupLevels = new List<PopupLevelData>();

            geoPopupLevels.Add(leftPanelRequest.GeographyMenu);

            DataTable geographyTable = GetTableOutputPPT(geoPopupLevels, "GeographyMenu", new DataTable());

            List<PopupLevelData> benchamrkPopupLevels = new List<PopupLevelData>();

            benchamrkPopupLevels.Add(leftPanelRequest.BenchmarkMenu);

            DataTable benchmarkTable = GetTableOutputPPT(benchamrkPopupLevels, "Benchmark", new DataTable());

            DataTable comparisonTable = GetTableOutputPPT(leftPanelRequest.ComparisonMenu, "Comparison", new DataTable());

            DataTable demogsTable = GetTableOutputPPT(leftPanelRequest.FilterMenu, "FilterMenu", new DataTable(), "Filter");

            DataTable beveragesTable = GetTableOutputPPT(leftPanelRequest.FilterMenu, "FilterMenu", new DataTable(), "BeverageFilter");

            benchmarkTable.Merge(comparisonTable);

            
            var listOfGeography = GetGeographySelection(geographyDataTable, geographyTable);
            var listOfMonths = GetTimeperiodSelection(timeperiodDataTable, leftPanelRequest.TimeperiodMenu.id);
            var benchmark = GetBenchmarkComparisons(productDataTable, benchmarkTable.Select("SelectionType='Benchmark'").CopyToDataTable())[0];
            var comparisonList = GetBenchmarkComparisons(productDataTable, comparisonTable);
            var filterList = GetFilterSelection(filterDataTable, demogsTable);
            var cde = _leftpanelMapping.GetFilterMapping().AsEnumerable().Where(a => new List<string> { "Diary - Single Select Questions", "Diary - Multi Select Questions" }.Contains(a.Field<string>("TableName")) && a.Field<object>("AttributeName") != null && a.Field<string>("AttributeName") != "NA").CopyToDataTable();

            #region GeographyCode
            var geoList = new List<string>();
            foreach (var geog in listOfGeography)
            {
                geoList.Add(geog.Geo);
            }
            #endregion

            #region DemogFilterCode
            var demogList = filterList.Where(fil => fil.IsDemog == true).ToList();
            #endregion

            #region OtherFilterCode
            var otherFilter = filterList.Where(fil => fil.IsDemog == false).ToList();
            #endregion

            #region DemogShare

            //var demogShare = demogShare1.Union(demogShare2, new DemogShareEqualityComparer()).ToList();
            var demogShare = GetDemogShareData(listOfGeography, listOfMonths, demogList);
            //var demogShare = new List<DemogShare>();

            //for multselect

            var demogshareMultiSelect = new List<DemogshareMulti>();

            demogshareMultiSelect.AddRange(demogShare.GroupBy(g => new { g.Country_ID }).Select(s => new DemogshareMulti
            {
                Country_ID = s.Key.Country_ID,
                Index_resp = s.Count(),
                Weight = s.Sum(cal => cal.Weight)
            }).ToList());

            var demoShareList = new List<DemogShareWeighted>();

            demoShareList.AddRange(demogShare.GroupBy(g => new { g.Country_ID, g.Gender_Name }).Select(s => new DemogShareWeighted
            {
                Country_ID = s.Key.Country_ID,
                AttributeType = "Gender_Name",
                Attribute = s.Key.Gender_Name,
                Weight = s.Sum(cal => cal.Weight),
                Resp_Count = s.Count()
            }).ToList());

            demoShareList.AddRange(demogShare.GroupBy(g => new { g.Country_ID, g.Age_Range }).Select(s => new DemogShareWeighted
            {
                Country_ID = s.Key.Country_ID,
                AttributeType = "Age_Range",
                Attribute = s.Key.Age_Range,
                Weight = s.Sum(cal => cal.Weight),
                Resp_Count = s.Count()
            }).ToList());

            demoShareList.AddRange(demogShare.GroupBy(g => new { g.Country_ID, g.Ethnicity }).Select(s => new DemogShareWeighted
            {
                Country_ID = s.Key.Country_ID,
                AttributeType = "Ethnicity",
                Attribute = s.Key.Ethnicity,
                Weight = s.Sum(cal => cal.Weight),
                Resp_Count = s.Count()
            }).ToList());

            demoShareList.AddRange(demogShare.GroupBy(g => new { g.Country_ID, g.HH_Size_Group }).Select(s => new DemogShareWeighted
            {
                Country_ID = s.Key.Country_ID,
                AttributeType = "HH_Size_Group",
                Attribute = s.Key.HH_Size_Group,
                Weight = s.Sum(cal => cal.Weight),
                Resp_Count = s.Count()
            }).ToList());

            demoShareList.AddRange(demogShare.GroupBy(g => new { g.Country_ID, g.SEC_Group }).Select(s => new DemogShareWeighted
            {
                Country_ID = s.Key.Country_ID,
                AttributeType = "SEC_Group",
                Attribute = s.Key.SEC_Group,
                Weight = s.Sum(cal => cal.Weight),
                Resp_Count = s.Count()
            }).ToList());

            demoShareList.AddRange(demogShare.GroupBy(g => new { g.Country_ID, g.kids_count }).Select(s => new DemogShareWeighted
            {
                Country_ID = s.Key.Country_ID,
                AttributeType = "kids_count",
                Attribute = s.Key.kids_count,
                Weight = s.Sum(cal => cal.Weight),
                Resp_Count = s.Count()
            }).ToList());

            #endregion

            #region totalShare

            //var totalShare = totalShare1.Union(totalShare2, new TotalShareEqualityComparer()).ToList();

            var totalShare = GetTotalShareData(listOfGeography,listOfMonths);

            //for multiselect

            var totalShareMultiSelect = new List<TotalShareMulti>();

            totalShareMultiSelect.AddRange(totalShare.GroupBy(g => new { g.Country_ID }).Select(s => new TotalShareMulti
            {
                Country_ID = s.Key.Country_ID,
                Index_resp = s.Count(),
                Weight = s.Sum(cal => cal.Weight),
                Population = s.Average(cal => cal.Population)
            }).ToList());

            var totalShareList = new List<TotalShareWeighted>();

            totalShareList.AddRange(totalShare.GroupBy(g => new { g.Country_ID }).Select(s => new TotalShareWeighted
            {
                Country_ID = s.Key.Country_ID,
                Weight = s.Sum(cal => cal.Weight),
                Population = s.Average(cal => cal.Population)
            }).ToList());

            //mulitselect avg population

            var countryAvgPolulationMulti = totalShareList;

            var totalSharePop = (from de in demoShareList
                                 join t in totalShareList on de.Country_ID equals t.Country_ID
                                 select new TotalSharePopulation
                                 {
                                     Country_ID = de.Country_ID,
                                     AttributeType = de.AttributeType,
                                     Attribute = de.Attribute,
                                     Population = t.Weight == 0 ? 0 : (de.Weight / t.Weight) * t.Population
                                 }).ToList();

            var totalSharePopMulti = (from de in demogshareMultiSelect
                                      join t in totalShareMultiSelect on de.Country_ID equals t.Country_ID
                                      select new TotalSharePopulationMulti
                                      {
                                          Country_ID = de.Country_ID,
                                          Population = t.Weight == 0 ? 0 : (de.Weight / t.Weight) * t.Population
                                      }).ToList();


            #endregion

            #region Numerator
            Dictionary<string, object> numeratorParam = new Dictionary<string, object>();
            numeratorParam.Add(listOfGeography[0].GeographyType, geoList);
            numeratorParam.Add("Month_ID", listOfMonths);
            if (filterList.Count > 0)
            {
                foreach (var filter in filterList)
                {
                    numeratorParam.Add(filter.AttributeType, filter.Attributes);
                }
            }


            string outerOperatorTypeNumerator = "AND";
            string innerOperatorTypeNumerator = "OR";

            var compositeNumeratorParam = new List<string> { "Country_ID", "Gender_Name", "Age_Range", "Ethnicity", "HH_Size_Group", "kids_count", "SEC Group" };
            var compositeNumeratorParamNew = new List<string> { "Country_ID", "Gender_Name", "Age_Range", "Ethnicity", "HH_Size_Group", "kids_count", "SEC Group", "index_resp", "Weight" };
            var compositeNumeratorParamNew2 = new List<string> { "Country_ID", "index_resp", "Weight" };
            string[] attributeList = { "Activity_Group", "Activity", "DayPart_Name", "Day_Name", "Occasions_Class", "Channel", "Reason", "Where_Name", "Where_Away_Group" };

            compositeNumeratorParamNew2.AddRange(cde.AsEnumerable().Where(a => attributeList.Contains(a.Field<string>("columnName"))).Select(a => a.Field<string>("AttributeName")).Distinct().ToList()); //&& !(new List<string> { "Working", "Others", "Eating" }.Contains(a.Field<string>("AttributeName")))

            var aggregationParamNum = new Dictionary<string, Dictionary<string, string>>();


            var benchmarkParamNum = new Dictionary<string, string>();
            benchmarkParamNum.Add("Category", benchmark.Category);
            string benchmarkSelection = benchmark.Category;
            if (benchmark.LowLevelCategory != null)
            {
                benchmarkParamNum.Add("Low Level Category", benchmark.LowLevelCategory);
                benchmarkSelection = benchmark.LowLevelCategory;
            }
            if (benchmark.Trademark != null)
            {
                benchmarkParamNum.Add("Trademark", benchmark.Trademark);
                benchmarkSelection = benchmark.Trademark;
            }
            if (benchmark.Brand != null)
            {
                benchmarkParamNum.Add("Brand", benchmark.Brand);
                benchmarkSelection = benchmark.Brand;
            }
            aggregationParamNum.Add(benchmarkSelection + " -- " + "Benchmark", benchmarkParamNum);
            var selectionList = new List<SelectionObj>();
            selectionList.Add(new SelectionObj() { SelectionID = 1, SelectionName = benchmarkSelection, SelectionType = "Benchmark", ConsumptionID = 1, ConsumptionName = "Observed Drinkers" });

            int selectionId = 1;
            foreach (var com in comparisonList)
            {
                var comParamNum = new Dictionary<string, string>();
                comParamNum.Add("Category", com.Category);
                string comSelection = com.Category;
                if (com.LowLevelCategory != null)
                {
                    comParamNum.Add("Low Level Category", com.LowLevelCategory);
                    comSelection = com.LowLevelCategory;
                }
                if (com.Trademark != null)
                {
                    comParamNum.Add("Trademark", com.Trademark);
                    comSelection = com.Trademark;
                }
                if (com.Brand != null)
                {
                    comParamNum.Add("Brand", com.Brand);
                    comSelection = com.Brand;
                }
                aggregationParamNum.Add(comSelection + " -- " + "Comparison", comParamNum);
                selectionList.Add(new SelectionObj() { SelectionID = 1, SelectionName = comSelection, SelectionType = "Comparison", ConsumptionID = 1, ConsumptionName = "Observed Drinkers" });
                selectionId++;
            }

           /* var numeratorCalculationequity = _elasticClient.Search<dynamic>(s => s
                 .Index(drinkIndex)
                 //.Size(100000)
                 .Query(q => _elasticMethod.GetQuery(numeratorParam, innerOperatorTypeNumerator, outerOperatorTypeNumerator, 0))
                 .Source(false)
                 .Aggregations(a => _elasticMethod.GetAggregationQuery(aggregationParamNum, compositeNumeratorParamNew)));*/


            var numeratorCalculation = _elasticClient.Search<dynamic>(s => s
                 .Index(drinkIndex)
                 //.Size(100000)
                 .Query(q => _elasticMethod.GetQuery(numeratorParam, innerOperatorTypeNumerator, outerOperatorTypeNumerator, 0))
                 .Source(false)
                 .Aggregations(a => _elasticMethod.GetAggregationQuery(aggregationParamNum, compositeNumeratorParamNew)));

            #region Weighted Respondent
            var weightedRespondentMultiSelect = GetWeightedRespondentMultiSelect(attributeList,cde,aggregationParamNum,numeratorParam);

            var weightedRespondentMultiSelectTotal = GetWeightedRespondentMultiSelectTotal(aggregationParamNum, numeratorParam);

            #endregion

            #region Reported Drinks
            var reportedDrinkSingleSelect = GetReportedDrinkMultiSelect(attributeList, cde, aggregationParamNum, numeratorParam);

            #region Reported Drink Denominator
            // Denominator
            var reportedRespondentMultiSelectTotal = GetReportedDrinkMultiSelectTotal(aggregationParamNum, numeratorParam);
            #endregion
            #endregion

            #region Num Survey & WP Week
            //Num Survey:

            var numSurveyWeeklyDaily = GetNumSurveys(listOfGeography, listOfMonths, demogList);


            var wpWeekWeeklyDaily = GetWpWeeks(listOfGeography, listOfMonths, demogList,aggregationParamNum);

            var observe_drinker_weighted_weeklyPlus = (from a in wpWeekWeeklyDaily
                                                       join b in numSurveyWeeklyDaily
                                                       on new { a.Resp_Static_ID, a.Country_ID } equals new { b.Resp_Static_ID, b.Country_ID }
                                                       where (a.Freq_Numeric / b.NumSurveys) >= 3
                                                       group a by new { a.Country_ID, a.Selection } into wp
                                                       select new ObserveDrinkerWeightedModel
                                                       {
                                                           Selection = wp.Key.Selection,
                                                           Country_ID = wp.Key.Country_ID,
                                                           ObserveDrinkerWeighted = wp.Sum(x => x.weight)
                                                       }).ToList();

            var observe_drinker_weighted_Daily = (from a in wpWeekWeeklyDaily
                                                  join b in numSurveyWeeklyDaily
                                                  on new { a.Resp_Static_ID, a.Country_ID } equals new { b.Resp_Static_ID, b.Country_ID }
                                                  where (a.Freq_Numeric / b.NumSurveys) >= 4
                                                  group a by new { a.Country_ID, a.Selection } into wp
                                                  select new ObserveDrinkerWeightedModel
                                                  {
                                                      Selection = wp.Key.Selection,
                                                      Country_ID = wp.Key.Country_ID,
                                                      ObserveDrinkerWeighted = wp.Sum(x => x.weight)
                                                  }).ToList();


            #endregion

            #region ObserveDrinkerPopulation

            var ObserverDrinkerPopulation =
                (from a in weightedRespondentMultiSelect
                 join b in demogshareMultiSelect on a.Country_ID equals b.Country_ID
                 join c in totalSharePopMulti on b.Country_ID equals c.Country_ID
                 select new WeightedRespondentMultiSelect()
                 {
                     Country_ID = a.Country_ID,
                     Attribute = a.Attribute,
                     AttributeType = a.AttributeType,
                     Selections = a.Selections,
                     WeightedRespondent = (a.WeightedRespondent / b.Weight) * c.Population //(observeDrinkerWeighted /b.weight)*c.population
                 }
                 ).ToList();

            var ObserverDrinkerPopulationTotal =
                (from a in weightedRespondentMultiSelectTotal
                 join b in demogshareMultiSelect on a.Country_ID equals b.Country_ID
                 join c in totalSharePopMulti on b.Country_ID equals c.Country_ID
                 select new WeightedRespondentMultiSelectDenom()
                 {
                     Country_ID = a.Country_ID,
                     Selections = a.Selections,
                     WeightedRespondent = (a.WeightedRespondent / b.Weight) * c.Population //(observeDrinkerWeighted /b.weight)*c.population
                 }
                 ).ToList();
            var newPopulation = totalSharePopMulti.Sum(x => x.Population);

            var ObserverDrinkerPopulationWeeklyPlus =
                (from a in observe_drinker_weighted_weeklyPlus
                 join b in demogshareMultiSelect on a.Country_ID equals b.Country_ID
                 join c in totalSharePopMulti on b.Country_ID equals c.Country_ID
                 select new WeightedRespondentMultiSelect()
                 {
                     Country_ID = a.Country_ID,
                     Selections = a.Selection,
                     WeightedRespondent = (a.ObserveDrinkerWeighted / b.Weight) * c.Population //(observeDrinkerWeighted /b.weight)*c.population
                 }
                 ).GroupBy(x => new { x.Selections }).Select(x => new ObserveDrinkerPopulationDaily_Weekly
                 {
                     Selections = x.Key.Selections,
                     Weight = x.Sum(s => s.WeightedRespondent) / newPopulation,
                     Population = x.Sum(s => s.WeightedRespondent)
                 }).ToList();

            var ObserverDrinkerPopulationDaily =
                 (from a in observe_drinker_weighted_Daily
                  join b in demogshareMultiSelect on a.Country_ID equals b.Country_ID
                  join c in totalSharePopMulti on b.Country_ID equals c.Country_ID
                  select new WeightedRespondentMultiSelect()
                  {
                      Country_ID = a.Country_ID,
                      Selections = a.Selection,
                      WeightedRespondent = (a.ObserveDrinkerWeighted / b.Weight) * c.Population //(observeDrinkerWeighted /b.weight)*c.population
                  }
                  ).GroupBy(x => new { x.Selections }).Select(x => new ObserveDrinkerPopulationDaily_Weekly
                  {
                      Selections = x.Key.Selections,
                      Weight = x.Sum(s => s.WeightedRespondent) / newPopulation,
                      Population = x.Sum(s => s.WeightedRespondent)
                  }).ToList();

            Dictionary<string, int> numOfDaysInMonth = new Dictionary<string, int>();
            numOfDaysInMonth.Add("202201", 31);
            numOfDaysInMonth.Add("202202", 28);
            numOfDaysInMonth.Add("202203", 31);
            numOfDaysInMonth.Add("202204", 30);
            numOfDaysInMonth.Add("202205", 31);
            numOfDaysInMonth.Add("202206", 30);
            numOfDaysInMonth.Add("202207", 31);
            numOfDaysInMonth.Add("202208", 31);
            numOfDaysInMonth.Add("202209", 30);
            numOfDaysInMonth.Add("202210", 31);
            numOfDaysInMonth.Add("202211", 30);
            numOfDaysInMonth.Add("202212", 31);
            var totalDays = listOfMonths.Select(x => numOfDaysInMonth[x]).Sum();
            var numOfWeeks = (float)totalDays / 7;

            var totalDrinkerTemp = (from a in weightedRespondentMultiSelect
                                    join b in reportedDrinkSingleSelect on new { a.Selections, a.Attribute, a.AttributeType, a.Country_ID } equals new { b.Selections, b.Attribute, b.AttributeType, b.Country_ID }
                                    join c in ObserverDrinkerPopulation on new { b.Selections, b.Attribute, b.AttributeType, b.Country_ID } equals new { c.Selections, c.Attribute, c.AttributeType, c.Country_ID }
                                    select new WeightedRespondentMultiSelect
                                    {
                                        Country_ID = a.Country_ID,
                                        Attribute = a.Attribute,
                                        AttributeType = a.AttributeType,
                                        Selections = a.Selections,
                                        WeightedRespondent = (b.WeightedRespondent / a.WeightedRespondent) * c.WeightedRespondent * numOfWeeks
                                    }
             ).ToList();

            var totalDrinkerNum = (from a in totalDrinkerTemp
                                   group a by new { a.Selections, a.Attribute, a.AttributeType } into tdt
                                   select new TotalDrinker
                                   {
                                       Selections = tdt.Key.Selections,
                                       Attribute = tdt.Key.Attribute,
                                       AttributeType = tdt.Key.AttributeType,
                                       TotalDrinkerSum = tdt.Sum(x => x.WeightedRespondent)
                                   }
                                ).ToList();

            var totalDrinkerDenoTemp = (from a in weightedRespondentMultiSelectTotal
                                        join b in reportedRespondentMultiSelectTotal on new { a.Selections, a.Country_ID } equals new { b.Selections, b.Country_ID }
                                        join c in ObserverDrinkerPopulationTotal on new { b.Selections, b.Country_ID } equals new { c.Selections, c.Country_ID }
                                        select new WeightedRespondentMultiSelectDenom
                                        {
                                            Country_ID = a.Country_ID,
                                            Selections = a.Selections,
                                            WeightedRespondent = (b.WeightedRespondent / a.WeightedRespondent) * c.WeightedRespondent * numOfWeeks
                                        }
           ).ToList();

            var totalDrinkerDeno = (from a in totalDrinkerDenoTemp
                                    group a by new { a.Selections } into tdt
                                    select new TotalDrinker
                                    {
                                        Selections = tdt.Key.Selections,
                                        TotalDrinkerSum = tdt.Sum(x => x.WeightedRespondent)
                                    }
                                ).ToList();

            #endregion

            #region equity
            var equityMappingList = GetEquitySlideMappings();
            var equityNumeratorData = GetEquityNumerator(equityMappingList, aggregationParamNum, numeratorParam);

           

            var equityDenominatorData = _elasticClient.Search<dynamic>(s => s
            .Index(equityIndex)
            .Query(q => _elasticMethod.GetQuery(numeratorParam, innerOperatorTypeNumerator, outerOperatorTypeNumerator, 0))
            .Source(false)
            .Aggregations(a => _elasticMethod.GetAggregationQuery(aggregationParamNum, "Weight", "index_resp")));

            var equityDenominatorDatanew = _elasticClient.Search<dynamic>(s => s
            .Index(equityIndex)
            .Query(q => q
             .Bool(b => b
             .Filter(f => f
             .Term(t => t
              .Field("Fam_Aware")
              .Value(1)
                 )
             )
             .Must(m => _elasticMethod.GetQuery(numeratorParam, innerOperatorTypeNumerator, outerOperatorTypeNumerator, 0))
         )
     ).Source(false)
     .Aggregations(a => _elasticMethod.GetAggregationQuery(aggregationParamNum, "Weight", "Index_resp")));


            var denominatorEquity = new List<EquityWeightDenominator>();
            var denominatorEquityNew = new List<EquityWeightDenominator>();

            foreach (var key in aggregationParamNum.Keys)
            {
                //var bucket = equityDenominatorData.Aggregations.Filter(key).Composite("equityResult").Buckets;
                //var bucket = equityDenominatorData.Aggregations.Filter(key).Composite("numerator").Buckets;
                //foreach (var b in bucket)
                //{
                //    denominatorEquity.Add(new DenominatorEquityData()
                //    {
                //        Selections = key,
                //        Country_ID = b.Key["Country_ID"].ToString(),
                //        Weight = (double)b.Key["Weight"],
                //        Index_Resp = (long)b.Key["index_resp"]
                //    });
                //}

                denominatorEquity.Add(new EquityWeightDenominator()
                {
                    Selections = key,
                    RespCount = (long)equityDenominatorData.Aggregations.Filter(key).Cardinality("colCount").Value,
                    Weight = (double)equityDenominatorData.Aggregations.Filter(key).Sum("weightSum").Value
                });
                denominatorEquityNew.Add(new EquityWeightDenominator()
                {
                    Selections = key,
                    RespCount = (long)equityDenominatorDatanew.Aggregations.Filter(key).Cardinality("colCount").Value,
                    Weight = (double)equityDenominatorDatanew.Aggregations.Filter(key).Sum("weightSum").Value
                });
            }


            var equityIntermediateData = new List<EquityFinalData>();
            equityIntermediateData.AddRange(from a in equityNumeratorData
                                            join b in denominatorEquityNew on a.Selections equals b.Selections
                                            where a.SlideNumber == 12
                                            select new EquityFinalData()
                                            {
                                                Selection = a.Selections,
                                                AttributeType = a.AttributeType,
                                                Attribute = a.Attribute,
                                                SlideNumber = a.SlideNumber,
                                                Percentage = (a.Weight / b.Weight),
                                                BaseSize = b.RespCount
                                            }
             );
            equityIntermediateData.AddRange(from a in equityNumeratorData
                                            join b in denominatorEquity on a.Selections equals b.Selections
                                            where a.SlideNumber == 13
                                            select new EquityFinalData()
                                            {
                                                Selection = a.Selections,
                                                AttributeType = a.AttributeType,
                                                Attribute = a.Attribute,
                                                SlideNumber = a.SlideNumber,
                                                Percentage = (a.Weight / b.Weight),
                                                BaseSize = b.RespCount
                                            }
             );

            var DailyPerMapp = equityMappingList.FindAll(a => a.SlideNumber == 13 && a.AttributeGroupId == 1001 && a.AttributeId == 3).FirstOrDefault();
            var DailyPopMapp = equityMappingList.FindAll(a => a.SlideNumber == 13 && a.AttributeGroupId == 1001 && a.AttributeId == 4).FirstOrDefault();

            var WeeklyPerMapp = equityMappingList.FindAll(a => a.SlideNumber == 13 && a.AttributeGroupId == 1001 && a.AttributeId == 1).FirstOrDefault();
            var WeeklyPopMapp = equityMappingList.FindAll(a => a.SlideNumber == 13 && a.AttributeGroupId == 1001 && a.AttributeId == 2).FirstOrDefault();

            equityIntermediateData.AddRange((
                from a in selectionList
                join b in ObserverDrinkerPopulationDaily on new { Selection = a.SelectionName + " -- " + a.SelectionType } equals new { Selection = b.Selections }
                select new EquityFinalData()
                {
                    SelectionId = a.SelectionID,
                    Selection = b.Selections,
                    SelectionType = a.SelectionType,
                    AttributeType = DailyPerMapp.AttributeType,
                    Attribute = DailyPerMapp.Attribute,
                    AttributeGroupSortId = DailyPerMapp.AttributeGroupSortId,
                    AttributeSortId = DailyPerMapp.AttributeSortId,
                    AttributeGroupId = DailyPerMapp.AttributeGroupId,
                    AttributeId = DailyPerMapp.AttributeId,
                    SlideNumber = DailyPerMapp.SlideNumber,
                    ConsumptionId = a.ConsumptionID,
                    ConsumptioName = a.ConsumptionName,
                    Percentage = b.Weight,
                    BaseSize = 0
                }).ToList());

            equityIntermediateData.AddRange((
                from a in selectionList
                join b in ObserverDrinkerPopulationDaily on new { Selection = a.SelectionName + " -- " + a.SelectionType } equals new { Selection = b.Selections }
                select new EquityFinalData()
                {
                    SelectionId = a.SelectionID,
                    Selection = b.Selections,
                    SelectionType = a.SelectionType,
                    AttributeType = DailyPopMapp.AttributeType,
                    Attribute = DailyPopMapp.Attribute,
                    AttributeGroupSortId = DailyPopMapp.AttributeGroupSortId,
                    AttributeSortId = DailyPopMapp.AttributeSortId,
                    AttributeGroupId = DailyPopMapp.AttributeGroupId,
                    AttributeId = DailyPopMapp.AttributeId,
                    SlideNumber = DailyPopMapp.SlideNumber,
                    ConsumptionId = a.ConsumptionID,
                    ConsumptioName = a.ConsumptionName,
                    Percentage = b.Weight,
                    BaseSize = 0
                }).ToList());

            equityIntermediateData.AddRange((
                from a in selectionList
                join b in ObserverDrinkerPopulationWeeklyPlus on new { Selection = a.SelectionName + " -- " + a.SelectionType } equals new { Selection = b.Selections }
                select new EquityFinalData()
                {
                    SelectionId = a.SelectionID,
                    Selection = b.Selections,
                    SelectionType = a.SelectionType,
                    AttributeType = WeeklyPerMapp.AttributeType,
                    Attribute = WeeklyPerMapp.Attribute,
                    AttributeGroupSortId = WeeklyPerMapp.AttributeGroupSortId,
                    AttributeSortId = WeeklyPerMapp.AttributeSortId,
                    AttributeGroupId = WeeklyPerMapp.AttributeGroupId,
                    AttributeId = WeeklyPerMapp.AttributeId,
                    SlideNumber = WeeklyPerMapp.SlideNumber,
                    ConsumptionId = a.ConsumptionID,
                    ConsumptioName = a.ConsumptionName,
                    Percentage = b.Weight,
                    BaseSize = 0
                }).ToList());

            equityIntermediateData.AddRange((
                from a in selectionList
                join b in ObserverDrinkerPopulationWeeklyPlus on new { Selection = a.SelectionName + " -- " + a.SelectionType } equals new { Selection = b.Selections }
                select new EquityFinalData()
                {
                    SelectionId = a.SelectionID,
                    Selection = b.Selections,
                    SelectionType = a.SelectionType,
                    AttributeType = WeeklyPopMapp.AttributeType,
                    Attribute = WeeklyPopMapp.Attribute,
                    AttributeGroupSortId = WeeklyPopMapp.AttributeGroupSortId,
                    AttributeSortId = WeeklyPopMapp.AttributeSortId,
                    AttributeGroupId = WeeklyPopMapp.AttributeGroupId,
                    AttributeId = WeeklyPopMapp.AttributeId,
                    SlideNumber = WeeklyPopMapp.SlideNumber,
                    ConsumptionId = a.ConsumptionID,
                    ConsumptioName = a.ConsumptionName,
                    Percentage = b.Weight,
                    BaseSize = 0
                }).ToList());

            #endregion
            var numeratorCalculation2 = _elasticClient.Search<dynamic>(s => s
                 .Index(drinkIndex)
                 //.Size(100000)
                 .Query(q => _elasticMethod.GetQuery(numeratorParam, innerOperatorTypeNumerator, outerOperatorTypeNumerator, 0))
                 .Source(false)
                 .Aggregations(a => _elasticMethod.GetAggregationQuery(aggregationParamNum, compositeNumeratorParamNew)));

            var listOfnumerator = new List<NumeratorDemog>();
            var listOfnumeratorNew = new List<NumeratorDemogWeightedNew>();
            foreach (var key in aggregationParamNum.Keys)
            {
                var bucket = numeratorCalculation2.Aggregations.Filter(key).Composite("numerator").Buckets;
                foreach (var b in bucket)
                {
                    foreach (var k in b.Key.AsEnumerable())
                    {
                        if (!new List<string>() { "Country_ID", "Weight", "index_resp" }.Contains(k.Key))
                        {
                            listOfnumeratorNew.Add(new NumeratorDemogWeightedNew()
                            {
                                Selections = key,
                                Country_ID = b.Key["Country_ID"].ToString(),
                                Attribute = k.Key,
                                value = k.Value.ToString(),
                                Weight = (double)b.Key["Weight"],
                                Index_resp = (long)b.Key["index_resp"]
                            });
                        }

                    }
                }
                listOfnumerator.AddRange(numeratorCalculation.Aggregations.Filter(key).Composite("numerator").Buckets.Select(b => new NumeratorDemog()
                {
                    Selections = key,
                    Country_ID = b.Key["Country_ID"].ToString(),
                    Gender_Name = b.Key["Gender_Name"].ToString(),
                    Age_Range = b.Key["Age_Range"].ToString(),
                    Ethnicity = b.Key["Ethnicity"].ToString(),
                    HH_Size_Group = b.Key["HH_Size_Group"].ToString(),
                    kids_count = b.Key["kids_count"].ToString(),
                    SEC_Group = b.Key["SEC Group"].ToString(),
                    Index_resp = Convert.ToInt64(b.Key["index_resp"].ToString()),
                    Weight = Convert.ToDouble(b.Key["Weight"].ToString())
                }));
                //numeratorCalculation.Aggregations.Composite(key).Buckets;
            }
            var res = listOfnumeratorNew.GroupBy(g => new { g.Selections, g.Country_ID, g.Attribute, g.Index_resp, g.Weight, g.value }).Select(s => new NumeratorDemogWeightedNew
            {
                Selections = s.Key.Selections,
                Country_ID = s.Key.Country_ID,
                Attribute = s.Key.Attribute,
                Index_resp = s.Key.Index_resp,
                value = s.Key.value,
                Weight = s.Key.Weight
            }).ToList();
            var numeratorDemogList = new List<NumeratorDemogWeighted>();
            var res2 = res.Where(x => x.value.Equals("1")).GroupBy(g => new { g.Selections, g.Country_ID, g.Attribute }).Select(s => new WeightedRespondentMultiSelect
            {
                Selections = s.Key.Selections,
                Country_ID = s.Key.Country_ID,
                Attribute = s.Key.Attribute,
                WeightedRespondent = s.Sum(x => x.Weight)
            }).ToList();

            numeratorDemogList.AddRange(listOfnumerator.GroupBy(g => new { g.Selections, g.Country_ID, g.Gender_Name }).Select(s => new NumeratorDemogWeighted
            {
                Selections = s.Key.Selections,
                Country_ID = s.Key.Country_ID,
                AttributeType = "Gender_Name",
                Attribute = s.Key.Gender_Name,
                Weight = s.Sum(cal => cal.Weight),
                Resp_Count = s.Count()
            }).ToList());

            numeratorDemogList.AddRange(listOfnumerator.GroupBy(g => new { g.Selections, g.Country_ID, g.Age_Range }).Select(s => new NumeratorDemogWeighted
            {
                Selections = s.Key.Selections,
                Country_ID = s.Key.Country_ID,
                AttributeType = "Age_Range",
                Attribute = s.Key.Age_Range,
                Weight = s.Sum(cal => cal.Weight),
                Resp_Count = s.Count()
            }).ToList());

            numeratorDemogList.AddRange(listOfnumerator.GroupBy(g => new { g.Selections, g.Country_ID, g.Ethnicity }).Select(s => new NumeratorDemogWeighted
            {
                Selections = s.Key.Selections,
                Country_ID = s.Key.Country_ID,
                AttributeType = "Ethnicity",
                Attribute = s.Key.Ethnicity,
                Weight = s.Sum(cal => cal.Weight),
                Resp_Count = s.Count()
            }).ToList());

            numeratorDemogList.AddRange(listOfnumerator.GroupBy(g => new { g.Selections, g.Country_ID, g.HH_Size_Group }).Select(s => new NumeratorDemogWeighted
            {
                Selections = s.Key.Selections,
                Country_ID = s.Key.Country_ID,
                AttributeType = "HH_Size_Group",
                Attribute = s.Key.HH_Size_Group,
                Weight = s.Sum(cal => cal.Weight),
                Resp_Count = s.Count()
            }).ToList());

            numeratorDemogList.AddRange(listOfnumerator.GroupBy(g => new { g.Selections, g.Country_ID, g.SEC_Group }).Select(s => new NumeratorDemogWeighted
            {
                Selections = s.Key.Selections,
                Country_ID = s.Key.Country_ID,
                AttributeType = "SEC_Group",
                Attribute = s.Key.SEC_Group,
                Weight = s.Sum(cal => cal.Weight),
                Resp_Count = s.Count()
            }).ToList());

            numeratorDemogList.AddRange(listOfnumerator.GroupBy(g => new { g.Selections, g.Country_ID, g.kids_count }).Select(s => new NumeratorDemogWeighted
            {
                Selections = s.Key.Selections,
                Country_ID = s.Key.Country_ID,
                AttributeType = "kids_count",
                Attribute = s.Key.kids_count,
                Weight = s.Sum(cal => cal.Weight),
                Resp_Count = s.Count()
            }).ToList());

            DataTable SlideMapping = _leftpanelMapping.GetSlideMapping();
            var demogSelectionMapping = (from a in SlideMapping.AsEnumerable()
                                         join b in selectionList on 1 equals 1
                                         where new List<long> { 4, 5 }.Contains(a.Field<long>("SlideNumber")) && a.Field<bool>("IsNumerator") == true
                                         select new DemogSelectionMapping()
                                         {
                                             SelectionType = b.SelectionType,
                                             SelectionID = b.SelectionID,
                                             SelectionName = b.SelectionName,
                                             ConsumptionID = b.ConsumptionID,
                                             ConsumptionName = b.ConsumptionName,
                                             AttributeGroupSortId = (int)a.Field<long>("AttributeGroupSortID"),
                                             AttributeSortId = (int)a.Field<long>("AttributeSortID"),
                                             SlideNumber = (int)a.Field<long>("SlideNumber"),
                                             AttributeType = a.Field<string>("AttributeGroupName"),
                                             Attribute = a.Field<string>("AttributeName")
                                         }).Distinct(new SlideMappingEqualityComparer()).ToList();



            var numeratorDemogListWithMapping = new List<NumeratorDemogWeightedWithMetric>();
            numeratorDemogListWithMapping.AddRange(
            (from a in numeratorDemogList
             join b in SlideMapping.AsEnumerable() on new { a.AttributeType, a.Attribute } equals new { AttributeType = b.Field<string>("TableColumnName").Replace("SEC Group", "SEC_Group"), Attribute = b.Field<string>("TableMetricName") }
             select new NumeratorDemogWeightedWithMetric
             {
                 Selections = a.Selections,
                 Country_ID = a.Country_ID,
                 AttributeType = a.AttributeType,
                 Attribute = a.Attribute,
                 MappingAttributeType = b.Field<string>("AttributeGroupName"),
                 MappingAttribute = b.Field<string>("AttributeName"),
                 AttributeGroupSortId = (int)b.Field<long>("AttributeGroupSortID"),
                 AttributeSortId = (int)b.Field<long>("AttributeSortID"),
                 SlideNumber = (int)b.Field<long>("SlideNumber"),
                 IsNumerator = b.Field<bool>("IsNumerator"),
                 Weight = a.Weight,
                 Resp_Count = a.Resp_Count
             }
             ).ToList());


            var numeratorDemogIntermediateList = (from a in numeratorDemogListWithMapping
                                                  join b in demoShareList on new { a.Country_ID, a.AttributeType, a.Attribute } equals new { b.Country_ID, b.AttributeType, b.Attribute }
                                                  join c in totalSharePop on new { a.Country_ID, a.AttributeType, a.Attribute } equals new { c.Country_ID, c.AttributeType, c.Attribute }
                                                  select new NumeratorDemogIntermediate
                                                  {
                                                      Country_ID = a.Country_ID,
                                                      Attribute = a.Attribute,
                                                      AttributeType = a.AttributeType,
                                                      AttributeGroupSortId = a.AttributeGroupSortId,
                                                      AttributeSortId = a.AttributeSortId,
                                                      MappingAttribute = a.MappingAttribute,
                                                      MappingAttributeType = a.MappingAttributeType,
                                                      IsNumerator = a.IsNumerator,
                                                      SlideNumber = a.SlideNumber,
                                                      Resp_Count = a.Resp_Count,
                                                      Selections = a.Selections,
                                                      WeeklyWeight = a.Weight,
                                                      incidenceWeight = b.Weight,
                                                      Population = c.Population,
                                                      DrinkerPopulation = b.Weight == 0 ? 0 : (a.Weight / b.Weight) * c.Population
                                                  }
                 ).ToList();

            var numeratorDemogFinal = (from a in numeratorDemogIntermediateList
                                       group a by new { a.Selections, a.AttributeType, /*a.Attribute,*/a.MappingAttributeType, a.MappingAttribute, a.IsNumerator, a.SlideNumber, a.AttributeGroupSortId, a.AttributeSortId } into grouped
                                       select new NumeratorDemogFinal
                                       {
                                           Selections = grouped.Key.Selections,
                                           AttributeType = grouped.Key.AttributeType,
                                           //Attribute = grouped.Key.Attribute,
                                           AttributeGroupSortId = grouped.Key.AttributeGroupSortId,
                                           AttributeSortId = grouped.Key.AttributeSortId,
                                           MappingAttributeType = grouped.Key.MappingAttributeType,
                                           MappingAttribute = grouped.Key.MappingAttribute,
                                           SlideNumber = grouped.Key.SlideNumber,
                                           IsNumerator = grouped.Key.IsNumerator,
                                           Weight = grouped.Sum(a => a.DrinkerPopulation),
                                           Resp_Count = grouped.Sum(a => a.Resp_Count)
                                       }
            ).ToList();

            var denominatorDemogFinal = (from a in numeratorDemogFinal
                                         where (a.MappingAttributeType == "Ethnicity" && new List<int> { 1, 2 }.Contains(a.AttributeSortId)) || a.MappingAttributeType != "Ethnicity"
                                         group a by new { a.Selections, a.AttributeType, a.MappingAttributeType, a.SlideNumber, a.AttributeGroupSortId } into grouped
                                         select new DenominatorDemogFinal
                                         {
                                             Selections = grouped.Key.Selections,
                                             AttributeType = grouped.Key.AttributeType,
                                             AttributeGroupSortId = grouped.Key.AttributeGroupSortId,
                                             MappingAttributeType = grouped.Key.MappingAttributeType,
                                             SlideNumber = grouped.Key.SlideNumber,
                                             Weight = grouped.Sum(a => a.Weight),
                                             Resp_Count = grouped.Sum(a => a.Resp_Count)
                                         }
            ).ToList();

            var finalIntermediateDemog = (from a in numeratorDemogFinal
                                          join b in denominatorDemogFinal on new { a.Selections, a.AttributeGroupSortId, a.AttributeType, a.MappingAttributeType, a.SlideNumber } equals new { b.Selections, b.AttributeGroupSortId, b.AttributeType, b.MappingAttributeType, b.SlideNumber }
                                          select new FinalIntermediateDemog
                                          {
                                              SelectionType = a.Selections.Split(" -- ")[1],
                                              Selection = a.Selections.Split(" -- ")[0],
                                              AttributeType = a.AttributeType,
                                              AttributeGroupSortId = a.AttributeGroupSortId,
                                              MappingAttributeType = a.MappingAttributeType,
                                              /*Attribute = a.Attribute,*/
                                              MappingAttribute = a.MappingAttribute,
                                              AttributeSortId = a.AttributeSortId,
                                              SlideNumber = a.SlideNumber,
                                              IsNumerator = a.IsNumerator,
                                              Numerator = a.Weight,
                                              Denominator = b.Weight,
                                              Percentage = (a.Weight / b.Weight),
                                              SampleSize = a.Resp_Count,
                                              BaseSize = b.Resp_Count

                                          }
            ).ToList();

            var finalTableDemog = new List<FinalTableDemog>();
            finalTableDemog.AddRange((from a in finalIntermediateDemog
                                      where a.SelectionType == "Benchmark"
                                      select new FinalTableDemog
                                      {
                                          SelectionType = a.SelectionType,
                                          Selection = a.Selection,
                                          AttributeType = a.AttributeType,
                                          AttributeGroupSortId = a.AttributeGroupSortId,
                                          MappingAttributeType = a.MappingAttributeType,
                                          /*Attribute = a.Attribute,*/
                                          MappingAttribute = a.MappingAttribute,
                                          AttributeSortId = a.AttributeSortId,
                                          SlideNumber = a.SlideNumber,
                                          IsNumerator = a.IsNumerator,
                                          Numerator = a.Numerator,
                                          Denominator = a.Denominator,
                                          Percentage = a.Percentage,
                                          SampleSize = a.SampleSize,
                                          BaseSize = a.BaseSize,
                                          Significance = 0

                                      }
            ).ToList());

            finalTableDemog.AddRange((from a in finalIntermediateDemog
                                      join b in finalIntermediateDemog on new { a.AttributeGroupSortId, a.AttributeSortId, a.AttributeType/*,a.Attribute*/, a.MappingAttributeType, a.MappingAttribute, a.SlideNumber }
                                                                   equals new { b.AttributeGroupSortId, b.AttributeSortId, b.AttributeType/*, b.Attribute*/, b.MappingAttributeType, b.MappingAttribute, b.SlideNumber }
                                      where a.SelectionType == "Comparison" && b.SelectionType == "Benchmark"
                                      select new FinalTableDemog
                                      {
                                          SelectionType = a.SelectionType,
                                          Selection = a.Selection,
                                          AttributeType = a.AttributeType,
                                          AttributeGroupSortId = a.AttributeGroupSortId,
                                          MappingAttributeType = a.MappingAttributeType,
                                          /*Attribute = a.Attribute,*/
                                          MappingAttribute = a.MappingAttribute,
                                          AttributeSortId = a.AttributeSortId,
                                          SlideNumber = a.SlideNumber,
                                          IsNumerator = a.IsNumerator,
                                          Numerator = a.Numerator,
                                          Denominator = a.Denominator,
                                          Percentage = a.Percentage,
                                          SampleSize = a.SampleSize,
                                          BaseSize = a.BaseSize,
                                          Significance = SignificanceValue(b.Numerator, b.BaseSize, a.Numerator, a.BaseSize)


                                      }
            ).ToList());

            var finalResultDemog = (from a in demogSelectionMapping
                                    join b in finalTableDemog
                                    //on new { a.SlideNumber, a.SelectionType, a.SelectionName, a.AttributeType, a.AttributeGroupSortId, a.AttributeSortId } equals new { b.SlideNumber, b.SelectionType, b.Selection, b.MappingAttributeType, b.AttributeGroupSortId, b.AttributeSortId } into joinedItems
                                    //on new { a=a.SlideNumber,c=a.SelectionName } equals new { b = b.SlideNumber,d=b.Selection } into joinedItems
                                    //on a.SlideNumber equals b.SlideNumber into joinedItems
                                    on new { SlideNumber = a.SlideNumber, SelectionType = a.SelectionType, SelectionName = a.SelectionName, AttributeType = a.AttributeType, AttributeGroupSortId = a.AttributeGroupSortId, AttributeSortId = a.AttributeSortId } equals new { SlideNumber = b.SlideNumber, SelectionType = b.SelectionType, SelectionName = b.Selection, AttributeType = b.MappingAttributeType, AttributeGroupSortId = b.AttributeGroupSortId, AttributeSortId = b.AttributeSortId } into joinedItems
                                    from joinItem in joinedItems.DefaultIfEmpty()
                                    orderby a.SlideNumber, a.SelectionType, a.SelectionID, a.SelectionName, a.AttributeGroupSortId, a.AttributeSortId
                                    select new PPTBindingData()
                                    {
                                        SelectionId = a.SelectionID,
                                        SelectionName = a.SelectionName,
                                        SelectionType = a.SelectionType,
                                        ConsumptionID = a.ConsumptionID,
                                        ConsumptionName = a.ConsumptionName,
                                        SlideNumber = a.SlideNumber,
                                        IsCategory = 0,
                                        SortId = a.AttributeSortId,
                                        GroupSort = a.AttributeGroupSortId,
                                        MetricType = a.AttributeType,
                                        Metric = a.Attribute,
                                        Percentage = (decimal)((joinItem==null || joinItem.Percentage == null) ? 0 : joinItem.Percentage),
                                        Significance = (decimal)((joinItem == null || joinItem.Significance == null) ? 0 : joinItem.Significance)

                                    }
            ).ToList();

            #region Template for MultiSelect and Single Select Slide
            var ActivityList = (from a in cde.AsEnumerable()
                                join c in selectionList on 1 equals 1
                                join b in totalDrinkerNum
                                on new { AttributeType = a.Field<string>("columnName"), Attribute = a.Field<string>("AttributeName"), Selection = c.SelectionName + " -- " + c.SelectionType } equals new { AttributeType = b.AttributeType, Attribute = b.Attribute, Selection = b.Selections }
                                into joinedItems
                                from joinItem in joinedItems.DefaultIfEmpty()
                                where new List<string> { "Activity" }.Contains(a.Field<string>("columnName"))
                                select new MultiSelectMapping
                                {
                                    SelectionType = c.SelectionType,
                                    SelectionID = c.SelectionID,
                                    Selection = c.SelectionName + " -- " + c.SelectionType,
                                    AttributeGroupId = (int)a.Field<long>("AttributeGroupID"),
                                    AttributeId = (int)a.Field<long>("AttributeId"),
                                    AttributeType = a.Field<string>("columnName"),
                                    Attribute = a.Field<string>("AttributeName"),
                                    SlideNumber = 7,
                                    AttributeGroupSortId = 1,
                                    ConsumptionId = c.ConsumptionID,
                                    ConsumptionName = c.ConsumptionName,
                                    TotalDrinker = (joinItem == null || joinItem.TotalDrinkerSum==null)?0:joinItem.TotalDrinkerSum
                                }).ToList();

            var ActivityMapping = ActivityList.Where(a => a.SelectionType == "Benchmark").OrderByDescending(a => a.TotalDrinker).Select((x, i) => new MultiSelectMapping
            {
                SelectionType = x.SelectionType,
                SelectionID = x.SelectionID,
                Selection = x.Selection,
                AttributeGroupId = x.AttributeGroupId,
                AttributeId = x.AttributeId,
                AttributeType = x.AttributeType,
                Attribute = x.Attribute,
                SlideNumber = x.SlideNumber,
                AttributeGroupSortId = x.AttributeGroupSortId,
                AttributeSortId = i + 1,
                ConsumptionId = x.ConsumptionId,
                ConsumptionName = x.ConsumptionName,
                TotalDrinker = x.TotalDrinker
            }).Take(10).ToList();

            var finalMultiSelectList = (
                from x in ActivityList
                join b in ActivityMapping on new { x.AttributeGroupId, x.AttributeId } equals new { b.AttributeGroupId, b.AttributeId }
                select new MultiSelectMapping()
                {
                    SelectionType = x.SelectionType,
                    SelectionID = x.SelectionID,
                    Selection = x.Selection,
                    AttributeGroupId = x.AttributeGroupId,
                    AttributeId = x.AttributeId,
                    AttributeType = x.AttributeType,
                    Attribute = x.Attribute,
                    SlideNumber = x.SlideNumber,
                    AttributeGroupSortId = x.AttributeGroupSortId,
                    AttributeSortId = b.AttributeSortId,

                    ConsumptionId = x.ConsumptionId,
                    ConsumptionName = x.ConsumptionName,
                    TotalDrinker = x.TotalDrinker
                }
                ).ToList();


            var ReasonList = (from a in cde.AsEnumerable()
                              join c in selectionList on 1 equals 1
                              join b in totalDrinkerNum
                              on new { AttributeType = a.Field<string>("columnName"), Attribute = a.Field<string>("AttributeName"), Selection = c.SelectionName + " -- " + c.SelectionType } equals new { AttributeType = b.AttributeType, Attribute = b.Attribute, Selection = b.Selections }
                              into joinedItems
                              from joinItem in joinedItems.DefaultIfEmpty()
                              where new List<string> { "Reason" }.Contains(a.Field<string>("columnName"))
                              select new MultiSelectMapping
                              {
                                  SelectionType = c.SelectionType,
                                  SelectionID = c.SelectionID,
                                  Selection = c.SelectionName + " -- " + c.SelectionType,
                                  AttributeGroupId = (int)a.Field<long>("AttributeGroupID"),
                                  AttributeId = (int)a.Field<long>("AttributeId"),
                                  AttributeType = a.Field<string>("columnName"),
                                  Attribute = a.Field<string>("AttributeName"),
                                  SlideNumber = 10,
                                  AttributeGroupSortId = 1,
                                  ConsumptionId = c.ConsumptionID,
                                  ConsumptionName = c.ConsumptionName,
                                  TotalDrinker = (joinItem == null || joinItem.TotalDrinkerSum == null) ? 0 : joinItem.TotalDrinkerSum
                              }).ToList();

            var ReasonMapping = ReasonList.Where(a => a.SelectionType == "Benchmark").OrderByDescending(a => a.TotalDrinker).Select((x, i) => new MultiSelectMapping
            {
                SelectionType = x.SelectionType,
                SelectionID = x.SelectionID,
                Selection = x.Selection,
                AttributeGroupId = x.AttributeGroupId,
                AttributeId = x.AttributeId,
                AttributeType = x.AttributeType,
                Attribute = x.Attribute,
                SlideNumber = x.SlideNumber,
                AttributeGroupSortId = x.AttributeGroupSortId,
                AttributeSortId = i + 1,
                ConsumptionId = x.ConsumptionId,
                ConsumptionName = x.ConsumptionName,
                TotalDrinker = x.TotalDrinker
            }).Take(10).ToList();

            finalMultiSelectList.AddRange((
                from x in ReasonList
                join b in ReasonMapping on new { x.AttributeGroupId, x.AttributeId } equals new { b.AttributeGroupId, b.AttributeId }
                select new MultiSelectMapping()
                {
                    SelectionType = x.SelectionType,
                    SelectionID = x.SelectionID,
                    Selection = x.Selection,
                    AttributeGroupId = x.AttributeGroupId,
                    AttributeId = x.AttributeId,
                    AttributeType = x.AttributeType,
                    Attribute = x.Attribute,
                    SlideNumber = x.SlideNumber,
                    AttributeGroupSortId = x.AttributeGroupSortId,
                    AttributeSortId = b.AttributeSortId,
                    ConsumptionId = x.ConsumptionId,
                    ConsumptionName = x.ConsumptionName,
                    TotalDrinker = x.TotalDrinker
                }
                ).ToList());


            var ActivityGroupList = (from a in cde.AsEnumerable()
                                     join c in selectionList on 1 equals 1
                                     join b in totalDrinkerNum
                                     on new { AttributeType = a.Field<string>("columnName"), Attribute = a.Field<string>("AttributeName"), Selection = c.SelectionName + " -- " + c.SelectionType } equals new { AttributeType = b.AttributeType, Attribute = b.Attribute, Selection = b.Selections }
                                     into joinedItems
                                     from joinItem in joinedItems.DefaultIfEmpty()
                                     where new List<string> { "Activity_Group" }.Contains(a.Field<string>("columnName"))
                                     select new MultiSelectMapping
                                     {
                                         SelectionType = c.SelectionType,
                                         SelectionID = c.SelectionID,
                                         Selection = c.SelectionName + " -- " + c.SelectionType,
                                         AttributeGroupId = (int)a.Field<long>("AttributeGroupID"),
                                         AttributeId = (int)a.Field<long>("AttributeId"),
                                         AttributeType = a.Field<string>("columnName"),
                                         Attribute = a.Field<string>("AttributeName"),
                                         SlideNumber = 6,
                                         AttributeGroupSortId = 1,
                                         ConsumptionId = c.ConsumptionID,
                                         ConsumptionName = c.ConsumptionName,
                                         TotalDrinker = (joinItem == null || joinItem.TotalDrinkerSum == null) ? 0 : joinItem.TotalDrinkerSum
                                     }).ToList();

            var ActivityGroupMapping = ActivityGroupList.Where(a => a.SelectionType == "Benchmark").OrderByDescending(a => a.TotalDrinker).Select((x, i) => new MultiSelectMapping
            {
                SelectionType = x.SelectionType,
                SelectionID = x.SelectionID,
                Selection = x.Selection,
                AttributeGroupId = x.AttributeGroupId,
                AttributeId = x.AttributeId,
                AttributeType = x.AttributeType,
                Attribute = x.Attribute,
                SlideNumber = x.SlideNumber,
                AttributeGroupSortId = x.AttributeGroupSortId,
                AttributeSortId = i + 1,
                ConsumptionId = x.ConsumptionId,
                ConsumptionName = x.ConsumptionName,
                TotalDrinker = x.TotalDrinker
            }).Take(9).ToList();

            finalMultiSelectList.AddRange((
                from x in ActivityGroupList
                join b in ActivityGroupMapping on new { x.AttributeGroupId, x.AttributeId } equals new { b.AttributeGroupId, b.AttributeId }
                select new MultiSelectMapping()
                {
                    SelectionType = x.SelectionType,
                    SelectionID = x.SelectionID,
                    Selection = x.Selection,
                    AttributeGroupId = x.AttributeGroupId,
                    AttributeId = x.AttributeId,
                    AttributeType = x.AttributeType,
                    Attribute = x.Attribute,
                    SlideNumber = x.SlideNumber,
                    AttributeGroupSortId = x.AttributeGroupSortId,
                    ConsumptionId = x.ConsumptionId,
                    ConsumptionName = x.ConsumptionName,
                    AttributeSortId = b.AttributeSortId,
                    TotalDrinker = x.TotalDrinker
                }
                ).ToList());


            var singleMapping = (from a in cde.AsEnumerable()
                                 join s in SlideMapping.AsEnumerable() on new { AttributeType = a.Field<string>("columnName").Trim().ToLower(), Attribute = a.Field<string>("AttributeName").Trim().ToLower() } equals new { AttributeType = s.Field<string>("TableColumnName").Trim().ToLower(), Attribute = s.Field<string>("TableMetricName").Trim().ToLower() }
                                 join c in selectionList on 1 equals 1
                                 where new List<int> { 8, 9, 14, 15 }.Contains((int)s.Field<long>("SlideNumber")) && !(((int)a.Field<long>("AttributeGroupID") == 45) && ((int)a.Field<long>("AttributeId") == 1))
                                 select new MultiSelectMapping
                                 {
                                     SelectionType = c.SelectionType,
                                     SelectionID = c.SelectionID,
                                     Selection = c.SelectionName + " -- " + c.SelectionType,
                                     AttributeGroupId = (int)a.Field<long>("AttributeGroupID"),
                                     AttributeId = (int)a.Field<long>("AttributeId"),
                                     AttributeType = a.Field<string>("columnName"),
                                     Attribute = a.Field<string>("AttributeName"),
                                     SlideNumber = (int)s.Field<long>("SlideNumber") == 15 ? 11 : ((int)s.Field<long>("SlideNumber") == 14 ? 15 : (int)s.Field<long>("SlideNumber")),
                                     AttributeGroupSortId = (int)s.Field<long>("AttributeGroupSortID"),
                                     AttributeSortId = (int)s.Field<long>("AttributeSortID"),
                                     ConsumptionId = c.ConsumptionID,
                                     ConsumptionName = c.ConsumptionName,
                                     TotalDrinker = 0
                                 }).ToList();


            var singleIntermediate = (from a in singleMapping
                                      join b in totalDrinkerNum
                                      on new { AttributeType = a.AttributeType, Attribute = a.Attribute, Selection = a.Selection } equals new { AttributeType = b.AttributeType, Attribute = b.Attribute, Selection = b.Selections }
                          into joinedItems
                                      from joinItem in joinedItems.DefaultIfEmpty()
                                      select new MultiSelectMapping
                                      {
                                          SelectionType = a.SelectionType,
                                          SelectionID = a.SelectionID,
                                          Selection = a.Selection,
                                          AttributeGroupId = a.AttributeGroupId,
                                          AttributeId = a.AttributeId,
                                          AttributeType = a.AttributeType,
                                          Attribute = a.Attribute,
                                          SlideNumber = a.SlideNumber,
                                          AttributeGroupSortId = a.AttributeGroupSortId,
                                          AttributeSortId = a.AttributeSortId,
                                          ConsumptionId = a.ConsumptionId,
                                          ConsumptionName = a.ConsumptionName,
                                          TotalDrinker = (joinItem == null || joinItem.TotalDrinkerSum == null) ? 0 : joinItem.TotalDrinkerSum
                                      }).ToList();

            finalMultiSelectList.AddRange((from a in singleIntermediate
                                           join b in SlideMapping.AsEnumerable()
                                           on new { AttributeType = a.AttributeType.Trim().ToLower(), Attribute = a.Attribute.Trim().ToLower() } equals new { AttributeType = b.Field<string>("TableColumnName").Trim().ToLower(), Attribute = b.Field<string>("TableMetricName").Trim().ToLower() }
                                           select new MultiSelectMapping
                                           {
                                               SelectionType = a.SelectionType,
                                               SelectionID = a.SelectionID,
                                               Selection = a.Selection,
                                               AttributeGroupId = a.AttributeGroupId,
                                               AttributeId = a.AttributeId,
                                               AttributeType = b.Field<string>("AttributeGroupName"),
                                               Attribute = b.Field<string>("AttributeName"),
                                               SlideNumber = a.SlideNumber,
                                               AttributeGroupSortId = a.AttributeGroupSortId,
                                               AttributeSortId = a.AttributeSortId,
                                               ConsumptionId = a.ConsumptionId,
                                               ConsumptionName = a.ConsumptionName,
                                               TotalDrinker = a.TotalDrinker
                                           }).ToList());


            var finalSlideData = (from a in finalMultiSelectList
                                  join b in totalDrinkerDeno
                                  on a.Selection equals b.Selections
                                  select new PPTBindingData
                                  {
                                      SelectionId = a.SelectionID,
                                      SelectionName = a.Selection.Split(" -- ")[0],
                                      SelectionType = a.SelectionType,
                                      ConsumptionID = a.ConsumptionId,
                                      ConsumptionName = a.ConsumptionName,
                                      SlideNumber = a.SlideNumber,
                                      IsCategory = 0,
                                      SortId = a.AttributeSortId,
                                      GroupSort = a.AttributeGroupSortId,
                                      MetricType = a.AttributeType,
                                      Metric = a.Attribute,
                                      Percentage = (decimal)(a.TotalDrinker / b.TotalDrinkerSum),
                                      Significance = 0
                                  }
                                  ).ToList();

            #endregion

            var equityConsumptionTemplate = (from a in equityMappingList
                                             join b in selectionList on 1 equals 1
                                             join c in equityIntermediateData
                                             on new { Selection = b.SelectionName + " -- " + b.SelectionType, AttributeType = a.AttributeType, Attribute = a.Attribute, SlideNumber = a.SlideNumber } equals
                                                new { Selection = c.Selection, AttributeType = c.AttributeType, Attribute = c.Attribute, SlideNumber = c.SlideNumber }
                                                into joinedItems
                                             from joinItem in joinedItems.DefaultIfEmpty()
                                             select new EquityFinalData()
                                             {
                                                 SelectionId = b.SelectionID,
                                                 SelectionType = b.SelectionType,
                                                 Selection = b.SelectionName,
                                                 SlideNumber = a.SlideNumber,
                                                 AttributeGroupSortId = a.AttributeGroupSortId,
                                                 AttributeSortId = a.AttributeSortId,
                                                 AttributeGroupId = a.AttributeGroupId,
                                                 AttributeId = a.AttributeId,
                                                 AttributeType = a.AttributeType,
                                                 Attribute = a.Attribute,
                                                 ConsumptionId = b.ConsumptionID,
                                                 ConsumptioName = b.ConsumptionName,
                                                 Percentage = (joinItem == null || joinItem.Percentage==null || Double.IsNaN(joinItem.Percentage)) ? 0 : joinItem.Percentage
                                             }
                                             ).ToList();

            List<PPTBindingData> pptBindingDataFinal = new List<PPTBindingData>();
            pptBindingDataFinal.AddRange(finalSlideData);
            pptBindingDataFinal.AddRange(equityConsumptionTemplate.Select(a => new PPTBindingData
            {
                SelectionId = a.SelectionId,
                SelectionName = a.Selection,
                SelectionType = a.SelectionType,
                ConsumptionID = a.ConsumptionId,
                ConsumptionName = a.ConsumptioName,
                SlideNumber = a.SlideNumber,
                IsCategory = 0,
                SortId = a.AttributeSortId,
                GroupSort = a.AttributeGroupSortId,
                MetricType = a.AttributeType,
                Metric = a.Attribute,
                Percentage = (decimal)a.Percentage,
                Significance = 0
            }));


            MemoryStream fileData = null;
            try
            {
                fileData = _reportGeneratorBusiness.FormatDataNew(leftPanelRequest, finalResultDemog, pptBindingDataFinal.OrderBy(o => o.SlideNumber).ThenBy(o => o.SelectionType).ThenBy(o => o.SelectionId).ThenBy(o => o.GroupSort).ThenBy(o => o.SortId).ToList());
            }
            catch (Exception ex)
            {

            }

            //var fileData = _reportGeneratorBusiness.FormatDataNew(leftPanelRequest, finalResultDemog, new List<PPTBindingData>());
            #endregion
            byte[] bytes = fileData.ToArray();
            fileData.Dispose();
            //resultNew.Aggregations.Composite("gr").Buckets;

            return new FileContentResult(bytes, "application/vnd.openxmlformats-officedocument.presentationml.presentation")
            {
                FileDownloadName = "Output.pptx"
            };
        }
        private System.Data.DataTable GetTableOutputPPT(List<PopupLevelData> selection, string stubType, System.Data.DataTable table)
        {
            if (stubType.Equals("GeographyMenu", StringComparison.OrdinalIgnoreCase))
            {
                table.Columns.Add("OUID", typeof(Int32));
                table.Columns.Add("CountryID", typeof(Int32));
                table.Columns.Add("RegionID", typeof(Int32));
                table.Columns.Add("BottlerID", typeof(Int32));

                foreach (var item in selection)
                {
                    DataRow geographyRow = table.NewRow();
                    if (item.type.Equals("Total", StringComparison.OrdinalIgnoreCase))
                    {
                        geographyRow[0] = -1;
                        geographyRow[1] = -1;
                        geographyRow[2] = -1;
                        geographyRow[3] = -1;
                    }
                    else if (item.type.Equals("OU", StringComparison.OrdinalIgnoreCase))
                    {
                        geographyRow[0] = Convert.ToInt32(item.metricId);
                        geographyRow[1] = DBNull.Value;
                        geographyRow[2] = DBNull.Value;
                        geographyRow[3] = DBNull.Value;
                    }
                    else if (item.type.Equals("Country", StringComparison.OrdinalIgnoreCase))
                    {
                        geographyRow[0] = DBNull.Value;
                        geographyRow[1] = Convert.ToInt32(item.metricId);
                        geographyRow[2] = DBNull.Value;
                        geographyRow[3] = DBNull.Value;
                    }
                    else if (item.type.Equals("Region", StringComparison.OrdinalIgnoreCase))
                    {
                        geographyRow[0] = DBNull.Value;
                        geographyRow[1] = Convert.ToInt32(item.parentMetricId);
                        geographyRow[2] = Convert.ToInt32(item.metricId);
                        geographyRow[3] = DBNull.Value;
                    }
                    else
                    {
                        geographyRow[0] = DBNull.Value;
                        geographyRow[1] = Convert.ToInt32(item.parentMetricId);
                        geographyRow[2] = DBNull.Value;
                        geographyRow[3] = Convert.ToInt32(item.metricId);
                    }
                    table.Rows.Add(geographyRow);
                }
            }
            else if (stubType.Equals("Benchmark", StringComparison.OrdinalIgnoreCase) || stubType.Equals("Comparison", StringComparison.OrdinalIgnoreCase))
            {
                int index = 0;
                if (stubType.Equals("Benchmark", StringComparison.OrdinalIgnoreCase))
                {
                    index = 100;
                }
                else
                {
                    index = 200;
                }

                table.Columns.Add("CategoryID", typeof(Int32));
                table.Columns.Add("LowLevelCategoryID", typeof(Int32));
                table.Columns.Add("TrademarkID", typeof(Int32));
                table.Columns.Add("BrandID", typeof(Int32));
                table.Columns.Add("ConsumptionID", typeof(Int32));
                table.Columns.Add("SelectionType", typeof(string));
                table.Columns.Add("SelectionID", typeof(Int32));
                table.Columns.Add("SelectionName", typeof(string));

                if (selection.Count > 0)
                {
                    foreach (var item in selection)
                    {
                        string[] parentMap = item.parentMap.Split(",");
                        DataRow benchmarkRow = table.NewRow();
                        benchmarkRow[0] = Convert.ToInt32(parentMap[0]);
                        if (1 < parentMap.Length)
                        {
                            benchmarkRow[1] = Convert.ToInt32(parentMap[1]);
                        }
                        else
                        {
                            benchmarkRow[1] = DBNull.Value;
                        }

                        benchmarkRow[4] = Convert.ToInt32(item.consumptionId);
                        benchmarkRow[5] = stubType;
                        //benchmarkRow[6] = Convert.ToInt32(item.id);
                        benchmarkRow[6] = Convert.ToInt32(index);
                        benchmarkRow[7] = item.text;
                        if (item.type.Equals("Trademark", StringComparison.OrdinalIgnoreCase))
                        {
                            benchmarkRow[2] = Convert.ToInt32(item.metricId);
                            benchmarkRow[3] = DBNull.Value;
                        }
                        else if (item.type.Equals("Brand", StringComparison.OrdinalIgnoreCase))
                        {
                            benchmarkRow[2] = DBNull.Value;
                            benchmarkRow[3] = Convert.ToInt32(item.metricId);
                        }
                        table.Rows.Add(benchmarkRow);
                        index++;
                    }
                }
            }
            return table;
        }
        private System.Data.DataTable GetTableOutputPPT(List<PopupLevelData> selection, string stubType, System.Data.DataTable table, string filterType)
        {
            if (filterType.Equals("Filter", StringComparison.OrdinalIgnoreCase))
            {
                table.Columns.Add("Attributegroupid", typeof(Int32));
                table.Columns.Add("Attributeid", typeof(Int32));
                foreach (var item in selection)
                {
                    DataRow demogsRow = table.NewRow();
                    string[] parentMap = item.parentMap.Split(",");
                    if (item.type.Equals("Filter", StringComparison.OrdinalIgnoreCase))
                    {
                        demogsRow[0] = Convert.ToInt32(parentMap[1]);
                        demogsRow[1] = Convert.ToInt32(parentMap[2]);
                        table.Rows.Add(demogsRow);
                    }
                }
            }
            else if (filterType.Equals("BeverageFilter", StringComparison.OrdinalIgnoreCase))
            {
                table.Columns.Add("CategoryID", typeof(Int32));
                table.Columns.Add("LowLevelCategoryID", typeof(Int32));
                table.Columns.Add("TrademarkID", typeof(Int32));
                table.Columns.Add("BrandID", typeof(Int32));
                table.Columns.Add("ConsumptionID", typeof(Int32));
                foreach (var item in selection)
                {
                    DataRow beveragesRow = table.NewRow();
                    string[] parentMap = item.parentMap.Split(",");
                    if (item.type.Equals("BeverageFilter", StringComparison.OrdinalIgnoreCase))
                    {
                        beveragesRow[0] = Convert.ToInt32(parentMap[1]);

                        if (item.levelId >= 3)
                        {
                            beveragesRow[1] = Convert.ToInt32(parentMap[2]);
                        }
                        else
                        {
                            beveragesRow[1] = DBNull.Value;
                        }
                        if (item.levelId == 4)
                        {
                            beveragesRow[2] = Convert.ToInt32(item.metricId);
                        }
                        else
                        {
                            beveragesRow[2] = DBNull.Value;
                        }
                        if (item.levelId == 5)
                        {
                            beveragesRow[3] = Convert.ToInt32(item.metricId);
                        }
                        else
                        {
                            beveragesRow[3] = DBNull.Value;
                        }
                        beveragesRow[4] = item.consumptionId;
                        table.Rows.Add(beveragesRow);
                    }
                }
            }
            return table;
        }
    }
}
