using coke_beach_reportGenerator_api.Functions.ElasticSearch;
using coke_beach_reportGenerator_api.Models;
using coke_beach_reportGenerator_api.Models.LeftPanelModel;
using coke_beach_reportGenerator_api.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Nest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace coke_beach_reportGenerator_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ElasticSearchPractice : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ILeftPanelBusiness _leftPanelBusiness;
        private LeftPanel _leftPanelData = null;
        private ILeftPanelMapping _leftpanelMapping;
        IElasticClient _elasticClient = null;
        private IReportGeneratorBusiness _reportGeneratorBusiness;
        private ElasticSearchPracticeSlideCalculation elasticSearchPracticeSlideCalculation;

        public ElasticSearchPractice(ILeftPanelBusiness leftPanelBusiness, IHostingEnvironment hostingEnvironment, IElasticClient client, IReportGeneratorBusiness reportGeneratorBusiness, ILeftPanelMapping leftPanelMapping, IConfiguration config)
        {
            _leftPanelBusiness = leftPanelBusiness;
            _hostingEnvironment = hostingEnvironment;
            _reportGeneratorBusiness = reportGeneratorBusiness;
            _elasticClient = client;
            _leftpanelMapping = leftPanelMapping;
            _configuration = config;

            var settings = new Dictionary<string, object> { { "search.max_buckets", 1000000 } };
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
            elasticSearchPracticeSlideCalculation = new ElasticSearchPracticeSlideCalculation(_elasticClient, _reportGeneratorBusiness, _leftpanelMapping,_configuration); 
        }
        [HttpGet]
        [Route("GetGeoLeftPanel")]
        public IActionResult GetGeoLeftPanel()
        {
            LeftPanel leftPanel = new LeftPanel();
            var file = ConstantPath.GetRootPath + @"Json\" + "leftPanel.json";
            using (StreamReader r = new StreamReader(file))
            {
                string data = r.ReadToEnd();
                LeftPanel _leftPanel = JsonConvert.DeserializeObject<LeftPanel>(data);
                leftPanel.geographyMenu = _leftPanel.geographyMenu;
                r.Close();
            }

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
            var obj = JsonConvert.SerializeObject(leftPanel);
            return new OkObjectResult(leftPanel);
        }

        [HttpPost]
        [Route("GetBenchmarkLeftPanel")]
        public IActionResult GetBenchmarkLeftPanel(BenchmarkPostModel benchmarkPost)
        {
            LeftPanel leftPanel = new LeftPanel();
            var file = ConstantPath.GetRootPath + @"Json\" + "leftPanel.json";
            using (StreamReader r = new StreamReader(file))
            {
                string tempdata = r.ReadToEnd();
                LeftPanel _leftPanel = JsonConvert.DeserializeObject<LeftPanel>(tempdata);

                leftPanel = filterData(_leftPanel, benchmarkPost.CountryId);
                r.Close();
            }
            return new OkObjectResult(leftPanel);
        }
        private LeftPanel filterData(LeftPanel leftPanel, string countryId)
        {
            LeftPanel _leftPanel = new LeftPanel();

            _leftPanel.timeperiodMenu = leftPanel.timeperiodMenu.Where(a => a.geography == countryId).ToList();

            _leftPanel.benchmarkMenu = new LeftPanelMenu();
            _leftPanel.benchmarkMenu.id = leftPanel.benchmarkMenu.id;
            _leftPanel.benchmarkMenu.name = leftPanel.benchmarkMenu.name;
            _leftPanel.benchmarkMenu.data = new List<PopupLevel>();
            foreach (var level in leftPanel.benchmarkMenu.data)
            {
                PopupLevel _level = new PopupLevel();
                _level.levelId = level.levelId;
                _level.levelName = level.levelName;
                _level.data = level.data.Where(e => e.GeographyId == countryId).ToList();
                _leftPanel.benchmarkMenu.data.Add(_level);
            }

            _leftPanel.comparisonMenu = new LeftPanelMenu();
            _leftPanel.comparisonMenu.id = leftPanel.comparisonMenu.id;
            _leftPanel.comparisonMenu.name = leftPanel.comparisonMenu.name;
            _leftPanel.comparisonMenu.data = new List<PopupLevel>();
            foreach (var level in leftPanel.comparisonMenu.data)
            {
                PopupLevel _level = new PopupLevel();
                _level.levelId = level.levelId;
                _level.levelName = level.levelName;
                _level.data = level.data.Where(e => e.GeographyId == countryId).ToList();
                _leftPanel.comparisonMenu.data.Add(_level);
            }

            _leftPanel.filterMenu = new LeftPanelMenu();
            _leftPanel.filterMenu.id = leftPanel.filterMenu.id;
            _leftPanel.filterMenu.name = leftPanel.filterMenu.name;
            _leftPanel.filterMenu.data = new List<PopupLevel>();
            foreach (var level in leftPanel.filterMenu.data)
            {
                PopupLevel _level = new PopupLevel();
                _level.levelId = level.levelId;
                _level.levelName = level.levelName;
                _level.data = level.data.Where(e => (level.levelId > 2 && e.GeographyId == countryId) || (e.type == "BeverageFilter" && level.levelId == 2 && e.GeographyId == countryId) || (e.type == "Filter" && level.levelId == 2) || level.levelId == 1).ToList();
                _leftPanel.filterMenu.data.Add(_level);
            }
            return _leftPanel;
        }
        [HttpPost]
        [Route("ElasticSearchPracticeSampleSize")]
        public IActionResult ElasticSearchPracticeSampleSize(LeftPanelRequest leftPanelRequest)
        {
            return elasticSearchPracticeSlideCalculation.ElasticSearchPracticeSampleSize(leftPanelRequest);
        }
        [HttpPost]
        [Route("ElasticSearchPractice")]
        public IActionResult ElasticSearchPracticeCalc(LeftPanelRequest leftPanelRequest)
        {
            return elasticSearchPracticeSlideCalculation.ElasticSearchPracticeCalc(leftPanelRequest);
        }
    }
}
